"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { StatusPill } from "@/components/StatusPill";
import { EmptyState } from "@/components/EmptyState";
import { api, ApiRequestError } from "@/lib/api";
import { AssignmentResponse } from "@/lib/types";

function isOverdue(deadline: string) {
  return new Date(deadline).getTime() < Date.now();
}

function StudentDashboard() {
  const [assignments, setAssignments] = useState<AssignmentResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<AssignmentResponse[]>("/api/assignments/my")
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiRequestError ? err.message : "Could not load assignments."));
  }, []);

  return (
    <AppShell role="Student" navItems={[{ href: "/student", label: "My assignments" }]}>
      <h1 className="font-serif text-2xl text-ink">My assignments</h1>
      <p className="mt-1 text-sm text-slate">Everything your teachers have published for your class.</p>

      {error && <p className="mt-6 rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

      {assignments && assignments.length === 0 && (
        <div className="mt-6">
          <EmptyState title="Nothing due yet" description="Published assignments for your class will appear here." />
        </div>
      )}

      <ul className="mt-6 space-y-3">
        {assignments?.map((a) => (
          <li key={a.id}>
            <Link
              href={`/student/assignments/${a.id}`}
              className="block rounded-lg border border-slate-light bg-white p-5 transition hover:border-gold"
            >
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="font-serif text-lg text-ink">{a.title}</p>
                  <p className="mt-1 text-sm text-slate">
                    {a.subjectName} · {a.maxMarks} marks
                  </p>
                </div>
                <div className="flex flex-col items-end gap-2">
                  {a.mySubmissionStatus ? (
                    <StatusPill status={a.mySubmissionStatus} />
                  ) : (
                    <StatusPill status={isOverdue(a.deadline) ? "Late" : "NotSubmitted"} />
                  )}
                  <span className="font-mono text-xs text-slate">
                    Due {new Date(a.deadline).toLocaleString()}
                  </span>
                </div>
              </div>
            </Link>
          </li>
        ))}
      </ul>
    </AppShell>
  );
}

export default function Page() {
  return (
    <RequireRole role="Student">
      <StudentDashboard />
    </RequireRole>
  );
}

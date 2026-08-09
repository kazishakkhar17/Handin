"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { StatusPill } from "@/components/StatusPill";
import { EmptyState } from "@/components/EmptyState";
import { api, ApiRequestError } from "@/lib/api";
import { AssignmentResponse } from "@/lib/types";

const NAV = [
  { href: "/teacher", label: "My assignments" },
  { href: "/teacher/assignments/new", label: "New assignment" },
];

function TeacherDashboard() {
  const [assignments, setAssignments] = useState<AssignmentResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<AssignmentResponse[]>("/api/assignments/teaching")
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiRequestError ? err.message : "Could not load assignments."));
  }, []);

  return (
    <AppShell role="Teacher" navItems={NAV}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-serif text-2xl text-ink">My assignments</h1>
          <p className="mt-1 text-sm text-slate">Assignments you've created, draft or published.</p>
        </div>
        <Link
          href="/teacher/assignments/new"
          className="rounded-md bg-sage px-4 py-2 text-sm font-medium text-paper transition hover:opacity-90"
        >
          + New assignment
        </Link>
      </div>

      {error && <p className="mt-6 rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

      {assignments && assignments.length === 0 && (
        <div className="mt-6">
          <EmptyState title="No assignments yet" description="Create your first assignment to get started." />
        </div>
      )}

      <ul className="mt-6 space-y-3">
        {assignments?.map((a) => (
          <li key={a.id}>
            <Link
              href={`/teacher/assignments/${a.id}`}
              className="block rounded-lg border border-slate-light bg-white p-5 transition hover:border-sage"
            >
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="font-serif text-lg text-ink">{a.title}</p>
                  <p className="mt-1 text-sm text-slate">
                    {a.subjectName} · {a.classCourseName} · {a.maxMarks} marks
                  </p>
                </div>
                <div className="flex flex-col items-end gap-2">
                  <StatusPill status={a.status} />
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
    <RequireRole role="Teacher">
      <TeacherDashboard />
    </RequireRole>
  );
}

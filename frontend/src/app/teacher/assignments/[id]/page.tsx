"use client";

import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { StatusPill } from "@/components/StatusPill";
import { EmptyState } from "@/components/EmptyState";
import { api, ApiRequestError } from "@/lib/api";
import { AssignmentResponse, SubmissionResponse, SubmissionStatus } from "@/lib/types";

const NAV = [
  { href: "/teacher", label: "My assignments" },
  { href: "/teacher/assignments/new", label: "New assignment" },
];

const STATUS_OPTIONS: SubmissionStatus[] = ["Submitted", "Late", "Graded", "ReturnedForRevision"];

function GradeRow({
  submission,
  maxMarks,
  onGraded,
}: {
  submission: SubmissionResponse;
  maxMarks: number;
  onGraded: (updated: SubmissionResponse) => void;
}) {
  const [expanded, setExpanded] = useState(false);
  const [marks, setMarks] = useState(submission.marksAwarded ?? 0);
  const [feedback, setFeedback] = useState(submission.feedback ?? "");
  const [status, setStatus] = useState<SubmissionStatus>(submission.status);
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);
  const [statusError, setStatusError] = useState<string | null>(null);

  async function handleGrade() {
    setError(null);
    setIsSaving(true);
    try {
      const updated = await api.post<SubmissionResponse>(
        `/api/assignments/${submission.assignmentId}/submissions/${submission.id}/grade`,
        { marksAwarded: marks, feedback }
      );
      onGraded(updated);
      setStatus(updated.status);
      setExpanded(false);
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not save the grade.");
    } finally {
      setIsSaving(false);
    }
  }

  async function handleStatusChange() {
    setStatusError(null);
    setIsUpdatingStatus(true);
    try {
      const updated = await api.patch<SubmissionResponse>(
        `/api/assignments/${submission.assignmentId}/submissions/${submission.id}/status`,
        { status }
      );
      onGraded(updated);
    } catch (err) {
      setStatusError(err instanceof ApiRequestError ? err.message : "Could not update the status.");
    } finally {
      setIsUpdatingStatus(false);
    }
  }

  return (
    <li className="rounded-lg border border-slate-light bg-white p-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="font-medium text-ink">{submission.studentName}</p>
          <p className="mt-0.5 font-mono text-xs text-slate">
            Submitted {new Date(submission.submittedAt).toLocaleString()}
            {submission.lastUpdatedAt && ` · updated ${new Date(submission.lastUpdatedAt).toLocaleString()}`}
          </p>
        </div>
        <div className="flex items-center gap-3">
          {submission.status === "Graded" && (
            <span className="font-mono text-sm text-ink">
              {submission.marksAwarded}/{maxMarks}
            </span>
          )}
          <StatusPill status={submission.status} />
        </div>
      </div>

      <button
        onClick={() => setExpanded((v) => !v)}
        className="mt-3 text-sm text-sage underline underline-offset-2"
      >
        {expanded ? "Hide answer" : "View answer & grade"}
      </button>

      {expanded && (
        <div className="mt-4 space-y-4 border-t border-slate-light pt-4">
          <p className="whitespace-pre-wrap rounded-md bg-paper-dim p-3 text-sm text-ink">
            {submission.answerText}
          </p>

          <div className="grid grid-cols-[120px_1fr] gap-4">
            <div>
              <label className="block text-sm font-medium text-ink">Marks</label>
              <input
                type="number"
                min={0}
                max={maxMarks}
                value={marks}
                onChange={(e) => setMarks(Number(e.target.value))}
                className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
              />
              <p className="mt-1 text-xs text-slate">out of {maxMarks}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-ink">Feedback</label>
              <textarea
                rows={3}
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
                className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
              />
            </div>
          </div>

          {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

          <button
            onClick={handleGrade}
            disabled={isSaving}
            className="rounded-md bg-sage px-4 py-2 text-sm font-medium text-paper transition hover:opacity-90 disabled:opacity-60"
          >
            {isSaving ? "Saving…" : "Save grade"}
          </button>

          <div className="flex items-end gap-3 border-t border-slate-light pt-4">
            <div>
              <label className="block text-sm font-medium text-ink">Submission status</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value as SubmissionStatus)}
                className="mt-1.5 rounded-md border border-slate-light bg-white px-3 py-2 text-sm text-ink shadow-sm focus:border-sage"
              >
                {STATUS_OPTIONS.map((s) => (
                  <option key={s} value={s}>
                    {s.replace(/([a-z])([A-Z])/g, "$1 $2")}
                  </option>
                ))}
              </select>
            </div>
            <button
              onClick={handleStatusChange}
              disabled={isUpdatingStatus || status === submission.status}
              className="rounded-md border border-slate-light px-4 py-2 text-sm font-medium text-ink transition hover:border-sage disabled:opacity-60"
            >
              {isUpdatingStatus ? "Updating…" : "Update status"}
            </button>
          </div>
          {statusError && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{statusError}</p>}
        </div>
      )}
    </li>
  );
}

function TeacherAssignmentDetail() {
  const params = useParams();
  const assignmentId = params.id as string;

  const [assignment, setAssignment] = useState<AssignmentResponse | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      const [a, subs] = await Promise.all([
        api.get<AssignmentResponse>(`/api/assignments/${assignmentId}`),
        api.get<SubmissionResponse[]>(`/api/assignments/${assignmentId}/submissions`),
      ]);
      setAssignment(a);
      setSubmissions(subs);
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not load this assignment.");
    }
  }, [assignmentId]);

  useEffect(() => {
    load();
  }, [load]);

  function handleGraded(updated: SubmissionResponse) {
    setSubmissions((prev) => prev?.map((s) => (s.id === updated.id ? updated : s)) ?? null);
  }

  return (
    <AppShell role="Teacher" navItems={NAV}>
      <Link href="/teacher" className="text-sm text-slate hover:text-ink">
        ← Back to my assignments
      </Link>

      {error && <p className="mt-4 rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

      {assignment && (
        <div className="mt-4">
          <div className="flex items-start justify-between gap-4">
            <div>
              <h1 className="font-serif text-2xl text-ink">{assignment.title}</h1>
              <p className="mt-1 text-sm text-slate">
                {assignment.subjectName} · {assignment.classCourseName} · {assignment.maxMarks} marks
              </p>
            </div>
            <StatusPill status={assignment.status} />
          </div>

          <p className="mt-3 font-mono text-sm text-slate">
            Deadline: {new Date(assignment.deadline).toLocaleString()}
          </p>

          <h2 className="mt-8 font-serif text-xl text-ink">Submissions</h2>

          {submissions && submissions.length === 0 && (
            <div className="mt-4">
              <EmptyState title="No submissions yet" description="Student submissions will appear here as they come in." />
            </div>
          )}

          <ul className="mt-4 space-y-3">
            {submissions?.map((s) => (
              <GradeRow key={s.id} submission={s} maxMarks={assignment.maxMarks} onGraded={handleGraded} />
            ))}
          </ul>
        </div>
      )}
    </AppShell>
  );
}

export default function Page() {
  return (
    <RequireRole role="Teacher">
      <TeacherAssignmentDetail />
    </RequireRole>
  );
}

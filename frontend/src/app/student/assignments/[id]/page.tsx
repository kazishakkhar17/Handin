"use client";

import { useEffect, useState, useCallback } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { StatusPill } from "@/components/StatusPill";
import { api, ApiRequestError } from "@/lib/api";
import { AssignmentResponse, SubmissionResponse } from "@/lib/types";

function StudentAssignmentDetail() {
  const params = useParams();
  const assignmentId = params.id as string;

  const [assignment, setAssignment] = useState<AssignmentResponse | null>(null);
  const [submission, setSubmission] = useState<SubmissionResponse | null>(null);
  const [answerText, setAnswerText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const load = useCallback(async () => {
    setError(null);
    try {
      const a = await api.get<AssignmentResponse>(`/api/assignments/${assignmentId}`);
      setAssignment(a);
      try {
        const s = await api.get<SubmissionResponse>(`/api/assignments/${assignmentId}/submissions/mine`);
        setSubmission(s);
        setAnswerText(s.answerText);
      } catch (err) {
        if (!(err instanceof ApiRequestError && err.status === 404)) throw err;
      }
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not load this assignment.");
    }
  }, [assignmentId]);

  useEffect(() => {
    load();
  }, [load]);

  const deadlinePassed = assignment ? new Date(assignment.deadline).getTime() < Date.now() : false;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setNotice(null);
    setIsSubmitting(true);
    try {
      if (submission) {
        await api.put(`/api/assignments/${assignmentId}/submissions`, { answerText });
        setNotice("Your submission was updated.");
      } else {
        await api.post(`/api/assignments/${assignmentId}/submissions`, { answerText });
        setNotice("Your answer was submitted.");
      }
      await load();
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not save your submission.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (error && !assignment) {
    return (
      <AppShell role="Student" navItems={[{ href: "/student", label: "My assignments" }]}>
        <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>
      </AppShell>
    );
  }

  const canEdit =
    !deadlinePassed &&
    (!submission || (assignment?.allowResubmission && submission.status !== "Graded"));

  return (
    <AppShell role="Student" navItems={[{ href: "/student", label: "My assignments" }]}>
      <Link href="/student" className="text-sm text-slate hover:text-ink">
        ← Back to my assignments
      </Link>

      {assignment && (
        <div className="mt-4">
          <div className="flex items-start justify-between gap-4">
            <div>
              <h1 className="font-serif text-2xl text-ink">{assignment.title}</h1>
              <p className="mt-1 text-sm text-slate">
                {assignment.subjectName} · {assignment.teacherName} · {assignment.maxMarks} marks
              </p>
            </div>
            {submission && <StatusPill status={submission.status} />}
          </div>

          <p className="mt-4 whitespace-pre-wrap text-ink">{assignment.description}</p>

          <p className={`mt-4 font-mono text-sm ${deadlinePassed ? "text-brick" : "text-slate"}`}>
            Deadline: {new Date(assignment.deadline).toLocaleString()}
            {deadlinePassed && " — passed"}
          </p>

          {submission?.status === "Graded" && (
            <div className="mt-6 rounded-lg border border-sage-light bg-sage-light/40 p-4">
              <p className="font-serif text-lg text-ink">
                Marks: {submission.marksAwarded} / {assignment.maxMarks}
              </p>
              {submission.feedback && <p className="mt-2 text-sm text-ink">{submission.feedback}</p>}
            </div>
          )}

          <form onSubmit={handleSubmit} className="mt-6">
            <label htmlFor="answer" className="block text-sm font-medium text-ink">
              Your answer
            </label>
            <textarea
              id="answer"
              required
              rows={10}
              disabled={!canEdit}
              value={answerText}
              onChange={(e) => setAnswerText(e.target.value)}
              className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-gold disabled:bg-paper-dim disabled:text-slate"
            />

            {notice && <p className="mt-3 rounded-md bg-sage-light px-3 py-2 text-sm text-sage">{notice}</p>}
            {error && <p className="mt-3 rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

            {canEdit ? (
              <button
                type="submit"
                disabled={isSubmitting}
                className="mt-4 rounded-md bg-ink px-4 py-2.5 font-medium text-paper transition hover:bg-ink-light disabled:opacity-60"
              >
                {isSubmitting ? "Saving…" : submission ? "Update submission" : "Submit answer"}
              </button>
            ) : (
              <p className="mt-4 text-sm text-slate">
                {deadlinePassed
                  ? "The deadline has passed — this can no longer be changed."
                  : submission?.status === "Graded"
                  ? "This submission has been graded and can no longer be changed."
                  : "Resubmission is not allowed for this assignment."}
              </p>
            )}
          </form>
        </div>
      )}
    </AppShell>
  );
}

export default function Page() {
  return (
    <RequireRole role="Student">
      <StudentAssignmentDetail />
    </RequireRole>
  );
}

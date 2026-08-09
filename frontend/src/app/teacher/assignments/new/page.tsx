"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { api, ApiRequestError } from "@/lib/api";
import { AssignmentResponse, TeachingAssignmentResponse } from "@/lib/types";

const NAV = [
  { href: "/teacher", label: "My assignments" },
  { href: "/teacher/assignments/new", label: "New assignment" },
];

function NewAssignmentForm() {
  const router = useRouter();
  const [options, setOptions] = useState<TeachingAssignmentResponse[] | null>(null);
  const [selectedKey, setSelectedKey] = useState("");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [deadline, setDeadline] = useState("");
  const [maxMarks, setMaxMarks] = useState(100);
  const [allowResubmission, setAllowResubmission] = useState(true);
  const [publishNow, setPublishNow] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    api
      .get<TeachingAssignmentResponse[]>("/api/users/me/teaching-assignments")
      .then((data) => {
        setOptions(data);
        if (data.length > 0) setSelectedKey(`${data[0].subjectId}:${data[0].classCourseId}`);
      })
      .catch((err) => setError(err instanceof ApiRequestError ? err.message : "Could not load your subjects."));
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    const [subjectId, classCourseId] = selectedKey.split(":").map(Number);
    if (!subjectId || !classCourseId) {
      setError("Please select a subject/class to assign this to.");
      return;
    }

    setIsSubmitting(true);
    try {
      const created = await api.post<AssignmentResponse>("/api/assignments", {
        title,
        description,
        subjectId,
        classCourseId,
        deadline: new Date(deadline).toISOString(),
        maxMarks,
        allowResubmission,
        status: publishNow ? "Published" : "Draft",
      });
      router.push(`/teacher/assignments/${created.id}`);
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create the assignment.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AppShell role="Teacher" navItems={NAV}>
      <Link href="/teacher" className="text-sm text-slate hover:text-ink">
        ← Back to my assignments
      </Link>

      <h1 className="mt-4 font-serif text-2xl text-ink">New assignment</h1>

      {options && options.length === 0 && (
        <p className="mt-4 rounded-md bg-brick-light px-3 py-2 text-sm text-brick">
          You haven't been assigned to teach any subject/class yet. Ask your admin to assign you first.
        </p>
      )}

      <form onSubmit={handleSubmit} className="mt-6 max-w-xl space-y-5">
        <div>
          <label htmlFor="subjectClass" className="block text-sm font-medium text-ink">
            Subject &amp; class
          </label>
          <select
            id="subjectClass"
            required
            value={selectedKey}
            onChange={(e) => setSelectedKey(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
          >
            {options?.map((o) => (
              <option key={`${o.subjectId}:${o.classCourseId}`} value={`${o.subjectId}:${o.classCourseId}`}>
                {o.subjectName} — {o.classCourseName}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="title" className="block text-sm font-medium text-ink">
            Title
          </label>
          <input
            id="title"
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
          />
        </div>

        <div>
          <label htmlFor="description" className="block text-sm font-medium text-ink">
            Description
          </label>
          <textarea
            id="description"
            required
            rows={5}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label htmlFor="deadline" className="block text-sm font-medium text-ink">
              Deadline
            </label>
            <input
              id="deadline"
              type="datetime-local"
              required
              value={deadline}
              onChange={(e) => setDeadline(e.target.value)}
              className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
            />
          </div>
          <div>
            <label htmlFor="maxMarks" className="block text-sm font-medium text-ink">
              Max marks
            </label>
            <input
              id="maxMarks"
              type="number"
              min={1}
              required
              value={maxMarks}
              onChange={(e) => setMaxMarks(Number(e.target.value))}
              className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-sage"
            />
          </div>
        </div>

        <label className="flex items-center gap-2 text-sm text-ink">
          <input
            type="checkbox"
            checked={allowResubmission}
            onChange={(e) => setAllowResubmission(e.target.checked)}
            className="rounded border-slate-light text-sage focus:ring-sage"
          />
          Allow students to update their submission before the deadline
        </label>

        <label className="flex items-center gap-2 text-sm text-ink">
          <input
            type="checkbox"
            checked={publishNow}
            onChange={(e) => setPublishNow(e.target.checked)}
            className="rounded border-slate-light text-sage focus:ring-sage"
          />
          Publish immediately (otherwise saved as a draft)
        </label>

        {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

        <button
          type="submit"
          disabled={isSubmitting || !options?.length}
          className="rounded-md bg-sage px-4 py-2.5 font-medium text-paper transition hover:opacity-90 disabled:opacity-60"
        >
          {isSubmitting ? "Creating…" : "Create assignment"}
        </button>
      </form>
    </AppShell>
  );
}

export default function Page() {
  return (
    <RequireRole role="Teacher">
      <NewAssignmentForm />
    </RequireRole>
  );
}

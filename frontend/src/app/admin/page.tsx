"use client";

import { FormEvent, useEffect, useState } from "react";
import { RequireRole } from "@/components/RequireRole";
import { AppShell } from "@/components/AppShell";
import { RoleBadge } from "@/components/RoleBadge";
import { StatusPill } from "@/components/StatusPill";
import { api, ApiRequestError } from "@/lib/api";
import {
  AssignmentResponse,
  ClassCourseResponse,
  SubjectResponse,
  SubmissionResponse,
  UserResponse,
  UserRole,
} from "@/lib/types";
import { ClassCourseSelect, SubjectSelect } from "@/components/CreatableSelects";

const NAV = [{ href: "/admin", label: "Overview" }];

type Tab = "users" | "academic" | "assign" | "activity";

function AdminDashboard() {
  const [tab, setTab] = useState<Tab>("users");
  const [presetRole, setPresetRole] = useState<UserRole | null>(null);

  return (
    <AppShell role="Admin" navItems={NAV}>
      <h1 className="font-serif text-2xl text-ink">Admin overview</h1>
      <p className="mt-1 text-sm text-slate">Manage people, classes, subjects, and teaching assignments.</p>

      <div className="mt-6 flex gap-1 overflow-x-auto border-b border-slate-light">
        {(["users", "academic", "assign", "activity"] as Tab[]).map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`shrink-0 whitespace-nowrap px-4 py-2 text-sm font-medium ${
              tab === t ? "border-b-2 border-ink text-ink" : "text-slate hover:text-ink"
            }`}
          >
            {t === "users"
              ? "Users"
              : t === "academic"
              ? "Classes & subjects"
              : t === "assign"
              ? "Assign teachers"
              : "Assignments & submissions"}
          </button>
        ))}
      </div>

      <div className="mt-6 min-w-0">
        {tab === "users" && (
          <UsersTab presetRole={presetRole} onPresetRoleConsumed={() => setPresetRole(null)} />
        )}
        {tab === "academic" && <AcademicTab />}
        {tab === "assign" && (
          <AssignTab
            onCreateTeacher={() => {
              setPresetRole("Teacher");
              setTab("users");
            }}
          />
        )}
        {tab === "activity" && <ActivityTab />}
      </div>
    </AppShell>
  );
}

// ---------------------------------------------------------------------------
// Users tab
// ---------------------------------------------------------------------------
function UsersTab({
  presetRole,
  onPresetRoleConsumed,
}: {
  presetRole?: UserRole | null;
  onPresetRoleConsumed?: () => void;
}) {
  const [users, setUsers] = useState<UserResponse[] | null>(null);
  const [classes, setClasses] = useState<ClassCourseResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<UserRole>(presetRole ?? "Student");
  const [classCourseId, setClassCourseId] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  function loadUsers() {
    api.get<UserResponse[]>("/api/users").then(setUsers).catch(() => setError("Could not load users."));
  }

  useEffect(() => {
    loadUsers();
    api.get<ClassCourseResponse[]>("/api/classes").then(setClasses).catch(() => {});
  }, []);

  useEffect(() => {
    if (presetRole) onPresetRoleConsumed?.();
    // Only meant to fire once, right after this tab mounts with an incoming preset role.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setNotice(null);
    setIsSubmitting(true);
    try {
      await api.post("/api/users", {
        fullName,
        email,
        password,
        role,
        classCourseId: role === "Student" ? Number(classCourseId) : undefined,
      });
      setNotice(`${fullName} was created.`);
      setFullName("");
      setEmail("");
      setPassword("");
      loadUsers();
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create the user.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function toggleActive(user: UserResponse) {
    try {
      if (user.isActive) {
        await api.delete(`/api/users/${user.id}`);
      } else {
        await api.put(`/api/users/${user.id}`, { isActive: true });
      }
      loadUsers();
    } catch {
      setError("Could not update this user.");
    }
  }

  return (
    <div className="grid gap-8 lg:grid-cols-[320px_1fr]">
      <form onSubmit={handleCreate} className="min-w-0 space-y-4 rounded-lg border border-slate-light bg-white p-5">
        <h2 className="font-serif text-lg text-ink">Create user</h2>

        <div>
          <label className="block text-sm font-medium text-ink">Full name</label>
          <input
            required
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-ink">Email</label>
          <input
            required
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-ink">Password</label>
          <input
            required
            type="password"
            minLength={6}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-ink">Role</label>
          <select
            value={role}
            onChange={(e) => setRole(e.target.value as UserRole)}
            className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          >
            <option value="Admin">Admin</option>
            <option value="Teacher">Teacher</option>
            <option value="Student">Student</option>
          </select>
        </div>
        {role === "Student" && (
          <div>
            <label className="block text-sm font-medium text-ink">Class / course</label>
            <ClassCourseSelect
              required
              classes={classes ?? []}
              value={classCourseId}
              onChange={setClassCourseId}
              onCreated={(c) => setClasses((prev) => [...(prev ?? []), c])}
            />
          </div>
        )}

        {notice && <p className="rounded-md bg-sage-light px-3 py-2 text-sm text-sage">{notice}</p>}
        {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper hover:bg-ink-light disabled:opacity-60"
        >
          {isSubmitting ? "Creating…" : "Create user"}
        </button>
      </form>

      <div className="min-w-0 overflow-x-auto rounded-lg border border-slate-light bg-white">
        <table className="w-full text-left text-sm">
          <thead className="bg-paper-dim text-xs uppercase tracking-wide text-slate">
            <tr>
              <th className="px-4 py-3">Name</th>
              <th className="px-4 py-3">Email</th>
              <th className="px-4 py-3">Role</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-light">
            {users?.map((u) => (
              <tr key={u.id}>
                <td className="px-4 py-3 text-ink">{u.fullName}</td>
                <td className="px-4 py-3 font-mono text-xs text-slate">{u.email}</td>
                <td className="px-4 py-3">
                  <RoleBadge role={u.role} />
                </td>
                <td className="px-4 py-3 text-slate">{u.isActive ? "Active" : "Inactive"}</td>
                <td className="px-4 py-3 text-right">
                  <button onClick={() => toggleActive(u)} className="text-sm text-slate underline hover:text-ink">
                    {u.isActive ? "Deactivate" : "Reactivate"}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Classes & Subjects tab
// ---------------------------------------------------------------------------
function AcademicTab() {
  const [classes, setClasses] = useState<ClassCourseResponse[] | null>(null);
  const [subjectsByClass, setSubjectsByClass] = useState<Record<number, SubjectResponse[]>>({});
  const [className, setClassName] = useState("");
  const [subjectName, setSubjectName] = useState("");
  const [subjectClassId, setSubjectClassId] = useState("");
  const [error, setError] = useState<string | null>(null);

  function loadClasses() {
    api.get<ClassCourseResponse[]>("/api/classes").then((data) => {
      setClasses(data);
      data.forEach((c) => {
        api
          .get<SubjectResponse[]>(`/api/subjects?classCourseId=${c.id}`)
          .then((subs) => setSubjectsByClass((prev) => ({ ...prev, [c.id]: subs })));
      });
    });
  }

  useEffect(loadClasses, []);

  async function handleCreateClass(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await api.post("/api/classes", { name: className });
      setClassName("");
      loadClasses();
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create class/course.");
    }
  }

  async function handleCreateSubject(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await api.post("/api/subjects", { name: subjectName, classCourseId: Number(subjectClassId) });
      setSubjectName("");
      loadClasses();
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create subject.");
    }
  }

  return (
    <div className="grid gap-8 lg:grid-cols-2">
      <div className="space-y-6">
        <form onSubmit={handleCreateClass} className="space-y-3 rounded-lg border border-slate-light bg-white p-5">
          <h2 className="font-serif text-lg text-ink">New class / course</h2>
          <input
            required
            placeholder="e.g. Grade 10 - Section B"
            value={className}
            onChange={(e) => setClassName(e.target.value)}
            className="w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          />
          <button type="submit" className="rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper hover:bg-ink-light">
            Create
          </button>
        </form>

        <form onSubmit={handleCreateSubject} className="space-y-3 rounded-lg border border-slate-light bg-white p-5">
          <h2 className="font-serif text-lg text-ink">New subject</h2>
          <ClassCourseSelect
            required
            classes={classes ?? []}
            value={subjectClassId}
            onChange={setSubjectClassId}
            onCreated={(c) => setClasses((prev) => (prev ? [...prev, c] : [c]))}
          />
          <input
            required
            placeholder="e.g. Mathematics"
            value={subjectName}
            onChange={(e) => setSubjectName(e.target.value)}
            className="w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
          />
          <button type="submit" className="rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper hover:bg-ink-light">
            Create
          </button>
        </form>

        {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}
      </div>

      <div className="space-y-4">
        {classes?.map((c) => (
          <div key={c.id} className="rounded-lg border border-slate-light bg-white p-5">
            <p className="font-serif text-lg text-ink">{c.name}</p>
            <ul className="mt-2 space-y-1">
              {(subjectsByClass[c.id] ?? []).map((s) => (
                <li key={s.id} className="text-sm text-slate">
                  · {s.name}
                </li>
              ))}
              {(subjectsByClass[c.id] ?? []).length === 0 && (
                <li className="text-sm text-slate">No subjects yet</li>
              )}
            </ul>
          </div>
        ))}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Assign teachers tab
// ---------------------------------------------------------------------------
const ADD_TEACHER = "__add_teacher__";

function AssignTab({ onCreateTeacher }: { onCreateTeacher: () => void }) {
  const [teachers, setTeachers] = useState<UserResponse[] | null>(null);
  const [classes, setClasses] = useState<ClassCourseResponse[] | null>(null);
  const [subjects, setSubjects] = useState<SubjectResponse[]>([]);
  const [teacherId, setTeacherId] = useState("");
  const [classCourseId, setClassCourseId] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  useEffect(() => {
    api.get<UserResponse[]>("/api/users?role=Teacher").then(setTeachers).catch(() => {});
    api.get<ClassCourseResponse[]>("/api/classes").then(setClasses).catch(() => {});
  }, []);

  useEffect(() => {
    if (!classCourseId) {
      setSubjects([]);
      return;
    }
    api.get<SubjectResponse[]>(`/api/subjects?classCourseId=${classCourseId}`).then(setSubjects);
  }, [classCourseId]);

  async function handleAssign(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setNotice(null);
    try {
      await api.post("/api/users/assign-teacher", {
        teacherId: Number(teacherId),
        subjectId: Number(subjectId),
        classCourseId: Number(classCourseId),
      });
      setNotice("Teacher assigned.");
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not assign this teacher.");
    }
  }

  return (
    <form onSubmit={handleAssign} className="max-w-md space-y-4 rounded-lg border border-slate-light bg-white p-5">
      <h2 className="font-serif text-lg text-ink">Assign a teacher to a subject</h2>

      <div>
        <label className="block text-sm font-medium text-ink">Teacher</label>
        <select
          required
          value={teacherId}
          onChange={(e) => {
            if (e.target.value === ADD_TEACHER) {
              onCreateTeacher();
              return;
            }
            setTeacherId(e.target.value);
          }}
          className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
        >
          <option value="">Select…</option>
          {teachers?.map((t) => (
            <option key={t.id} value={t.id}>
              {t.fullName}
            </option>
          ))}
          <option value={ADD_TEACHER}>+ Add new teacher…</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-ink">Class / course</label>
        <ClassCourseSelect
          required
          classes={classes ?? []}
          value={classCourseId}
          onChange={(id) => {
            setClassCourseId(id);
            setSubjectId("");
          }}
          onCreated={(c) => setClasses((prev) => [...(prev ?? []), c])}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-ink">Subject</label>
        <SubjectSelect
          required
          classCourseId={classCourseId}
          subjects={subjects}
          value={subjectId}
          onChange={setSubjectId}
          onCreated={(s) => setSubjects((prev) => [...prev, s])}
          disabled={!classCourseId}
        />
      </div>

      {notice && <p className="rounded-md bg-sage-light px-3 py-2 text-sm text-sage">{notice}</p>}
      {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

      <button type="submit" className="rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper hover:bg-ink-light">
        Assign
      </button>
    </form>
  );
}

// ---------------------------------------------------------------------------
// Assignments & submissions tab (read-only, system-wide view for Admin)
// ---------------------------------------------------------------------------
function ActivityTab() {
  const [assignments, setAssignments] = useState<AssignmentResponse[] | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<AssignmentResponse[]>("/api/admin/assignments")
      .then(setAssignments)
      .catch(() => setError("Could not load assignments."));
    api
      .get<SubmissionResponse[]>("/api/admin/submissions")
      .then(setSubmissions)
      .catch(() => setError("Could not load submissions."));
  }, []);

  return (
    <div className="space-y-8">
      {error && <p className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">{error}</p>}

      <div>
        <h2 className="font-serif text-lg text-ink">All assignments</h2>
        <div className="mt-3 min-w-0 overflow-x-auto rounded-lg border border-slate-light bg-white">
          <table className="w-full text-left text-sm">
            <thead className="bg-paper-dim text-xs uppercase tracking-wide text-slate">
              <tr>
                <th className="px-4 py-3">Title</th>
                <th className="px-4 py-3">Subject</th>
                <th className="px-4 py-3">Class</th>
                <th className="px-4 py-3">Teacher</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Deadline</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-light">
              {assignments?.map((a) => (
                <tr key={a.id}>
                  <td className="px-4 py-3 text-ink">{a.title}</td>
                  <td className="px-4 py-3 text-slate">{a.subjectName}</td>
                  <td className="px-4 py-3 text-slate">{a.classCourseName}</td>
                  <td className="px-4 py-3 text-slate">{a.teacherName}</td>
                  <td className="px-4 py-3">
                    <StatusPill status={a.status} />
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-slate">
                    {new Date(a.deadline).toLocaleString()}
                  </td>
                </tr>
              ))}
              {assignments?.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-6 text-center text-sm text-slate">
                    No assignments yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div>
        <h2 className="font-serif text-lg text-ink">All submissions</h2>
        <div className="mt-3 min-w-0 overflow-x-auto rounded-lg border border-slate-light bg-white">
          <table className="w-full text-left text-sm">
            <thead className="bg-paper-dim text-xs uppercase tracking-wide text-slate">
              <tr>
                <th className="px-4 py-3">Student</th>
                <th className="px-4 py-3">Assignment</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Marks</th>
                <th className="px-4 py-3">Submitted</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-light">
              {submissions?.map((s) => (
                <tr key={s.id}>
                  <td className="px-4 py-3 text-ink">{s.studentName}</td>
                  <td className="px-4 py-3 text-slate">{s.assignmentTitle}</td>
                  <td className="px-4 py-3">
                    <StatusPill status={s.status} />
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-slate">
                    {s.marksAwarded ?? "—"}
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-slate">
                    {new Date(s.submittedAt).toLocaleString()}
                  </td>
                </tr>
              ))}
              {submissions?.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-sm text-slate">
                    No submissions yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default function Page() {
  return (
    <RequireRole role="Admin">
      <AdminDashboard />
    </RequireRole>
  );
}

"use client";

import { useState } from "react";
import { api, ApiRequestError } from "@/lib/api";
import { ClassCourseResponse, SubjectResponse } from "@/lib/types";

const CREATE_NEW = "__create_new__";

function InlineCreateField({
  placeholder,
  isSubmitting,
  error,
  onAdd,
  onCancel,
}: {
  placeholder: string;
  isSubmitting: boolean;
  error: string | null;
  onAdd: (name: string) => void;
  onCancel: () => void;
}) {
  const [name, setName] = useState("");

  return (
    <div className="mt-1.5 space-y-2">
      <div className="flex gap-2">
        <input
          autoFocus
          placeholder={placeholder}
          value={name}
          onChange={(e) => setName(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter") {
              e.preventDefault();
              if (name.trim()) onAdd(name.trim());
            }
          }}
          className="w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink"
        />
        <button
          type="button"
          disabled={isSubmitting || !name.trim()}
          onClick={() => onAdd(name.trim())}
          className="shrink-0 rounded-md bg-ink px-3 py-2 text-sm font-medium text-paper hover:bg-ink-light disabled:opacity-60"
        >
          {isSubmitting ? "Adding…" : "Add"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="shrink-0 rounded-md border border-slate-light px-3 py-2 text-sm text-slate hover:text-ink"
        >
          Cancel
        </button>
      </div>
      {error && <p className="text-sm text-brick">{error}</p>}
    </div>
  );
}

export function ClassCourseSelect({
  classes,
  value,
  onChange,
  onCreated,
  required,
  disabled,
}: {
  classes: ClassCourseResponse[];
  value: string;
  onChange: (id: string) => void;
  onCreated: (newClass: ClassCourseResponse) => void;
  required?: boolean;
  disabled?: boolean;
}) {
  const [creating, setCreating] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleAdd(name: string) {
    setIsSubmitting(true);
    setError(null);
    try {
      const created = await api.post<ClassCourseResponse>("/api/classes", { name });
      onCreated(created);
      onChange(String(created.id));
      setCreating(false);
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create class/course.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (creating) {
    return (
      <InlineCreateField
        placeholder="e.g. Grade 10 - Section B"
        isSubmitting={isSubmitting}
        error={error}
        onAdd={handleAdd}
        onCancel={() => {
          setCreating(false);
          setError(null);
        }}
      />
    );
  }

  return (
    <select
      required={required}
      disabled={disabled}
      value={value}
      onChange={(e) => {
        if (e.target.value === CREATE_NEW) {
          setCreating(true);
          return;
        }
        onChange(e.target.value);
      }}
      className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink disabled:bg-paper-dim"
    >
      <option value="">Select…</option>
      {classes.map((c) => (
        <option key={c.id} value={c.id}>
          {c.name}
        </option>
      ))}
      <option value={CREATE_NEW}>+ Create new class…</option>
    </select>
  );
}

export function SubjectSelect({
  subjects,
  classCourseId,
  value,
  onChange,
  onCreated,
  required,
  disabled,
}: {
  subjects: SubjectResponse[];
  classCourseId: string;
  value: string;
  onChange: (id: string) => void;
  onCreated: (newSubject: SubjectResponse) => void;
  required?: boolean;
  disabled?: boolean;
}) {
  const [creating, setCreating] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleAdd(name: string) {
    setIsSubmitting(true);
    setError(null);
    try {
      const created = await api.post<SubjectResponse>("/api/subjects", {
        name,
        classCourseId: Number(classCourseId),
      });
      onCreated(created);
      onChange(String(created.id));
      setCreating(false);
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not create subject.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (creating) {
    return (
      <InlineCreateField
        placeholder="e.g. Mathematics"
        isSubmitting={isSubmitting}
        error={error}
        onAdd={handleAdd}
        onCancel={() => {
          setCreating(false);
          setError(null);
        }}
      />
    );
  }

  return (
    <select
      required={required}
      disabled={disabled}
      value={value}
      onChange={(e) => {
        if (e.target.value === CREATE_NEW) {
          setCreating(true);
          return;
        }
        onChange(e.target.value);
      }}
      className="mt-1.5 w-full rounded-md border border-slate-light px-3 py-2 text-sm focus:border-ink disabled:bg-paper-dim"
    >
      <option value="">Select…</option>
      {subjects.map((s) => (
        <option key={s.id} value={s.id}>
          {s.name}
        </option>
      ))}
      {classCourseId && <option value={CREATE_NEW}>+ Create new subject…</option>}
    </select>
  );
}

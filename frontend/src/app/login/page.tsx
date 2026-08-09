"use client";

import { FormEvent, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { useRouter } from "next/navigation";
import { ApiRequestError } from "@/lib/api";

const ROLE_HOME: Record<string, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
};

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const user = await login(email, password);
      router.push(ROLE_HOME[user.role] ?? "/");
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : "Could not connect to the server.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/* Left: brand panel with the role legend — the recurring signature device used
          throughout the app (a colored tag per role) is introduced here first. */}
      <div className="hidden flex-col justify-between bg-ink p-12 text-paper lg:flex">
        <div>
          <p className="font-mono text-xs uppercase tracking-[0.2em] text-gold">
            Assignment &amp; Submission Management
          </p>
          <h1 className="mt-6 max-w-md font-serif text-4xl leading-tight">
            Coursework, submissions, and marks — kept in one place.
          </h1>
        </div>

        <dl className="space-y-4 text-sm">
          <RoleLegendRow color="bg-gold" label="Student" desc="View assignments, submit answers, track marks" />
          <RoleLegendRow color="bg-sage" label="Teacher" desc="Create assignments, review work, give feedback" />
          <RoleLegendRow color="bg-paper" label="Admin" desc="Manage people, classes, and subjects" />
        </dl>
      </div>

      {/* Right: login form */}
      <div className="flex items-center justify-center p-8">
        <div className="w-full max-w-sm">
          <h2 className="font-serif text-2xl text-ink">Sign in</h2>
          <p className="mt-1 text-sm text-slate">Use the account your administrator gave you.</p>

          <form onSubmit={handleSubmit} className="mt-8 space-y-5">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-ink">
                Email
              </label>
              <input
                id="email"
                type="email"
                required
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-gold"
                placeholder="you@school.test"
              />
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-ink">
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="mt-1.5 w-full rounded-md border border-slate-light bg-white px-3 py-2 text-ink shadow-sm focus:border-gold"
                placeholder="••••••••"
              />
            </div>

            {error && (
              <p role="alert" className="rounded-md bg-brick-light px-3 py-2 text-sm text-brick">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-md bg-ink px-4 py-2.5 font-medium text-paper transition hover:bg-ink-light disabled:opacity-60"
            >
              {isSubmitting ? "Signing in…" : "Sign in"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

function RoleLegendRow({ color, label, desc }: { color: string; label: string; desc: string }) {
  return (
    <div className="flex items-start gap-3">
      <span className={`mt-1 h-2.5 w-2.5 flex-none rounded-full ${color}`} aria-hidden />
      <div>
        <dt className="font-medium text-paper">{label}</dt>
        <dd className="text-paper/70">{desc}</dd>
      </div>
    </div>
  );
}

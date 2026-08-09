"use client";

import { ReactNode, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { UserRole } from "@/lib/types";

export function RequireRole({ role, children }: { role: UserRole; children: ReactNode }) {
  const { user, isLoading, logout } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (isLoading) return;
    if (!user) {
      router.replace("/login");
      return;
    }
    if (user.role !== role) {
      router.replace(`/${user.role.toLowerCase()}`);
    }
  }, [user, isLoading, role, router]);

  if (isLoading || !user || user.role !== role) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="font-serif text-lg text-slate">Loading…</p>
      </div>
    );
  }

  return <>{children}</>;
}

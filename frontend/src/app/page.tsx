"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";

const ROLE_HOME: Record<string, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
};

export default function HomePage() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (isLoading) return;
    router.replace(user ? ROLE_HOME[user.role] : "/login");
  }, [user, isLoading, router]);

  return (
    <div className="flex min-h-screen items-center justify-center">
      <p className="font-serif text-lg text-slate">Loading…</p>
    </div>
  );
}

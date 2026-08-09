"use client";

import { ReactNode } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { RoleBadge } from "./RoleBadge";
import { UserRole } from "@/lib/types";

const ACCENT_BORDER: Record<UserRole, string> = {
  Admin: "border-ink",
  Teacher: "border-sage",
  Student: "border-gold",
};

interface NavItem {
  href: string;
  label: string;
}

export function AppShell({
  role,
  navItems,
  children,
}: {
  role: UserRole;
  navItems: NavItem[];
  children: ReactNode;
}) {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen">
      <header className={`border-b-2 ${ACCENT_BORDER[role]} bg-white`}>
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <div className="flex items-center gap-3">
            <span className="font-serif text-lg text-ink">Handin</span>
            <RoleBadge role={role} />
          </div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-slate">{user?.fullName}</span>
            <button
              onClick={logout}
              className="rounded-md border border-slate-light px-3 py-1.5 text-sm text-ink transition hover:bg-paper-dim"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto flex max-w-6xl gap-8 px-6 py-8">
        <nav className="w-48 flex-none">
          <ul className="space-y-1">
            {navItems.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className="block rounded-md px-3 py-2 text-sm text-ink transition hover:bg-paper-dim"
                >
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        <main className="flex-1 min-w-0">{children}</main>
      </div>
    </div>
  );
}

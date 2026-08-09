import { UserRole } from "@/lib/types";

const ROLE_STYLES: Record<UserRole, string> = {
  Admin: "bg-ink text-paper",
  Teacher: "bg-sage text-paper",
  Student: "bg-gold text-ink",
};

export function RoleBadge({ role }: { role: UserRole }) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${ROLE_STYLES[role]}`}>
      {role}
    </span>
  );
}

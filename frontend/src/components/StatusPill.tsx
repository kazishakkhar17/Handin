const STATUS_STYLES: Record<string, string> = {
  Draft: "bg-slate-light text-slate",
  Published: "bg-sage-light text-sage",
  NotSubmitted: "bg-slate-light text-slate",
  Submitted: "bg-gold-light text-gold",
  Late: "bg-brick-light text-brick",
  Graded: "bg-sage-light text-sage",
  ReturnedForRevision: "bg-brick-light text-brick",
};

export function StatusPill({ status }: { status: string }) {
  const style = STATUS_STYLES[status] ?? "bg-slate-light text-slate";
  const label = status.replace(/([a-z])([A-Z])/g, "$1 $2");
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${style}`}>
      {label}
    </span>
  );
}

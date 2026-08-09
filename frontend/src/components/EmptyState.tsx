export function EmptyState({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-light bg-white/50 px-6 py-12 text-center">
      <p className="font-serif text-lg text-ink">{title}</p>
      <p className="mt-1 text-sm text-slate">{description}</p>
    </div>
  );
}

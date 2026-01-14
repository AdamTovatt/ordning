interface IdTagProps {
  id: string;
  className?: string;
}

export function IdTag({ id, className = '' }: IdTagProps) {
  return (
    <span className={`inline-flex items-center px-2 py-1 rounded text-sm font-mono bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] opacity-70 border border-[var(--color-border)] ${className}`}>
      {id}
    </span>
  );
}

import { type SelectHTMLAttributes, forwardRef } from 'react';

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  error?: string;
  label?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ error, label, className = '', id, children, ...props }, ref) => {
    const selectId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className="flex flex-col gap-2 w-full">
        {label && (
          <label
            htmlFor={selectId}
            className="text-sm font-medium text-[var(--color-fg)]"
          >
            {label}
          </label>
        )}
        <select
          ref={ref}
          id={selectId}
          className={`
            w-full px-4 py-3 rounded-md
            bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)]
            border border-[var(--color-border)]
            focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)]
            disabled:opacity-60 disabled:cursor-not-allowed
            transition-[border-color,box-shadow]
            ${error ? 'border-danger-dark' : ''}
            ${className}
          `}
          {...props}
        >
          {children}
        </select>
        {error && (
          <span className="text-sm text-danger-dark">{error}</span>
        )}
      </div>
    );
  }
);

Select.displayName = 'Select';

import { type InputHTMLAttributes, forwardRef } from 'react';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  error?: string;
  label?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ error, label, className = '', id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className="flex flex-col gap-2 w-full">
        {label && (
          <label
            htmlFor={inputId}
            className="text-sm font-medium text-[var(--color-fg)]"
          >
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={`
            w-full px-4 py-3 rounded-md
            bg-[var(--elevation-level-2-dark)] text-[var(--color-fg)]
            border border-[var(--color-border)]
            placeholder:text-[var(--color-fg)] placeholder:opacity-50
            focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)]
            disabled:opacity-60 disabled:cursor-not-allowed
            transition-[border-color,box-shadow]
            ${error ? 'border-danger-dark' : ''}
            ${className}
          `}
          {...props}
        />
        {error && (
          <span className="text-sm text-danger-dark">{error}</span>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

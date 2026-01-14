import { type TextareaHTMLAttributes, forwardRef } from 'react';

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: string;
  label?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ error, label, className = '', id, ...props }, ref) => {
    const textareaId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className="flex flex-col gap-2 w-full">
        {label && (
          <label
            htmlFor={textareaId}
            className="text-sm font-medium text-[var(--color-fg)]"
          >
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          id={textareaId}
          className={`
            w-full px-4 py-3 rounded-md
            bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)]
            border border-[var(--color-border)]
            placeholder:text-[var(--color-fg)] placeholder:opacity-50
            focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)]
            disabled:opacity-60 disabled:cursor-not-allowed
            transition-[border-color,box-shadow]
            resize-y min-h-[100px]
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

Textarea.displayName = 'Textarea';

import { type ReactNode, type ButtonHTMLAttributes } from 'react';

export interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  children: ReactNode;
}

export function IconButton({
  variant = 'default',
  size = 'md',
  className = '',
  children,
  disabled,
  loading = false,
  ...props
}: IconButtonProps) {
  const baseStyles = 'inline-flex items-center justify-center rounded-full transition-all';

  const variantStyles = {
    default: 'bg-transparent text-[var(--color-fg)] opacity-60 hover:opacity-100 hover:bg-[var(--elevation-level-4-dark)] disabled:cursor-not-allowed disabled:opacity-40',
    danger: 'bg-transparent text-[var(--dark-danger-color)] opacity-60 hover:opacity-100 hover:bg-[rgba(182,35,36,0.1)] disabled:cursor-not-allowed disabled:opacity-40',
    ghost: 'bg-transparent text-[var(--color-fg)] opacity-60 hover:opacity-100 hover:bg-[var(--elevation-level-3-dark)] disabled:cursor-not-allowed disabled:opacity-40',
  };

  const sizeStyles = {
    sm: 'p-1 text-sm',
    md: 'p-2 text-base',
    lg: 'p-3 text-lg',
  };

  const combinedClassName = `${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${className}`;

  return (
    <button
      className={combinedClassName}
      disabled={disabled || loading}
      {...props}
    >
      {loading ? (
        <svg className="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
      ) : (
        children
      )}
    </button>
  );
}

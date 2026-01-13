import { type ReactNode, type ButtonHTMLAttributes } from 'react';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost' | 'outlinePrimary';
  size?: 'sm' | 'md' | 'lg';
  icon?: ReactNode;
  loading?: boolean;
  children?: ReactNode;
}

export function Button({
  variant = 'primary',
  size = 'md',
  icon,
  loading = false,
  disabled,
  className = '',
  children,
  ...props
}: ButtonProps) {
  const baseStyles = 'inline-flex items-center justify-center gap-2 font-medium rounded-md transition-all border';

  const variantStyles = {
    primary: 'bg-brand-gradient text-white border-transparent hover:-translate-y-px shadow-brand disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none',
    secondary: 'bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border-[var(--color-border)] hover:bg-[var(--elevation-level-4-dark)] disabled:opacity-60 disabled:cursor-not-allowed',
    danger: 'bg-danger-dark text-white border-transparent hover:bg-danger-light hover:-translate-y-px disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none',
    ghost: 'bg-transparent text-[var(--color-fg)] border-transparent hover:bg-[var(--elevation-level-3-dark)] disabled:opacity-60 disabled:cursor-not-allowed',
    outlinePrimary: 'bg-transparent text-white border border-brand-light hover:border-brand-extra-light shadow-brand hover:shadow-[0_4px_12px_rgba(121,94,169,0.3)] hover:-translate-y-px disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none',
  };

  const sizeStyles = {
    sm: 'px-4 py-2 text-sm',
    md: 'px-6 py-3 text-base',
    lg: 'px-8 py-4 text-lg',
  };

  const combinedClassName = `${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${className}`;
  
  const buttonStyle = variant === 'primary' ? {
    background: 'var(--brand-gradient)',
    boxShadow: (disabled || loading) ? 'none' : '0 2px 8px rgba(121, 94, 169, 0.2)',
  } : variant === 'outlinePrimary' ? {
    boxShadow: (disabled || loading) ? 'none' : '0 2px 8px rgba(121, 94, 169, 0.2)',
  } : {};

  return (
    <button
      className={combinedClassName}
      style={buttonStyle}
      disabled={disabled || loading}
      {...props}
    >
      {loading ? (
        <>
          <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          Loading...
        </>
      ) : (
        <>
          {icon && <span className="flex items-center justify-center">{icon}</span>}
          {children}
        </>
      )}
    </button>
  );
}

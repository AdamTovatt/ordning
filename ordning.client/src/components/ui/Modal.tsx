import { type ReactNode, useEffect } from 'react';
import { IconX } from '@tabler/icons-react';
import { IconButton } from './IconButton';

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  className?: string;
}

export function Modal({ isOpen, onClose, title, children, className = '' }: ModalProps) {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }

    return () => {
      document.body.style.overflow = '';
    };
  }, [isOpen]);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      onClick={(e) => {
        if (e.target === e.currentTarget) {
          onClose();
        }
      }}
    >
      <div className="fixed inset-0 bg-black/70" />
      <div
        className={`
          relative z-10
          w-full max-w-lg max-h-[90vh]
          bg-[var(--elevation-level-2-dark)]
          border border-[var(--color-border)]
          rounded-2xl
          shadow-[0_20px_40px_rgba(0,0,0,0.4)]
          flex flex-col
          animate-[modalSlideIn_0.2s_ease-out]
          ${className}
        `}
        onClick={(e) => e.stopPropagation()}
      >
        {title && (
          <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--color-border)]">
            <h2 className="text-xl font-semibold text-[var(--color-fg)]">{title}</h2>
            <IconButton onClick={onClose} variant="ghost" size="sm">
              <IconX size={20} />
            </IconButton>
          </div>
        )}
        <div className="flex-1 overflow-y-auto p-6">{children}</div>
      </div>
    </div>
  );
}

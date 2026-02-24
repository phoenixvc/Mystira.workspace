import { useEffect, useRef, type ReactNode } from 'react';
import clsx from 'clsx';
import { FocusTrap } from './FocusTrap';

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg';
  closeOnOverlayClick?: boolean;
}

export function Modal({
  isOpen,
  onClose,
  title,
  children,
  size = 'md',
  closeOnOverlayClick = true,
}: ModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;

    if (isOpen) {
      dialog.showModal();
    } else {
      dialog.close();
    }
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

  const handleOverlayClick = (e: React.MouseEvent) => {
    if (closeOnOverlayClick && e.target === dialogRef.current) {
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <dialog
      ref={dialogRef}
      className={clsx('modal', `modal--${size}`)}
      onClick={handleOverlayClick}
      aria-labelledby={title ? 'modal-title' : undefined}
    >
      <FocusTrap active={isOpen}>
        <div className="modal__content">
          {title && (
            <div className="modal__header">
              <h2 id="modal-title" className="modal__title">
                {title}
              </h2>
              <button
                type="button"
                className="modal__close"
                onClick={onClose}
                aria-label="Close modal"
              >
                ×
              </button>
            </div>
          )}
          <div className="modal__body">{children}</div>
        </div>
      </FocusTrap>
    </dialog>
  );
}

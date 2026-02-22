import { useState, useCallback, useRef } from "react";

export interface ConfirmationOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  variant?: "danger" | "warning" | "info";
}

export interface ConfirmationState extends ConfirmationOptions {
  isOpen: boolean;
}

const defaultOptions: ConfirmationOptions = {
  title: "Confirm",
  message: "Are you sure?",
  confirmText: "Confirm",
  cancelText: "Cancel",
  variant: "danger",
};

/**
 * Hook for managing confirmation dialogs with promise-based API
 *
 * Usage:
 * ```tsx
 * const { confirm, confirmationProps } = useConfirmation();
 *
 * const handleDelete = async () => {
 *   const confirmed = await confirm({
 *     title: "Delete Item",
 *     message: "Are you sure you want to delete this item?",
 *   });
 *   if (confirmed) {
 *     // Proceed with delete
 *   }
 * };
 *
 * return (
 *   <>
 *     <button onClick={handleDelete}>Delete</button>
 *     <ConfirmationDialog {...confirmationProps} />
 *   </>
 * );
 * ```
 */
export function useConfirmation() {
  const [state, setState] = useState<ConfirmationState>({
    ...defaultOptions,
    isOpen: false,
  });
  const resolveRef = useRef<((value: boolean) => void) | null>(null);

  const confirm = useCallback((options: Partial<ConfirmationOptions> = {}): Promise<boolean> => {
    return new Promise(resolve => {
      resolveRef.current = resolve;
      setState({
        ...defaultOptions,
        ...options,
        isOpen: true,
      });
    });
  }, []);

  const handleConfirm = useCallback(() => {
    resolveRef.current?.(true);
    resolveRef.current = null;
    setState(prev => ({ ...prev, isOpen: false }));
  }, []);

  const handleCancel = useCallback(() => {
    resolveRef.current?.(false);
    resolveRef.current = null;
    setState(prev => ({ ...prev, isOpen: false }));
  }, []);

  const confirmationProps = {
    isOpen: state.isOpen,
    title: state.title,
    message: state.message,
    confirmText: state.confirmText,
    cancelText: state.cancelText,
    variant: state.variant,
    onConfirm: handleConfirm,
    onCancel: handleCancel,
  };

  return {
    confirm,
    confirmationProps,
  };
}

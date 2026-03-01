import { useState, useCallback } from "react";
import { ValidationResult } from "../utils/schemaValidator";

interface UploadConfirmation {
  isOpen: boolean;
  type: "no-validation" | "validation-failed" | null;
  errorCount?: number;
}

interface UseFileUploadOptions {
  onUpload: (file: File) => Promise<void>;
  validationResult?: ValidationResult | null;
}

export function useFileUpload({ onUpload, validationResult }: UseFileUploadOptions) {
  const [uploading, setUploading] = useState(false);
  const [pendingFile, setPendingFile] = useState<File | null>(null);
  const [confirmation, setConfirmation] = useState<UploadConfirmation>({
    isOpen: false,
    type: null,
  });

  const performUpload = useCallback(
    async (file: File) => {
      setUploading(true);
      try {
        await onUpload(file);
      } finally {
        setUploading(false);
        setPendingFile(null);
      }
    },
    [onUpload]
  );

  const uploadFile = useCallback(
    async (file: File) => {
      if (!file) {
        return;
      }

      // Check if validation is required
      if (!validationResult) {
        setPendingFile(file);
        setConfirmation({ isOpen: true, type: "no-validation" });
        return;
      }

      if (!validationResult.valid) {
        setPendingFile(file);
        setConfirmation({
          isOpen: true,
          type: "validation-failed",
          errorCount: validationResult.errors.length,
        });
        return;
      }

      // Validation passed, proceed with upload
      await performUpload(file);
    },
    [validationResult, performUpload]
  );

  const confirmUpload = useCallback(async () => {
    setConfirmation({ isOpen: false, type: null });
    if (pendingFile) {
      await performUpload(pendingFile);
    }
  }, [pendingFile, performUpload]);

  const cancelUpload = useCallback(() => {
    setConfirmation({ isOpen: false, type: null });
    setPendingFile(null);
  }, []);

  const confirmationProps = {
    isOpen: confirmation.isOpen,
    title:
      confirmation.type === "no-validation" ? "Upload Without Validation?" : "Upload With Errors?",
    message:
      confirmation.type === "no-validation"
        ? "You haven't validated the file yet. Do you want to upload without validation?"
        : `Validation failed with ${confirmation.errorCount} error(s). Do you still want to upload?`,
    confirmText: "Upload Anyway",
    cancelText: "Cancel",
    variant: "warning" as const,
    onConfirm: confirmUpload,
    onCancel: cancelUpload,
  };

  return {
    uploading,
    uploadFile,
    confirmationProps,
  };
}

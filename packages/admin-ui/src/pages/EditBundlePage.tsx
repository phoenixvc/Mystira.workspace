import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { bundlesApi } from "../api/bundles";
import ErrorAlert from "../components/ErrorAlert";
import FormField from "../components/FormField";
import LoadingSpinner from "../components/LoadingSpinner";
import Textarea from "../components/Textarea";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

const bundleSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  version: z.string().max(50, "Version must be less than 50 characters").optional(),
});

type BundleFormData = z.infer<typeof bundleSchema>;

function EditBundlePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<BundleFormData>({
    resolver: zodResolver(bundleSchema),
    defaultValues: {
      name: "",
      description: "",
      version: "",
    },
  });

  const {
    data: bundle,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["bundle", id],
    queryFn: () => bundlesApi.getBundle(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: BundleFormData) => {
      return bundlesApi.updateBundle(id!, {
        name: data.name,
        description: data.description || undefined,
        version: data.version || undefined,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bundles"] });
      queryClient.invalidateQueries({ queryKey: ["bundle", id] });
      showToast.success("Bundle updated successfully!");
      navigate("/admin/bundles");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to update bundle");
    },
  });

  useEffect(() => {
    if (bundle) {
      reset({
        name: bundle.name,
        description: bundle.description || "",
        version: bundle.version || "",
      });
    }
  }, [bundle, reset]);

  const onSubmit = async (data: BundleFormData) => {
    await updateMutation.mutateAsync(data);
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading bundle..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading bundle"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["bundle", id] })}
      />
    );
  }

  if (!bundle) {
    return (
      <div className="alert alert-danger" role="alert">
        Bundle not found
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">✏️ Edit Bundle</h1>
        <Link to="/admin/bundles" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Bundles
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit(onSubmit)}>
            <FormField label="Name" error={errors.name?.message} required>
              <TextInput id="name" {...register("name")} />
            </FormField>

            <FormField label="Description" error={errors.description?.message}>
              <Textarea id="description" rows={5} {...register("description")} />
            </FormField>

            <FormField label="Version" error={errors.version?.message}>
              <TextInput id="version" {...register("version")} placeholder="e.g., 1.0.0" />
            </FormField>

            <div className="d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <span
                      className="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    Updating...
                  </>
                ) : (
                  <>
                    <i className="bi bi-save"></i> Save Changes
                  </>
                )}
              </button>
              <Link to="/admin/bundles" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EditBundlePage;

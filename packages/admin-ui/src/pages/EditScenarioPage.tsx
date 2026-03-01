import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { scenariosApi } from "../api/scenarios";
import ErrorAlert from "../components/ErrorAlert";
import FormField from "../components/FormField";
import LoadingSpinner from "../components/LoadingSpinner";
import NumberInput from "../components/NumberInput";
import Textarea from "../components/Textarea";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

const scenarioSchema = z.object({
  title: z.string().min(1, "Title is required").max(200, "Title must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  ageRating: z.number().min(0).max(18),
  tags: z.string().optional(), // Comma-separated tags
});

type ScenarioFormData = z.infer<typeof scenarioSchema>;

function EditScenarioPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<ScenarioFormData>({
    resolver: zodResolver(scenarioSchema),
    defaultValues: {
      title: "",
      description: "",
      ageRating: 0,
      tags: "",
    },
  });

  const {
    data: scenario,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["scenario", id],
    queryFn: () => scenariosApi.getScenario(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: ScenarioFormData) => {
      const tags = data.tags
        ? data.tags
            .split(",")
            .map(tag => tag.trim())
            .filter(tag => tag.length > 0)
        : [];
      return scenariosApi.updateScenario(id!, {
        title: data.title,
        description: data.description,
        ageRating: data.ageRating,
        tags,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["scenarios"] });
      queryClient.invalidateQueries({ queryKey: ["scenario", id] });
      showToast.success("Scenario updated successfully!");
      navigate("/admin/scenarios");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to update scenario");
    },
  });

  useEffect(() => {
    if (scenario) {
      reset({
        title: scenario.title,
        description: scenario.description || "",
        ageRating: scenario.ageRating,
        tags: scenario.tags?.join(", ") || "",
      });
    }
  }, [scenario, reset]);

  const onSubmit = async (data: ScenarioFormData) => {
    await updateMutation.mutateAsync(data);
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading scenario..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading scenario"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["scenario", id] })}
      />
    );
  }

  if (!scenario) {
    return (
      <div className="alert alert-warning" role="alert">
        Scenario not found.
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">✏️ Edit Scenario</h1>
        <Link to="/admin/scenarios" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Scenarios
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit(onSubmit)}>
            <FormField label="Title" error={errors.title?.message} required>
              <TextInput id="title" {...register("title")} />
            </FormField>

            <FormField label="Description" error={errors.description?.message}>
              <Textarea id="description" rows={5} {...register("description")} />
            </FormField>

            <FormField
              label="Age Rating"
              error={errors.ageRating?.message}
              required
              helpText="Age rating from 0 to 18"
            >
              <NumberInput
                id="ageRating"
                min="0"
                max="18"
                {...register("ageRating", { valueAsNumber: true })}
              />
            </FormField>

            <FormField
              label="Tags"
              error={errors.tags?.message}
              helpText="Enter tags separated by commas"
            >
              <TextInput
                id="tags"
                placeholder="Comma-separated tags (e.g., fantasy, adventure, mystery)"
                {...register("tags")}
              />
            </FormField>

            <div className="d-flex gap-2">
              <button
                type="submit"
                className="btn btn-primary"
                disabled={isSubmitting || updateMutation.isPending}
              >
                {isSubmitting || updateMutation.isPending ? (
                  <>
                    <span
                      className="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="bi bi-save"></i> Save Changes
                  </>
                )}
              </button>
              <Link to="/admin/scenarios" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EditScenarioPage;

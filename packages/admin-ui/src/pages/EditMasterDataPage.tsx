import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import {
  ageGroupsApi,
  archetypesApi,
  compassAxesApi,
  echoTypesApi,
  fantasyThemesApi,
} from "../api/masterData";
import ErrorAlert from "../components/ErrorAlert";
import FormField from "../components/FormField";
import LoadingSpinner from "../components/LoadingSpinner";
import NumberInput from "../components/NumberInput";
import Textarea from "../components/Textarea";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

type MasterDataType =
  | "age-groups"
  | "archetypes"
  | "compass-axes"
  | "echo-types"
  | "fantasy-themes";

const ageGroupSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  minAge: z.number().min(0).max(100).optional(),
  maxAge: z.number().min(0).max(100).optional(),
});

const archetypeSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
});

const compassAxisSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  positiveLabel: z.string().max(100).optional(),
  negativeLabel: z.string().max(100).optional(),
});

const echoTypeSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
});

const fantasyThemeSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
});

type AgeGroupFormData = z.infer<typeof ageGroupSchema>;
type ArchetypeFormData = z.infer<typeof archetypeSchema>;
type CompassAxisFormData = z.infer<typeof compassAxisSchema>;
type EchoTypeFormData = z.infer<typeof echoTypeSchema>;
type FantasyThemeFormData = z.infer<typeof fantasyThemeSchema>;

type FormData =
  | AgeGroupFormData
  | ArchetypeFormData
  | CompassAxisFormData
  | EchoTypeFormData
  | FantasyThemeFormData;

function EditMasterDataPage() {
  const { type, id } = useParams<{ type: MasterDataType; id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const getSchema = () => {
    if (!type) return null;
    switch (type) {
      case "age-groups":
        return ageGroupSchema;
      case "archetypes":
        return archetypeSchema;
      case "compass-axes":
        return compassAxisSchema;
      case "echo-types":
        return echoTypeSchema;
      case "fantasy-themes":
        return fantasyThemeSchema;
      default:
        return null;
    }
  };

  const getTitle = () => {
    if (!type) return "";
    switch (type) {
      case "age-groups":
        return "Age Group";
      case "archetypes":
        return "Archetype";
      case "compass-axes":
        return "Compass Axis";
      case "echo-types":
        return "Echo Type";
      case "fantasy-themes":
        return "Fantasy Theme";
      default:
        return "";
    }
  };

  const getApi = () => {
    if (!type) return null;
    switch (type) {
      case "age-groups":
        return { get: ageGroupsApi.getAgeGroup, update: ageGroupsApi.updateAgeGroup };
      case "archetypes":
        return { get: archetypesApi.getArchetype, update: archetypesApi.updateArchetype };
      case "compass-axes":
        return { get: compassAxesApi.getCompassAxis, update: compassAxesApi.updateCompassAxis };
      case "echo-types":
        return { get: echoTypesApi.getEchoType, update: echoTypesApi.updateEchoType };
      case "fantasy-themes":
        return {
          get: fantasyThemesApi.getFantasyTheme,
          update: fantasyThemesApi.updateFantasyTheme,
        };
      default:
        return null;
    }
  };

  const getDefaultValues = (): FormData => {
    if (!type) return { name: "", description: "" };
    switch (type) {
      case "age-groups":
        return { name: "", description: "", minAge: undefined, maxAge: undefined };
      case "archetypes":
        return { name: "", description: "" };
      case "compass-axes":
        return { name: "", description: "", positiveLabel: "", negativeLabel: "" };
      case "echo-types":
        return { name: "", description: "" };
      case "fantasy-themes":
        return { name: "", description: "" };
      default:
        return { name: "", description: "" };
    }
  };

  const validTypes: MasterDataType[] = [
    "age-groups",
    "archetypes",
    "compass-axes",
    "echo-types",
    "fantasy-themes",
  ];
  const isValidType = type && validTypes.includes(type);
  const api = getApi();
  const schema = getSchema();
  const title = getTitle();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<FormData>({
    resolver: schema ? zodResolver(schema) : undefined,
    defaultValues: getDefaultValues(),
  });

  const {
    data: item,
    isLoading,
    error,
  } = useQuery({
    queryKey: [type, id],
    queryFn: () => {
      if (!api || !id) throw new Error("Invalid API or ID");
      return api.get(id);
    },
    enabled:
      !!id &&
      !!api &&
      !!type &&
      ["age-groups", "archetypes", "compass-axes", "echo-types", "fantasy-themes"].includes(type),
  });

  const updateMutation = useMutation({
    mutationFn: (data: FormData) => {
      if (!api || !id) throw new Error("Invalid API or ID");
      return api.update(id, data);
    },
    onSuccess: () => {
      if (!type) return;
      queryClient.invalidateQueries({ queryKey: [type] });
      queryClient.invalidateQueries({ queryKey: [type, id] });
      showToast.success(`${title} updated successfully!`);
      navigate(`/admin/master-data/${type}`);
    },
    onError: error => {
      showToast.error(
        error instanceof Error ? error.message : `Failed to update ${title.toLowerCase()}`
      );
    },
  });

  useEffect(() => {
    if (item) {
      reset(item as FormData);
    }
  }, [item, reset]);

  if (
    !type ||
    !["age-groups", "archetypes", "compass-axes", "echo-types", "fantasy-themes"].includes(type)
  ) {
    return (
      <div className="alert alert-danger" role="alert">
        Invalid master data type.
      </div>
    );
  }

  const onSubmit = async (data: FormData) => {
    await updateMutation.mutateAsync(data);
  };

  if (!isValidType) {
    return (
      <div className="alert alert-danger" role="alert">
        Invalid master data type.
      </div>
    );
  }

  if (isLoading) {
    return <LoadingSpinner message={`Loading ${title.toLowerCase()}...`} />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title={`Error loading ${title.toLowerCase()}`}
        onRetry={() => queryClient.invalidateQueries({ queryKey: [type, id] })}
      />
    );
  }

  if (!item) {
    return (
      <div className="alert alert-warning" role="alert">
        {title} not found.
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">✏️ Edit {title}</h1>
        <Link to={`/admin/master-data/${type}`} className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to {title}s
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

            {type === "age-groups" && (
              <>
                <FormField
                  label="Minimum Age"
                  error={(errors as Record<string, { message?: string }>).minAge?.message}
                  helpText="Minimum age for this group (0-100)"
                >
                  <NumberInput
                    id="minAge"
                    min="0"
                    max="100"
                    {...register("minAge", { valueAsNumber: true })}
                  />
                </FormField>

                <FormField
                  label="Maximum Age"
                  error={(errors as Record<string, { message?: string }>).maxAge?.message}
                  helpText="Maximum age for this group (0-100)"
                >
                  <NumberInput
                    id="maxAge"
                    min="0"
                    max="100"
                    {...register("maxAge", { valueAsNumber: true })}
                  />
                </FormField>
              </>
            )}

            {type === "compass-axes" && (
              <>
                <FormField
                  label="Positive Label"
                  error={(errors as Record<string, { message?: string }>).positiveLabel?.message}
                  helpText="Label for the positive end of the axis"
                >
                  <TextInput id="positiveLabel" {...register("positiveLabel")} />
                </FormField>

                <FormField
                  label="Negative Label"
                  error={(errors as Record<string, { message?: string }>).negativeLabel?.message}
                  helpText="Label for the negative end of the axis"
                >
                  <TextInput id="negativeLabel" {...register("negativeLabel")} />
                </FormField>
              </>
            )}

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
              <Link to={`/admin/master-data/${type}`} className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EditMasterDataPage;

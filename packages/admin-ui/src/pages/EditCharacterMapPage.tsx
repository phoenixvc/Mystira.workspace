import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { characterMapsApi } from "../api/characterMaps";
import ErrorAlert from "../components/ErrorAlert";
import FormField from "../components/FormField";
import LoadingSpinner from "../components/LoadingSpinner";
import Textarea from "../components/Textarea";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

const characterMapSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  imageId: z.string().optional(),
});

type CharacterMapFormData = z.infer<typeof characterMapSchema>;

function EditCharacterMapPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<CharacterMapFormData>({
    resolver: zodResolver(characterMapSchema),
    defaultValues: {
      name: "",
      description: "",
      imageId: "",
    },
  });

  const {
    data: characterMap,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["characterMap", id],
    queryFn: () => characterMapsApi.getCharacterMap(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: CharacterMapFormData) => {
      return characterMapsApi.updateCharacterMap(id!, {
        name: data.name,
        description: data.description || undefined,
        imageId: data.imageId || undefined,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["characterMaps"] });
      queryClient.invalidateQueries({ queryKey: ["characterMap", id] });
      showToast.success("Character map updated successfully!");
      navigate("/admin/character-maps");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to update character map");
    },
  });

  useEffect(() => {
    if (characterMap) {
      reset({
        name: characterMap.name,
        description: characterMap.description || "",
        imageId: characterMap.imageId || "",
      });
    }
  }, [characterMap, reset]);

  const onSubmit = async (data: CharacterMapFormData) => {
    await updateMutation.mutateAsync(data);
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading character map..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading character map"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["characterMap", id] })}
      />
    );
  }

  if (!characterMap) {
    return (
      <div className="alert alert-warning" role="alert">
        Character map not found.
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">✏️ Edit Character Map</h1>
        <Link to="/admin/character-maps" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Character Maps
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

            <FormField
              label="Image ID"
              error={errors.imageId?.message}
              helpText="ID of the media asset to use as the character map image"
            >
              <TextInput id="imageId" {...register("imageId")} />
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
              <Link to="/admin/character-maps" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EditCharacterMapPage;

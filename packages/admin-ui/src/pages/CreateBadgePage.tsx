import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { z } from "zod";
import { badgesApi } from "../api/badges";
import FormField from "../components/FormField";
import Textarea from "../components/Textarea";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

const badgeSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be less than 200 characters"),
  description: z.string().max(1000, "Description must be less than 1000 characters").optional(),
  imageId: z.string().optional(),
});

type BadgeFormData = z.infer<typeof badgeSchema>;

function CreateBadgePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BadgeFormData>({
    resolver: zodResolver(badgeSchema),
    defaultValues: {
      name: "",
      description: "",
      imageId: "",
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: BadgeFormData) => {
      return badgesApi.createBadge({
        name: data.name,
        description: data.description || undefined,
        imageId: data.imageId || undefined,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["badges"] });
      showToast.success("Badge created successfully!");
      navigate("/admin/badges");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to create badge");
    },
  });

  const onSubmit = async (data: BadgeFormData) => {
    await createMutation.mutateAsync(data);
  };

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">➕ Create Badge</h1>
        <Link to="/admin/badges" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Badges
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
              helpText="ID of the media asset to use as the badge image"
            >
              <TextInput id="imageId" {...register("imageId")} />
            </FormField>

            <div className="d-flex gap-2">
              <button
                type="submit"
                className="btn btn-primary"
                disabled={isSubmitting || createMutation.isPending}
              >
                {isSubmitting || createMutation.isPending ? (
                  <>
                    <span
                      className="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    Creating...
                  </>
                ) : (
                  <>
                    <i className="bi bi-plus-circle"></i> Create Badge
                  </>
                )}
              </button>
              <Link to="/admin/badges" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default CreateBadgePage;

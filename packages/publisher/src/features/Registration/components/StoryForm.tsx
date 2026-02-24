import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button, Input } from '@/components';
import type { CreateStoryRequest, UpdateStoryRequest } from '@/api/types';

const storySchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title too long'),
  summary: z.string().min(10, 'Summary must be at least 10 characters').max(2000, 'Summary too long'),
});

type StoryFormData = z.infer<typeof storySchema>;

interface StoryFormProps {
  defaultValues?: Partial<StoryFormData>;
  onSubmit: (data: CreateStoryRequest | UpdateStoryRequest) => void;
  isSubmitting?: boolean;
  submitLabel?: string;
}

export function StoryForm({
  defaultValues,
  onSubmit,
  isSubmitting = false,
  submitLabel = 'Save',
}: StoryFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<StoryFormData>({
    resolver: zodResolver(storySchema),
    defaultValues,
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="story-form">
      <Input
        label="Title"
        placeholder="Enter story title"
        error={errors.title?.message}
        {...register('title')}
      />

      <div className="story-form__field">
        <label htmlFor="summary" className="input-label">
          Summary
        </label>
        <textarea
          id="summary"
          placeholder="Describe your story..."
          className={`input story-form__textarea ${errors.summary ? 'input--error' : ''}`}
          rows={4}
          {...register('summary')}
        />
        {errors.summary && (
          <span className="input-error" role="alert">
            {errors.summary.message}
          </span>
        )}
      </div>

      <div className="story-form__actions">
        <Button type="submit" loading={isSubmitting}>
          {submitLabel}
        </Button>
      </div>
    </form>
  );
}

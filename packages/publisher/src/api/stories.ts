import { request } from './client';
import type {
  Story,
  CreateStoryRequest,
  UpdateStoryRequest,
  StoryListParams,
  PaginatedResponse,
} from './types';

const STORIES_PATH = '/stories';

export const storiesApi = {
  // List stories with optional filters
  list: (params?: StoryListParams): Promise<PaginatedResponse<Story>> =>
    request<PaginatedResponse<Story>>({
      method: 'GET',
      url: STORIES_PATH,
      params,
    }),

  // Get single story by ID
  get: (id: string): Promise<Story> =>
    request<Story>({
      method: 'GET',
      url: `${STORIES_PATH}/${id}`,
    }),

  // Create new story
  create: (data: CreateStoryRequest): Promise<Story> =>
    request<Story>({
      method: 'POST',
      url: STORIES_PATH,
      data,
    }),

  // Update existing story
  update: (id: string, data: UpdateStoryRequest): Promise<Story> =>
    request<Story>({
      method: 'PATCH',
      url: `${STORIES_PATH}/${id}`,
      data,
    }),

  // Delete story
  delete: (id: string): Promise<void> =>
    request<void>({
      method: 'DELETE',
      url: `${STORIES_PATH}/${id}`,
    }),

  // Submit story for registration
  submitForRegistration: (id: string): Promise<Story> =>
    request<Story>({
      method: 'POST',
      url: `${STORIES_PATH}/${id}/submit`,
    }),
};

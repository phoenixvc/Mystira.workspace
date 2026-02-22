import { request } from './client';
import type {
  Attribution,
  AddContributorRequest,
  UpdateAttributionRequest,
  ApprovalRequest,
  OverrideRequest,
  ValidateSplitsResponse,
  User,
  UserSearchParams,
} from './types';

const CONTRIBUTORS_PATH = '/contributors';
const USERS_PATH = '/users';

export const contributorsApi = {
  // Get all contributors for a story
  getByStory: (storyId: string): Promise<Attribution[]> =>
    request<Attribution[]>({
      method: 'GET',
      url: `${CONTRIBUTORS_PATH}/story/${storyId}`,
    }),

  // Add contributor to story
  add: (data: AddContributorRequest): Promise<Attribution> =>
    request<Attribution>({
      method: 'POST',
      url: CONTRIBUTORS_PATH,
      data,
    }),

  // Update contributor attribution
  update: (id: string, data: UpdateAttributionRequest): Promise<Attribution> =>
    request<Attribution>({
      method: 'PATCH',
      url: `${CONTRIBUTORS_PATH}/${id}`,
      data,
    }),

  // Remove contributor from story
  remove: (id: string): Promise<void> =>
    request<void>({
      method: 'DELETE',
      url: `${CONTRIBUTORS_PATH}/${id}`,
    }),

  // Submit approval decision
  submitApproval: (data: ApprovalRequest): Promise<Attribution> =>
    request<Attribution>({
      method: 'POST',
      url: `${CONTRIBUTORS_PATH}/approve`,
      data,
    }),

  // Override non-responsive contributor
  override: (data: OverrideRequest): Promise<Attribution> =>
    request<Attribution>({
      method: 'POST',
      url: `${CONTRIBUTORS_PATH}/override`,
      data,
    }),

  // Validate royalty splits sum to 100%
  validateSplits: (storyId: string): Promise<ValidateSplitsResponse> =>
    request<ValidateSplitsResponse>({
      method: 'GET',
      url: `${CONTRIBUTORS_PATH}/validate/${storyId}`,
    }),

  // Search users for adding as contributors
  searchUsers: (params: UserSearchParams): Promise<User[]> =>
    request<User[]>({
      method: 'GET',
      url: `${USERS_PATH}/search`,
      params,
    }),
};

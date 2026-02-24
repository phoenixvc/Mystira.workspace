import { request } from './client';
import type {
  OpenRole,
  RoleRequest,
  CreateOpenRoleRequest,
  SubmitRoleRequestRequest,
  RespondToRoleRequestRequest,
  OpenRoleListParams,
  RoleRequestListParams,
} from './types/role-request';

const OPEN_ROLES_PATH = '/open-roles';
const ROLE_REQUESTS_PATH = '/role-requests';

export const roleRequestsApi = {
  // Get all open roles
  getOpenRoles: (params?: OpenRoleListParams) =>
    request<{ items: OpenRole[]; total: number }>({
      method: 'GET',
      url: OPEN_ROLES_PATH,
      params,
    }),

  // Get open roles for a specific story
  getOpenRolesByStory: (storyId: string) =>
    request<OpenRole[]>({
      method: 'GET',
      url: `${OPEN_ROLES_PATH}/story/${storyId}`,
    }),

  // Create an open role
  createOpenRole: (data: CreateOpenRoleRequest) =>
    request<OpenRole>({
      method: 'POST',
      url: OPEN_ROLES_PATH,
      data,
    }),

  // Update an open role
  updateOpenRole: (id: string, data: Partial<CreateOpenRoleRequest>) =>
    request<OpenRole>({
      method: 'PATCH',
      url: `${OPEN_ROLES_PATH}/${id}`,
      data,
    }),

  // Delete an open role
  deleteOpenRole: (id: string) =>
    request<void>({
      method: 'DELETE',
      url: `${OPEN_ROLES_PATH}/${id}`,
    }),

  // Get role requests (for publishers)
  getRoleRequests: (params?: RoleRequestListParams) =>
    request<{ items: RoleRequest[]; total: number }>({
      method: 'GET',
      url: ROLE_REQUESTS_PATH,
      params,
    }),

  // Get role requests for a specific story
  getRoleRequestsByStory: (storyId: string) =>
    request<RoleRequest[]>({
      method: 'GET',
      url: `${ROLE_REQUESTS_PATH}/story/${storyId}`,
    }),

  // Get role requests for a specific open role
  getRoleRequestsByOpenRole: (openRoleId: string) =>
    request<RoleRequest[]>({
      method: 'GET',
      url: `${ROLE_REQUESTS_PATH}/open-role/${openRoleId}`,
    }),

  // Submit a role request (for contributors)
  submitRoleRequest: (data: SubmitRoleRequestRequest) =>
    request<RoleRequest>({
      method: 'POST',
      url: ROLE_REQUESTS_PATH,
      data,
    }),

  // Respond to a role request (for publishers)
  respondToRoleRequest: (data: RespondToRoleRequestRequest) =>
    request<RoleRequest>({
      method: 'POST',
      url: `${ROLE_REQUESTS_PATH}/${data.requestId}/respond`,
      data: { accept: data.accept, message: data.message },
    }),

  // Withdraw a role request (for contributors)
  withdrawRoleRequest: (requestId: string) =>
    request<void>({
      method: 'POST',
      url: `${ROLE_REQUESTS_PATH}/${requestId}/withdraw`,
    }),
};


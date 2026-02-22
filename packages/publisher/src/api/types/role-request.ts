// Role Request types for open roles/tender system

import type { ContributorRole } from './story';

export type RoleRequestStatus = 'pending' | 'accepted' | 'rejected' | 'withdrawn';

export interface OpenRole {
  id: string;
  storyId: string;
  storyTitle: string;
  role: ContributorRole;
  splitPercentage: number;
  description?: string;
  requirements?: string;
  deadline?: string;
  createdAt: string;
  updatedAt: string;
}

export interface RoleRequest {
  id: string;
  openRoleId: string;
  storyId: string;
  storyTitle: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: ContributorRole;
  proposedSplit?: number;
  message?: string;
  portfolio?: string;
  status: RoleRequestStatus;
  createdAt: string;
  updatedAt: string;
  respondedAt?: string;
}

export interface CreateOpenRoleRequest {
  storyId: string;
  role: ContributorRole;
  splitPercentage: number;
  description?: string;
  requirements?: string;
  deadline?: string;
}

export interface SubmitRoleRequestRequest {
  openRoleId: string;
  proposedSplit?: number;
  message?: string;
  portfolio?: string;
}

export interface RespondToRoleRequestRequest {
  requestId: string;
  accept: boolean;
  message?: string;
}

export interface OpenRoleListParams {
  role?: ContributorRole;
  storyId?: string;
  page?: number;
  pageSize?: number;
}

export interface RoleRequestListParams {
  storyId?: string;
  status?: RoleRequestStatus;
  page?: number;
  pageSize?: number;
}


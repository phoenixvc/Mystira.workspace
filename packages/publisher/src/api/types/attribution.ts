// Attribution types that mirror backend API contracts

import type { ApprovalStatus, ContributorRole } from './story';

export interface Attribution {
  id: string;
  storyId: string;
  userId: string;
  role: ContributorRole;
  split: number;
  approvalStatus: ApprovalStatus;
  createdAt: string;
  updatedAt: string;
}

export interface AddContributorRequest {
  storyId: string;
  userId?: string;
  email?: string;
  role: ContributorRole;
  split: number;
}

export interface UpdateAttributionRequest {
  role?: ContributorRole;
  split?: number;
}

export interface ApprovalRequest {
  storyId: string;
  approved: boolean;
  comment?: string;
}

export interface OverrideRequest {
  storyId: string;
  targetUserId: string;
  justification: string;
}

export interface RoyaltySplit {
  userId: string;
  userName: string;
  role: ContributorRole;
  percentage: number;
}

export interface ValidateSplitsResponse {
  valid: boolean;
  total: number;
  errors?: string[];
}

// Story types that mirror backend API contracts

export type StoryStatus =
  | 'draft'
  | 'pending_approval'
  | 'approved'
  | 'registered'
  | 'rejected';

export interface Story {
  id: string;
  title: string;
  summary: string;
  contributors: StoryContributor[];
  status: StoryStatus;
  createdAt: string;
  updatedAt: string;
  registeredAt?: string;
  transactionId?: string;
}

export interface StoryContributor {
  userId: string;
  userName: string;
  userEmail: string;
  role: ContributorRole;
  split: number;
  approvalStatus: ApprovalStatus;
  approvedAt?: string;
}

export type ContributorRole =
  | 'primary_author'
  | 'co_author'
  | 'illustrator'
  | 'editor'
  | 'moderator'
  | 'publisher';

export type ApprovalStatus = 'pending' | 'approved' | 'rejected' | 'overridden';

export interface CreateStoryRequest {
  title: string;
  summary: string;
  sourceProjectId?: string;
}

export interface UpdateStoryRequest {
  title?: string;
  summary?: string;
}

export interface StoryListParams {
  status?: StoryStatus;
  search?: string;
  page?: number;
  pageSize?: number;
}

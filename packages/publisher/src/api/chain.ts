// gRPC client for Mystira.Chain blockchain interactions

import { env } from '@/config/env';

const GRPC_ENDPOINT = env.grpcEndpoint;

export interface RegistrationRequest {
  storyId: string;
  metadata: StoryMetadata;
  contributors: ChainContributor[];
}

export interface StoryMetadata {
  title: string;
  summary: string;
  createdAt: string;
}

export interface ChainContributor {
  userId: string;
  role: string;
  splitPercentage: number;
}

export interface RegistrationResponse {
  transactionId: string;
  blockNumber: number;
  timestamp: string;
  status: 'pending' | 'confirmed' | 'failed';
}

export interface RegistrationStatus {
  transactionId: string;
  status: 'pending' | 'confirmed' | 'failed';
  confirmations: number;
  blockNumber?: number;
  errorMessage?: string;
}

// In a real implementation, this would use grpc-web client
// For now, we provide a REST-like interface that wraps the gRPC calls
export const chainApi = {
  // Register story on-chain
  registerStory: async (data: RegistrationRequest): Promise<RegistrationResponse> => {
    const endpoint = GRPC_ENDPOINT.startsWith('http') ? GRPC_ENDPOINT : `https://${GRPC_ENDPOINT}`;
    const response = await fetch(`${endpoint}/chain/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to register on chain');
    }

    return response.json();
  },

  // Check registration status
  getRegistrationStatus: async (transactionId: string): Promise<RegistrationStatus> => {
    const endpoint = GRPC_ENDPOINT.startsWith('http') ? GRPC_ENDPOINT : `https://${GRPC_ENDPOINT}`;
    const response = await fetch(`${endpoint}/chain/status/${transactionId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch registration status');
    }

    return response.json();
  },

  // Get on-chain record for a story
  getOnChainRecord: async (storyId: string): Promise<RegistrationResponse | null> => {
    const endpoint = GRPC_ENDPOINT.startsWith('http') ? GRPC_ENDPOINT : `https://${GRPC_ENDPOINT}`;
    const response = await fetch(`${endpoint}/chain/record/${storyId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
      },
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error('Failed to fetch on-chain record');
    }

    return response.json();
  },
};

import { request } from './client';
import type { AuditLog, AuditLogParams, PaginatedResponse } from './types';

const AUDIT_PATH = '/audit';

export const auditApi = {
  // Get audit logs with filters
  getLogs: (params?: AuditLogParams): Promise<PaginatedResponse<AuditLog>> =>
    request<PaginatedResponse<AuditLog>>({
      method: 'GET',
      url: AUDIT_PATH,
      params,
    }),

  // Get audit logs for specific story
  getByStory: (storyId: string, params?: AuditLogParams): Promise<PaginatedResponse<AuditLog>> =>
    request<PaginatedResponse<AuditLog>>({
      method: 'GET',
      url: `${AUDIT_PATH}/story/${storyId}`,
      params,
    }),

  // Get single audit log entry
  get: (id: string): Promise<AuditLog> =>
    request<AuditLog>({
      method: 'GET',
      url: `${AUDIT_PATH}/${id}`,
    }),

  // Export audit logs as CSV
  exportCsv: async (params?: AuditLogParams): Promise<Blob> => {
    const response = await request<Blob>({
      method: 'GET',
      url: `${AUDIT_PATH}/export`,
      params,
      responseType: 'blob',
    });
    return response;
  },
};

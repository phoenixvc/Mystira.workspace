import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { auditApi } from '@/api';
import type { AuditLogParams } from '@/api/types';
import { logger } from '@/utils/logger';

export function useAuditLogs(storyId?: string) {
  const [filters, setFilters] = useState<AuditLogParams>({});

  const queryParams = { ...filters, storyId };

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['audit-logs', queryParams],
    queryFn: () =>
      storyId
        ? auditApi.getByStory(storyId, filters)
        : auditApi.getLogs(filters),
  });

  const exportLogs = async () => {
    try {
      const blob = await auditApi.exportCsv(queryParams);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `audit-logs-${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (err) {
      logger.error('Failed to export audit logs:', err);
    }
  };

  return {
    logs: data?.items ?? [],
    total: data?.total ?? 0,
    page: data?.page ?? 1,
    pageSize: data?.pageSize ?? 20,
    hasMore: data?.hasMore ?? false,
    isLoading,
    error,
    filters,
    setFilters,
    refetch,
    exportLogs,
  };
}

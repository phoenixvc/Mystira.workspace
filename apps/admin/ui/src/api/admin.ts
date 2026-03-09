import { badgesApi } from "./badges";
import { bundlesApi } from "./bundles";
import { mediaApi } from "./media";
import { scenariosApi } from "./scenarios";

export interface DashboardStats {
  totalScenarios: number;
  totalMedia: number;
  totalBadges: number;
  totalBundles: number;
}

export const adminApi = {
  getStats: async (): Promise<DashboardStats> => {
    // Fetch stats from multiple endpoints and aggregate
    const [scenarios, media, badges, bundles] = await Promise.all([
      scenariosApi
        .getScenarios({ page: 1, pageSize: 1 })
        .catch(() => ({ totalCount: 0 })),
      mediaApi
        .getMedia({ page: 1, pageSize: 1 })
        .catch(() => ({ totalCount: 0 })),
      badgesApi
        .getBadges({ page: 1, pageSize: 1 })
        .catch(() => ({ totalCount: 0 })),
      bundlesApi
        .getBundles({ page: 1, pageSize: 1 })
        .catch(() => ({ totalCount: 0 })),
    ]);

    return {
      totalScenarios: scenarios.totalCount || 0,
      totalMedia: media.totalCount || 0,
      totalBadges: badges.totalCount || 0,
      totalBundles: bundles.totalCount || 0,
    };
  },
};

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  AgeGroup,
  ageGroupsApi,
  Archetype,
  archetypesApi,
  compassAxesApi,
  CompassAxis,
  EchoType,
  echoTypesApi,
  FantasyTheme,
  fantasyThemesApi,
} from "../api/masterData";
import ConfirmationDialog from "../components/ConfirmationDialog";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";
import { showToast } from "../utils/toast";

type MasterDataType =
  | "age-groups"
  | "archetypes"
  | "compass-axes"
  | "echo-types"
  | "fantasy-themes";

interface MasterDataPageConfig {
  title: string;
  icon: string;
  api: {
    getItems: (request?: { page?: number; pageSize?: number; searchTerm?: string }) => Promise<{
      items: unknown[];
      totalCount: number;
      page: number;
      pageSize: number;
    }>;
    deleteItem: (id: string) => Promise<void>;
  };
  getItemName: (item: unknown) => string;
  getItemDescription: (item: unknown) => string;
}

const masterDataConfigs: Record<MasterDataType, MasterDataPageConfig> = {
  "age-groups": {
    title: "Age Groups",
    icon: "👥",
    api: {
      getItems: ageGroupsApi.getAgeGroups,
      deleteItem: ageGroupsApi.deleteAgeGroup,
    },
    getItemName: item => (item as AgeGroup).name,
    getItemDescription: item => (item as AgeGroup).description || "-",
  },
  archetypes: {
    title: "Archetypes",
    icon: "🎭",
    api: {
      getItems: archetypesApi.getArchetypes,
      deleteItem: archetypesApi.deleteArchetype,
    },
    getItemName: item => (item as Archetype).name,
    getItemDescription: item => (item as Archetype).description || "-",
  },
  "compass-axes": {
    title: "Compass Axes",
    icon: "🧭",
    api: {
      getItems: compassAxesApi.getCompassAxes,
      deleteItem: compassAxesApi.deleteCompassAxis,
    },
    getItemName: item => (item as CompassAxis).name,
    getItemDescription: item => (item as CompassAxis).description || "-",
  },
  "echo-types": {
    title: "Echo Types",
    icon: "🔊",
    api: {
      getItems: echoTypesApi.getEchoTypes,
      deleteItem: echoTypesApi.deleteEchoType,
    },
    getItemName: item => (item as EchoType).name,
    getItemDescription: item => (item as EchoType).description || "-",
  },
  "fantasy-themes": {
    title: "Fantasy Themes",
    icon: "✨",
    api: {
      getItems: fantasyThemesApi.getFantasyThemes,
      deleteItem: fantasyThemesApi.deleteFantasyTheme,
    },
    getItemName: item => (item as FantasyTheme).name,
    getItemDescription: item => (item as FantasyTheme).description || "-",
  },
};

function MasterDataPage() {
  const { type } = useParams<{ type: MasterDataType }>();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; id: string | null }>({
    isOpen: false,
    id: null,
  });
  const queryClient = useQueryClient();

  const config = type && type in masterDataConfigs ? masterDataConfigs[type] : null;

  const { data, isLoading, error } = useQuery({
    queryKey: [type, page, pageSize, searchTerm],
    queryFn: () => {
      if (!config) throw new Error("Invalid config");
      return config.api.getItems({
        page,
        pageSize,
        searchTerm: searchTerm || undefined,
      });
    },
    enabled: !!config,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => {
      if (!config) throw new Error("Invalid config");
      return config.api.deleteItem(id);
    },
    onSuccess: () => {
      if (!type || !config) return;
      queryClient.invalidateQueries({ queryKey: [type] });
      showToast.success(`${config.title.slice(0, -1)} deleted successfully`);
    },
    onError: () => {
      if (!config) return;
      showToast.error(`Failed to delete ${config.title.toLowerCase()}`);
    },
  });

  if (!type || !(type in masterDataConfigs)) {
    return (
      <div className="alert alert-danger" role="alert">
        Invalid master data type
      </div>
    );
  }

  const handleDeleteClick = (id: string) => {
    setDeleteConfirm({ isOpen: true, id });
  };

  const handleDeleteConfirm = async () => {
    if (deleteConfirm.id) {
      try {
        await deleteMutation.mutateAsync(deleteConfirm.id);
        setDeleteConfirm({ isOpen: false, id: null });
      } catch {
        // Error handled by onError callback
      }
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirm({ isOpen: false, id: null });
  };

  const handleSearchReset = () => {
    setSearchTerm("");
    setPage(1);
  };

  if (isLoading) {
    return <LoadingSpinner message={`Loading ${config!.title.toLowerCase()}...`} />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title={`Error loading ${config!.title.toLowerCase()}`}
        onRetry={() => queryClient.invalidateQueries({ queryKey: [type] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <ConfirmationDialog
        isOpen={deleteConfirm.isOpen}
        title={`Delete ${config!.title.slice(0, -1)}`}
        message={`Are you sure you want to delete this ${config!.title.toLowerCase()}? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        variant="danger"
      />
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">
          {config!.icon} {config!.title}
        </h1>
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group me-2">
            <Link to={`/admin/master-data/${type}/create`} className="btn btn-sm btn-primary">
              <i className="bi bi-plus-circle"></i> Create
            </Link>
          </div>
        </div>
      </div>

      <SearchBar
        value={searchTerm}
        onChange={value => {
          setSearchTerm(value);
          setPage(1);
        }}
        placeholder={`Search ${config!.title.toLowerCase()}...`}
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.items.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Description</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((item: unknown) => {
                      const id = (item as { id: string }).id;
                      return (
                        <tr key={id}>
                          <td>{config!.getItemName(item)}</td>
                          <td>{config!.getItemDescription(item)}</td>
                          <td>
                            <div className="btn-group btn-group-sm">
                              <Link
                                to={`/admin/master-data/${type}/edit/${id}`}
                                className="btn btn-outline-primary"
                              >
                                <i className="bi bi-pencil"></i> Edit
                              </Link>
                              <button
                                className="btn btn-outline-danger"
                                onClick={() => handleDeleteClick(id)}
                                disabled={deleteMutation.isPending}
                              >
                                <i className="bi bi-trash"></i> Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              <Pagination currentPage={page} totalPages={totalPages} onPageChange={setPage} />
            </>
          ) : (
            <div className="text-center py-5">
              <p className="text-muted">No {config!.title.toLowerCase()} found.</p>
              <Link to={`/admin/master-data/${type}/create`} className="btn btn-primary">
                Create Your First {config!.title.slice(0, -1)}
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default MasterDataPage;

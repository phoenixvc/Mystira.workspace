import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import ErrorBoundary from "./components/ErrorBoundary";
import Layout from "./Layout";
import AccountsPage from "./pages/AccountsPage";
import AvatarsPage from "./pages/AvatarsPage";
import BadgesPage from "./pages/BadgesPage";
import BundlesPage from "./pages/BundlesPage";
import CharacterMapsPage from "./pages/CharacterMapsPage";
import CreateBundlePage from "./pages/CreateBundlePage";
import EditBundlePage from "./pages/EditBundlePage";
import CreateBadgePage from "./pages/CreateBadgePage";
import CreateCharacterMapPage from "./pages/CreateCharacterMapPage";
import CreateMasterDataPage from "./pages/CreateMasterDataPage";
import CreateScenarioPage from "./pages/CreateScenarioPage";
import DashboardPage from "./pages/DashboardPage";
import EditBadgePage from "./pages/EditBadgePage";
import EditCharacterMapPage from "./pages/EditCharacterMapPage";
import EditMasterDataPage from "./pages/EditMasterDataPage";
import EditScenarioPage from "./pages/EditScenarioPage";
import ErrorPage from "./pages/ErrorPage";
import ImportBadgePage from "./pages/ImportBadgePage";
import ImportBundlePage from "./pages/ImportBundlePage";
import ImportCharacterMapPage from "./pages/ImportCharacterMapPage";
import ImportMediaPage from "./pages/ImportMediaPage";
import ImportMediaZipPage from "./pages/ImportMediaZipPage";
import ImportScenarioPage from "./pages/ImportScenarioPage";
import LoginPage from "./pages/LoginPage";
import MasterDataPage from "./pages/MasterDataPage";
import MediaPage from "./pages/MediaPage";
import NotFoundPage from "./pages/NotFoundPage";
import ProfilesPage from "./pages/ProfilesPage";
import ScenariosPage from "./pages/ScenariosPage";
import ValidateScenariosPage from "./pages/ValidateScenariosPage";
import ProtectedRoute from "./ProtectedRoute";

function App() {
  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
            errorElement={<ErrorPage />}
          >
            <Route index element={<Navigate to="/admin" replace />} />
            <Route path="admin" element={<DashboardPage />} errorElement={<ErrorPage />} />
            <Route
              path="admin/scenarios"
              element={<ScenariosPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/scenarios/import"
              element={<ImportScenarioPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/scenarios/create"
              element={<CreateScenarioPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/scenarios/edit/:id"
              element={<EditScenarioPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/scenarios/validate"
              element={<ValidateScenariosPage />}
              errorElement={<ErrorPage />}
            />
            <Route path="admin/media" element={<MediaPage />} errorElement={<ErrorPage />} />
            <Route
              path="admin/media/import"
              element={<ImportMediaPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/media/import-zip"
              element={<ImportMediaZipPage />}
              errorElement={<ErrorPage />}
            />
            <Route path="admin/badges" element={<BadgesPage />} errorElement={<ErrorPage />} />
            <Route
              path="admin/badges/import"
              element={<ImportBadgePage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/badges/create"
              element={<CreateBadgePage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/badges/edit/:id"
              element={<EditBadgePage />}
              errorElement={<ErrorPage />}
            />
            <Route path="admin/bundles" element={<BundlesPage />} errorElement={<ErrorPage />} />
            <Route
              path="admin/bundles/create"
              element={<CreateBundlePage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/bundles/edit/:id"
              element={<EditBundlePage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/bundles/import"
              element={<ImportBundlePage />}
              errorElement={<ErrorPage />}
            />
            <Route path="admin/avatars" element={<AvatarsPage />} errorElement={<ErrorPage />} />
            <Route
              path="admin/character-maps"
              element={<CharacterMapsPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/character-maps/import"
              element={<ImportCharacterMapPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/character-maps/create"
              element={<CreateCharacterMapPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/character-maps/edit/:id"
              element={<EditCharacterMapPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/master-data/:type"
              element={<MasterDataPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/master-data/:type/create"
              element={<CreateMasterDataPage />}
              errorElement={<ErrorPage />}
            />
            <Route
              path="admin/master-data/:type/edit/:id"
              element={<EditMasterDataPage />}
              errorElement={<ErrorPage />}
            />
            <Route path="admin/accounts" element={<AccountsPage />} errorElement={<ErrorPage />} />
            <Route path="admin/profiles" element={<ProfilesPage />} errorElement={<ErrorPage />} />
          </Route>
          {/* 404 catch-all route */}
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </BrowserRouter>
    </ErrorBoundary>
  );
}

export default App;

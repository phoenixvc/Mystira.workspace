import { useMsal, useIsAuthenticated, useAccount } from "@azure/msal-react";
import { InteractionStatus, AccountInfo } from "@azure/msal-browser";
import { useCallback, useMemo } from "react";
import { loginRequest, tokenRequest, BYPASS_AUTH } from "./msalConfig";

// Mock account for bypass mode
const mockAccount: AccountInfo = {
  homeAccountId: "dev-home-account-id",
  localAccountId: "dev-local-account-id",
  environment: "dev",
  tenantId: "dev-tenant-id",
  username: "dev@local",
  name: "Development User",
} as AccountInfo;

export function useAuth() {
  // Always call hooks unconditionally (React rules)
  const { instance, inProgress, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(accounts[0]);

  const login = useCallback(async () => {
    if (BYPASS_AUTH) {
      console.warn("Login called but BYPASS_AUTH is enabled");
      return;
    }

    if (inProgress !== InteractionStatus.None) return;

    try {
      // Try popup first, fall back to redirect if blocked
      await instance.loginPopup(loginRequest);
    } catch (popupError) {
      console.warn("Popup login failed, trying redirect:", popupError);
      await instance.loginRedirect(loginRequest);
    }
  }, [instance, inProgress]);

  const logout = useCallback(async () => {
    if (BYPASS_AUTH) {
      console.warn("Logout called but BYPASS_AUTH is enabled");
      return;
    }

    if (inProgress !== InteractionStatus.None) return;

    try {
      await instance.logoutPopup({
        mainWindowRedirectUri: "/login",
      });
    } catch (popupError) {
      console.warn("Popup logout failed, trying redirect:", popupError);
      await instance.logoutRedirect({
        postLogoutRedirectUri: "/login",
      });
    }
  }, [instance, inProgress]);

  const getAccessToken = useCallback(async () => {
    if (BYPASS_AUTH) {
      return "dev-token";
    }

    if (!account) return null;

    try {
      const response = await instance.acquireTokenSilent({
        ...tokenRequest,
        account,
      });
      return response.accessToken;
    } catch (error) {
      console.error("Silent token acquisition failed:", error);
      // Fall back to interactive method
      try {
        const response = await instance.acquireTokenPopup(tokenRequest);
        return response.accessToken;
      } catch (interactiveError) {
        console.error("Interactive token acquisition failed:", interactiveError);
        return null;
      }
    }
  }, [instance, account]);

  // Return appropriate values based on bypass mode
  return useMemo(() => {
    if (BYPASS_AUTH) {
      return {
        isAuthenticated: true,
        isLoading: false,
        user: mockAccount,
        login,
        logout,
        getAccessToken,
      };
    }

    return {
      isAuthenticated,
      isLoading: inProgress !== InteractionStatus.None,
      user: account,
      login,
      logout,
      getAccessToken,
    };
  }, [isAuthenticated, inProgress, account, login, logout, getAccessToken]);
}

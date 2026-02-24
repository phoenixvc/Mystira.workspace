import { MsalProvider } from "@azure/msal-react";
import { EventType, AuthenticationResult } from "@azure/msal-browser";
import { msalInstance } from "./msalInstance";
import { ReactNode, useEffect, useState } from "react";
import { BYPASS_AUTH } from "./msalConfig";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [isInitialized, setIsInitialized] = useState(false);
  const [configError, setConfigError] = useState<string | null>(null);

  useEffect(() => {
    const initializeMsal = async () => {
      // If bypassing auth, skip MSAL initialization
      if (BYPASS_AUTH) {
        console.warn(
          "⚠️ Authentication bypassed - BYPASS_AUTH is enabled. This should only be used in development."
        );
        setIsInitialized(true);
        return;
      }

      // Check if client ID is configured
      const clientId = import.meta.env.VITE_AZURE_CLIENT_ID;
      if (!clientId) {
        setConfigError(
          "Azure AD Client ID is not configured. Please set VITE_AZURE_CLIENT_ID in your .env file, or enable BYPASS_AUTH for development."
        );
        setIsInitialized(true);
        return;
      }

      try {
        await msalInstance.initialize();

        // Handle redirect promise after login redirect
        const response = await msalInstance.handleRedirectPromise();
        if (response) {
          console.log("Login redirect successful", response);
          // Set active account
          msalInstance.setActiveAccount(response.account);
        } else {
          // Check if there are any accounts already signed in
          const accounts = msalInstance.getAllAccounts();
          if (accounts.length > 0 && accounts[0]) {
            msalInstance.setActiveAccount(accounts[0]);
          }
        }

        // Register event callbacks
        msalInstance.addEventCallback(event => {
          if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
            const payload = event.payload as AuthenticationResult;
            msalInstance.setActiveAccount(payload.account);
          }
        });

        setIsInitialized(true);
      } catch (error) {
        console.error("MSAL initialization failed:", error);
        setConfigError(
          `MSAL initialization failed: ${error instanceof Error ? error.message : String(error)}`
        );
        setIsInitialized(true); // Still set to true to prevent infinite loading
      }
    };

    initializeMsal();
  }, []);

  if (!isInitialized) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: "100vh" }}
      >
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (configError) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: "100vh" }}
      >
        <div className="container">
          <div className="row justify-content-center">
            <div className="col-md-6">
              <div className="card shadow">
                <div className="card-body p-5">
                  <div className="alert alert-danger" role="alert">
                    <h4 className="alert-heading">
                      <i className="bi bi-exclamation-triangle me-2"></i>
                      Authentication Configuration Error
                    </h4>
                    <p className="mb-0">{configError}</p>
                  </div>
                  <div className="mt-4">
                    <h5>To fix this issue:</h5>
                    <ol>
                      <li>
                        <strong>For production:</strong> Set <code>VITE_AZURE_CLIENT_ID</code> in
                        your environment variables
                      </li>
                      <li>
                        <strong>For development:</strong> Set <code>VITE_BYPASS_AUTH=true</code> in
                        your <code>.env</code> file
                      </li>
                    </ol>
                    <p className="text-muted mt-3">
                      <small>
                        See <code>.env.example</code> for required environment variables.
                      </small>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // If bypassing auth, render children without MSAL provider
  if (BYPASS_AUTH) {
    return <>{children}</>;
  }

  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
}

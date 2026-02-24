import { Configuration, LogLevel, PopupRequest } from "@azure/msal-browser";

// Check if authentication should be bypassed (for development)
export const BYPASS_AUTH = import.meta.env.VITE_BYPASS_AUTH === "true";

// Get Azure AD client ID - required unless BYPASS_AUTH is enabled
const clientId = import.meta.env.VITE_AZURE_CLIENT_ID || "";

// Validate configuration
if (!BYPASS_AUTH && !clientId) {
  console.error(
    "VITE_AZURE_CLIENT_ID is not set. Please set it in your .env file or enable BYPASS_AUTH for development."
  );
}

// MSAL configuration for Entra ID (Azure AD)
// These values should be set in environment variables
export const msalConfig: Configuration = {
  auth: {
    clientId: clientId || "00000000-0000-0000-0000-000000000000", // Dummy client ID if bypassing auth
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID || "common"}`,
    redirectUri: import.meta.env.VITE_AZURE_REDIRECT_URI || window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            break;
          case LogLevel.Warning:
            console.warn(message);
            break;
          case LogLevel.Info:
            console.info(message);
            break;
          case LogLevel.Verbose:
            console.debug(message);
            break;
        }
      },
      logLevel: LogLevel.Warning,
    },
  },
};

// Scopes for login request
export const loginRequest: PopupRequest = {
  scopes: ["User.Read"],
};

// Scopes for token request
// Note: This should be configured in environment variables
// Format: api://{your-api-client-id}/.default or api://{your-api-client-id}/access_as_user
export const tokenRequest = {
  scopes: [import.meta.env.VITE_AZURE_API_SCOPE || "User.Read"],
};

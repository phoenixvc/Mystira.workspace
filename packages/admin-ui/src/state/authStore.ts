import { create } from "zustand";

interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
}

// Simple localStorage-based persistence
const loadAuthFromStorage = (): { token: string | null } => {
  try {
    const stored = localStorage.getItem("auth-storage");
    if (stored) {
      const parsed = JSON.parse(stored);
      return { token: parsed.token || null };
    }
  } catch {
    // Ignore errors
  }
  return { token: null };
};

const saveAuthToStorage = (token: string | null) => {
  try {
    localStorage.setItem("auth-storage", JSON.stringify({ token }));
  } catch {
    // Ignore errors
  }
};

const initialState = loadAuthFromStorage();

export const useAuthStore = create<AuthState>(set => ({
  token: initialState.token,
  isAuthenticated: !!initialState.token,
  login: (token: string) => {
    saveAuthToStorage(token);
    set({ token, isAuthenticated: true });
  },
  logout: () => {
    localStorage.removeItem("auth-storage");
    set({ token: null, isAuthenticated: false });
  },
}));

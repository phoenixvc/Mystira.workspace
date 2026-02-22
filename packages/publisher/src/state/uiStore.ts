import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message?: string;
  duration?: number;
}

export type Theme = 'light' | 'dark' | 'system';

interface UIState {
  sidebarOpen: boolean;
  notifications: Notification[];
  theme: Theme;
  effectiveTheme: 'light' | 'dark';
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  setTheme: (theme: Theme) => void;
  toggleTheme: () => void;
  addNotification: (notification: Omit<Notification, 'id'>) => void;
  removeNotification: (id: string) => void;
  clearNotifications: () => void;
}

const getSystemTheme = (): 'light' | 'dark' => {
  if (typeof window === 'undefined') return 'light';
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};

const applyTheme = (theme: 'light' | 'dark') => {
  const root = document.documentElement;
  root.classList.remove('light', 'dark');
  root.classList.add(theme);
  root.setAttribute('data-theme', theme);
};

export const useUIStore = create<UIState>()(
  persist(
    (set, get) => {
      const systemTheme = getSystemTheme();
      
      // Initialize theme on first load
      if (typeof window !== 'undefined') {
        const stored = localStorage.getItem('ui-storage');
        let initialTheme: Theme = 'system';
        
        if (stored) {
          try {
            const parsed = JSON.parse(stored);
            initialTheme = (parsed.state?.theme || 'system') as Theme;
          } catch {
            initialTheme = 'system';
          }
        }
        
        const effectiveTheme = initialTheme === 'system' ? systemTheme : initialTheme;
        applyTheme(effectiveTheme);
      }

      // Listen for system theme changes
      if (typeof window !== 'undefined') {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handleChange = (e: MediaQueryListEvent) => {
          const state = get();
          if (state.theme === 'system') {
            const newTheme = e.matches ? 'dark' : 'light';
            set({ effectiveTheme: newTheme });
            applyTheme(newTheme);
          }
        };
        
        // Modern browsers
        if (mediaQuery.addEventListener) {
          mediaQuery.addEventListener('change', handleChange);
        } else {
          // Fallback for older browsers
          mediaQuery.addListener(handleChange);
        }
      }

      return {
        sidebarOpen: true,
        notifications: [],
        theme: 'system',
        effectiveTheme: systemTheme,

        toggleSidebar: () => set(state => ({ sidebarOpen: !state.sidebarOpen })),
        setSidebarOpen: open => set({ sidebarOpen: open }),

        setTheme: (theme: Theme) => {
          const effective = theme === 'system' ? getSystemTheme() : theme;
          applyTheme(effective);
          set({ theme, effectiveTheme: effective });
        },

        toggleTheme: () => {
          const state = get();
          const newTheme = state.effectiveTheme === 'light' ? 'dark' : 'light';
          applyTheme(newTheme);
          set({ theme: newTheme, effectiveTheme: newTheme });
        },

        addNotification: notification =>
          set(state => ({
            notifications: [
              ...state.notifications,
              { ...notification, id: crypto.randomUUID() },
            ],
          })),

        removeNotification: id =>
          set(state => ({
            notifications: state.notifications.filter(n => n.id !== id),
          })),

        clearNotifications: () => set({ notifications: [] }),
      };
    },
    {
      name: 'ui-storage',
      partialize: state => ({ theme: state.theme }),
      onRehydrateStorage: () => (state) => {
        if (state && typeof window !== 'undefined') {
          const effective = state.theme === 'system' ? getSystemTheme() : state.theme;
          applyTheme(effective);
          state.effectiveTheme = effective;
        }
      },
    }
  )
);

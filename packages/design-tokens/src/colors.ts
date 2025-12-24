/**
 * Mystira Color Palette
 * Primary: Purple (#7c3aed) - Unified across all packages
 * Based on Publisher's comprehensive token system
 */

export const colors = {
  // Primary Purple (brand color)
  primary: {
    50: '#faf5ff',
    100: '#f3e8ff',
    200: '#e9d5ff',
    300: '#d8b4fe',
    400: '#c084fc',
    500: '#a855f7',
    600: '#9333ea',
    700: '#7c3aed', // Main brand color
    800: '#6b21a8',
    900: '#581c87',
    950: '#3b0764',
    DEFAULT: '#7c3aed', // Convenience accessor for the main brand color
  },

  // Neutral Gray
  neutral: {
    50: '#fafafa',
    100: '#f4f4f5',
    200: '#e4e4e7',
    300: '#d4d4d8',
    400: '#a1a1aa',
    500: '#71717a',
    600: '#52525b',
    700: '#3f3f46',
    800: '#27272a',
    900: '#18181b',
    950: '#09090b',
  },

  // Semantic colors
  success: {
    50: '#f0fdf4',
    100: '#dcfce7',
    200: '#bbf7d0',
    300: '#86efac',
    400: '#4ade80',
    500: '#22c55e',
    600: '#16a34a',
    700: '#15803d',
    800: '#166534',
    900: '#14532d',
  },

  warning: {
    50: '#fffbeb',
    100: '#fef3c7',
    200: '#fde68a',
    300: '#fcd34d',
    400: '#fbbf24',
    500: '#f59e0b',
    600: '#d97706',
    700: '#b45309',
    800: '#92400e',
    900: '#78350f',
  },

  danger: {
    50: '#fef2f2',
    100: '#fee2e2',
    200: '#fecaca',
    300: '#fca5a5',
    400: '#f87171',
    500: '#ef4444',
    600: '#dc2626',
    700: '#b91c1c',
    800: '#991b1b',
    900: '#7f1d1d',
  },

  info: {
    50: '#eff6ff',
    100: '#dbeafe',
    200: '#bfdbfe',
    300: '#93c5fd',
    400: '#60a5fa',
    500: '#3b82f6',
    600: '#2563eb',
    700: '#1d4ed8',
    800: '#1e40af',
    900: '#1e3a8a',
  },
} as const;

export type ColorScale = typeof colors.primary;
export type ColorName = keyof typeof colors;

/**
 * Semantic color mappings for light mode
 */
export const lightModeColors = {
  // Backgrounds
  background: {
    primary: colors.neutral[50],
    secondary: colors.neutral[100],
    tertiary: colors.neutral[200],
    elevated: '#ffffff',
    inverse: colors.neutral[900],
  },
  // Foregrounds / Text
  foreground: {
    primary: colors.neutral[900],
    secondary: colors.neutral[600],
    tertiary: colors.neutral[500],
    inverse: colors.neutral[50],
    muted: colors.neutral[400],
  },
  // Borders
  border: {
    default: colors.neutral[200],
    strong: colors.neutral[300],
    muted: colors.neutral[100],
  },
  // Interactive states
  interactive: {
    default: colors.primary[700],
    hover: colors.primary[800],
    active: colors.primary[900],
    focus: colors.primary[600],
  },
} as const;

/**
 * Semantic color mappings for dark mode
 */
export const darkModeColors = {
  // Backgrounds (inverted neutral scale)
  background: {
    primary: colors.neutral[900],
    secondary: colors.neutral[800],
    tertiary: colors.neutral[700],
    elevated: colors.neutral[800],
    inverse: colors.neutral[50],
  },
  // Foregrounds / Text
  foreground: {
    primary: colors.neutral[50],
    secondary: colors.neutral[300],
    tertiary: colors.neutral[400],
    inverse: colors.neutral[900],
    muted: colors.neutral[500],
  },
  // Borders
  border: {
    default: colors.neutral[700],
    strong: colors.neutral[600],
    muted: colors.neutral[800],
  },
  // Interactive states (slightly lighter for dark mode contrast)
  interactive: {
    default: colors.primary[500],
    hover: colors.primary[400],
    active: colors.primary[600],
    focus: colors.primary[400],
  },
} as const;

/**
 * Semantic colors structure type (uses string to allow different color values per theme)
 */
export interface SemanticColors {
  readonly background: {
    readonly primary: string;
    readonly secondary: string;
    readonly tertiary: string;
    readonly elevated: string;
    readonly inverse: string;
  };
  readonly foreground: {
    readonly primary: string;
    readonly secondary: string;
    readonly tertiary: string;
    readonly inverse: string;
    readonly muted: string;
  };
  readonly border: {
    readonly default: string;
    readonly strong: string;
    readonly muted: string;
  };
  readonly interactive: {
    readonly default: string;
    readonly hover: string;
    readonly active: string;
    readonly focus: string;
  };
}

export type ThemeMode = 'light' | 'dark';

/**
 * Get semantic colors for a given theme mode
 */
export function getSemanticColors(mode: ThemeMode): SemanticColors {
  return mode === 'dark' ? darkModeColors : lightModeColors;
}

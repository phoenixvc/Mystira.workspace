/**
 * Mystira Typography System
 * Based on Publisher's comprehensive token system
 */

export const fontFamily = {
  sans: ['Inter', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
  mono: ['Fira Code', 'JetBrains Mono', 'Menlo', 'Monaco', 'Consolas', 'monospace'],
  display: ['Inter', 'system-ui', 'sans-serif'],
} as const;

export const fontSize = {
  xs: ['0.75rem', { lineHeight: '1rem' }],      // 12px
  sm: ['0.875rem', { lineHeight: '1.25rem' }],   // 14px
  base: ['1rem', { lineHeight: '1.5rem' }],      // 16px
  lg: ['1.125rem', { lineHeight: '1.75rem' }],   // 18px
  xl: ['1.25rem', { lineHeight: '1.75rem' }],    // 20px
  '2xl': ['1.5rem', { lineHeight: '2rem' }],     // 24px
  '3xl': ['1.875rem', { lineHeight: '2.25rem' }], // 30px
  '4xl': ['2.25rem', { lineHeight: '2.5rem' }],  // 36px
  '5xl': ['3rem', { lineHeight: '1' }],          // 48px
  '6xl': ['3.75rem', { lineHeight: '1' }],       // 60px
} as const;

export const fontWeight = {
  normal: '400',
  medium: '500',
  semibold: '600',
  bold: '700',
} as const;

export const letterSpacing = {
  tighter: '-0.05em',
  tight: '-0.025em',
  normal: '0em',
  wide: '0.025em',
  wider: '0.05em',
  widest: '0.1em',
} as const;

export const lineHeight = {
  none: '1',
  tight: '1.25',
  snug: '1.375',
  normal: '1.5',
  relaxed: '1.625',
  loose: '2',
} as const;

export type FontFamily = keyof typeof fontFamily;
export type FontSize = keyof typeof fontSize;
export type FontWeight = keyof typeof fontWeight;

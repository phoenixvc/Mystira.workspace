/**
 * @mystira/design-tokens
 * Unified design tokens for Mystira platform
 */

export * from './colors';
export * from './typography';
export * from './spacing';
export * from './components';

// Re-export as grouped objects for convenience
import { colors } from './colors';
import { fontFamily, fontSize, fontWeight, letterSpacing, lineHeight } from './typography';
import { spacing } from './spacing';
import { borderRadius, boxShadow, transition, transitionDuration, transitionTimingFunction, zIndex, opacity } from './components';

export const tokens = {
  colors,
  fontFamily,
  fontSize,
  fontWeight,
  letterSpacing,
  lineHeight,
  spacing,
  borderRadius,
  boxShadow,
  transition,
  transitionDuration,
  transitionTimingFunction,
  zIndex,
  opacity,
} as const;

export default tokens;

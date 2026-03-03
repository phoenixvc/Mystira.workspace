#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../../.."
API_SPEC_DIR="$SCRIPT_DIR/.."
OUTPUT_DIR="$ROOT_DIR/packages/contracts/src/generated"

echo "=== Mystira TypeScript Contract Generation ==="
echo "API Spec Dir: $API_SPEC_DIR"
echo "Output Dir: $OUTPUT_DIR"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

echo ""
echo "Generating TypeScript contracts from OpenAPI specs..."

# Generate from App API
echo "  → Generating App API types..."
npx openapi-typescript "$API_SPEC_DIR/openapi/app-api.yaml" \
  --output "$OUTPUT_DIR/app-api.ts" \
  --export-type \
  --immutable

# Generate from Story Generator API
echo "  → Generating Story Generator API types..."
npx openapi-typescript "$API_SPEC_DIR/openapi/story-generator-api.yaml" \
  --output "$OUTPUT_DIR/story-generator-api.ts" \
  --export-type \
  --immutable

# Create index file
cat > "$OUTPUT_DIR/index.ts" << 'EOF'
/**
 * Auto-generated API contracts from OpenAPI specifications.
 * DO NOT EDIT DIRECTLY - regenerate using `pnpm generate` in packages/api-spec
 */

export * as AppApi from './app-api';
export * as StoryGeneratorApi from './story-generator-api';
EOF

echo ""
echo "✓ TypeScript contracts generated successfully at $OUTPUT_DIR"

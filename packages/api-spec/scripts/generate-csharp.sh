#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../../.."
API_SPEC_DIR="$SCRIPT_DIR/.."
OUTPUT_DIR="$ROOT_DIR/packages/contracts/dotnet/Mystira.Contracts/Generated"

echo "=== Mystira C# Contract Generation ==="
echo "API Spec Dir: $API_SPEC_DIR"
echo "Output Dir: $OUTPUT_DIR"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Check if nswag is available
if ! command -v nswag &> /dev/null; then
    echo "Installing NSwag.ConsoleCore..."
    dotnet tool install --global NSwag.ConsoleCore || true
fi

echo ""
echo "Generating C# contracts from OpenAPI specs..."

# Generate from App API
echo "  → Generating App API contracts..."
nswag openapi2csclient \
  /input:"$API_SPEC_DIR/openapi/app-api.yaml" \
  /output:"$OUTPUT_DIR/AppApiContracts.cs" \
  /namespace:Mystira.Contracts.Generated.App \
  /className:AppApiClient \
  /generateClientInterfaces:true \
  /generateDtoTypes:true \
  /dateType:System.DateTimeOffset \
  /dateTimeType:System.DateTimeOffset \
  /arrayType:System.Collections.Generic.IReadOnlyList \
  /generateOptionalParameters:true \
  /generateNullableReferenceTypes:true

# Generate from Story Generator API
echo "  → Generating Story Generator API contracts..."
nswag openapi2csclient \
  /input:"$API_SPEC_DIR/openapi/story-generator-api.yaml" \
  /output:"$OUTPUT_DIR/StoryGeneratorApiContracts.cs" \
  /namespace:Mystira.Contracts.Generated.StoryGenerator \
  /className:StoryGeneratorApiClient \
  /generateClientInterfaces:true \
  /generateDtoTypes:true \
  /dateType:System.DateTimeOffset \
  /dateTimeType:System.DateTimeOffset \
  /arrayType:System.Collections.Generic.IReadOnlyList \
  /generateOptionalParameters:true \
  /generateNullableReferenceTypes:true

echo ""
echo "✓ C# contracts generated successfully at $OUTPUT_DIR"

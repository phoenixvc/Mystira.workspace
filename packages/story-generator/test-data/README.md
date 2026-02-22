# Test Data

This directory contains test files and scripts for the Mystira Story Generator project.

## Test Files

### YAML Test Files

- **[test-story.yaml](test-story.yaml)** - Valid story example
  - Complete story with all required fields
  - Demonstrates proper YAML structure
  - Includes characters, scenes, and metadata
  - Use this as a reference for creating new stories

- **[test-story-invalid.yaml](test-story-invalid.yaml)** - Invalid story example
  - Demonstrates various validation errors
  - Missing required fields
  - Useful for testing error handling

## Test Scripts

- **[test-import-feature.sh](test-import-feature.sh)** - YAML import feature test script
  - Builds the web project
  - Validates test YAML files
  - Checks implementation files
  - Can be run from anywhere in the repository

### Running the Test Script

```bash
# From repository root
./test-data/test-import-feature.sh

# Or from any directory
cd /path/to/Mystira.StoryGenerator
./test-data/test-import-feature.sh
```

## Using Test Files

### Testing YAML Import Feature

1. Start the API and Web applications:
   ```bash
   dotnet run --project src/Mystira.StoryGenerator.Api &
   dotnet run --project src/Mystira.StoryGenerator.Web
   ```

2. In the web application:
   - Click "Import from Clipboard" after copying the contents of `test-story.yaml`
   - Or click "Import from File" and select `test-story.yaml`

3. Test error handling with `test-story-invalid.yaml` to see validation feedback

### Manual Validation Testing

Test the validation API directly:
```bash
curl -X POST http://localhost:5000/api/stories/validate \
  -H "Content-Type: application/json" \
  -d @test-story.yaml
```

## Creating New Test Files

When creating new test YAML files:

1. Follow the schema defined in `src/Mystira.StoryGenerator.Api/config/story-schema.json`
2. Include all required fields: title, description, difficulty, etc.
3. Add meaningful examples that test specific features or edge cases
4. Document the purpose of the test file in this README

## Related Documentation

- [YAML Import Feature Documentation](../docs/YAML_IMPORT_FEATURE.md)
- [Schema Validation Documentation](../docs/SCHEMA_VALIDATION_IMPLEMENTATION.md)
- [Main README](../README.md)

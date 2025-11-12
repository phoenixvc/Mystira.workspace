# YAML Import Feature

## Overview

The Mystira Story Generator now includes a YAML import feature that allows users to import stories directly from YAML files or clipboard content. This feature is accessible from the left-hand panel in the UI and enables quick story creation from pre-formatted YAML documents.

## Features

### 1. Import from Clipboard
- Click the import button (📥) in the sidebar header
- Select "From Clipboard" from the dropdown menu
- The application reads YAML content from your system clipboard
- Validates the YAML structure and displays a summary if valid
- Shows an error message if the YAML is invalid

### 2. Import from File
- Click the import button (📥) in the sidebar header
- Select "From File" from the dropdown menu
- Browse and select a `.yaml` or `.yml` file from your computer
- File size limit: 1MB
- Validates the YAML structure and displays a summary if valid
- Shows an error message if the YAML is invalid

## YAML Structure Requirements

For a story YAML to be valid, it must include these required fields:

- **title**: The name of the story
- **description**: A detailed description of the story

Optional fields that will be included in the summary:
- **difficulty**: Story difficulty level (e.g., "Easy", "Medium", "Hard")
- **age_group**: Target age group (e.g., "6-9", "10-12")
- **session_length**: Recommended session duration (e.g., "Short", "Medium", "Long")
- **tags**: Story tags or categories
- **characters**: List of characters in the story
- **scenes**: List of scenes in the story

## Example YAML Structure

```yaml
title: "The Enchanted Forest Adventure"
description: "A magical journey through an enchanted forest where young heroes must help woodland creatures solve their problems using wisdom, courage, and friendship."
difficulty: "Medium"
age_group: "6-9"
session_length: "Medium"
tags:
  - adventure
  - magic
  - friendship
characters:
  - name: "Lily the Young Explorer"
    role: "protagonist"
  - name: "Whiskers the Wise Rabbit"
    role: "mentor"
scenes:
  - id: "forest_entrance"
    title: "Entering the Enchanted Forest"
    type: "narrative"
  - id: "meet_whiskers"
    title: "Meeting Whiskers"
    type: "choice"
```

## Workflow

1. **Import YAML**: User clicks the import button and chooses source (clipboard or file)
2. **Validation**: System validates the YAML structure against required fields
3. **Display Results**: 
   - If valid: Shows story summary with title, description, and other details
   - If invalid: Shows specific error messages about missing required fields
4. **Create Chat Session**: User clicks "Continue to Chat" to create a new chat session
5. **Session Creation**: A new chat session is created with:
   - The story summary as an initial system message
   - The complete YAML stored as a snapshot in the session
   - User can now chat and interact with the story

## Implementation Details

### Components

- **SessionSidebar.razor**: Main UI component with import buttons and modals
- **YamlImportService.cs**: Service handling YAML parsing and validation

### Key Methods

- `ImportFromClipboard()`: Reads from system clipboard using JavaScript interop
- `TriggerFileInput()`: Opens file browser for selection
- `HandleFileSelect()`: Processes selected file
- `ImportYamlAsync()`: Validates and parses YAML content
- `ConfirmImport()`: Creates new chat session with imported story

### JavaScript Functions

Added to `wwwroot/js/clipboard.js`:
- `mystiraClipboard.readText()`: Reads text from system clipboard

## Error Handling

The import feature includes comprehensive error handling for:
- Empty clipboard or file content
- Clipboard permission issues
- Invalid YAML syntax
- Missing required fields
- File read errors
- File size exceeding limits

## UI/UX Features

- **Import Menu**: Dropdown menu with import options
- **Modal Dialogs**: Clear visual feedback for success/error
- **Story Summary**: Formatted display of imported story details
- **Visual Indicators**: Icons and colors to indicate status
- **Responsive Design**: Works on desktop and mobile layouts

## Limitations

1. Basic YAML parsing: Supports simple key-value structure
2. File size limit: 1MB maximum
3. Clipboard access: May require user permission in some browsers
4. Supported file types: `.yaml` and `.yml` files only

## Future Enhancements

Potential improvements for future versions:
- More sophisticated YAML parsing with nested structures
- Batch import of multiple stories
- YAML template generator
- Export current story as YAML
- Validation against Mystira story schema

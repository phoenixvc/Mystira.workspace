# Enhanced Schema Support for RAG Indexer

## Overview

The RAG indexer has been enhanced to support comprehensive instruction categorization using field enums, enabling better organization, filtering, and retrieval of instruction chunks for the Mystira Story Generator.

## Enhanced Schema Features

### New Field Enums Support

#### 1. Instruction Categorization
- **category**: High-level pipeline step
  - `story_generation`: Core story creation and structure rules
  - `validation`: Content validation and quality checks
  - `autofix`: Automated correction mechanisms
  - `summarization`: Content summary and condensation
  - `config`: Configuration and setup instructions
  - `safety`: Content safety and compliance rules
  - `meta`: Metadata and organizational rules

- **subcategory**: Topic/area of rule
  - `core_story_rules`: Fundamental story mechanics
  - `tone_and_humour`: Writing style and tone guidelines
  - `scene_types_and_rolls`: Scene structure and dice mechanics
  - `character_and_archetypes`: Character development rules
  - `narrative_causality`: Cause-and-effect relationships
  - `compass_scoring`: Progress tracking systems
  - `focus_vs_microchoices`: Decision impact mechanics
  - `cumulative_consequences`: Long-term choice effects
  - `developmental_goals`: Educational objectives
  - `educational_goals`: Learning outcomes
  - `gameplay_and_tone`: Game balance and atmosphere
  - `smart_roster`: Character management systems
  - `developmental_link`: Character progression
  - `field_enums`: Field validation rules

#### 2. Instruction Type Classification
- **instructionType**: Specific type of instruction
  - `requirements`: Mandatory story rules
  - `guidelines`: Best practices and recommendations
  - `examples`: Illustrative use cases
  - `validation`: Content checking rules
  - `schema_docs`: Field documentation

#### 3. Priority Management
- **priority**: Importance level for RAG weighting
  - `high`: Critical story requirements
  - `normal`: Standard guidelines
  - `low`: Optional enhancements

#### 4. Metadata and Versioning
- **isMandatory**: Whether instruction is required
- **source**: Origin of instruction content
- **version**: Semantic versioning for updates
- **createdAt/updatedAt**: Timestamp tracking

#### 5. Enhanced Tagging
- **tags**: Flexible, descriptive labels
- **examples**: Contextual usage samples
- **keywords**: Search optimization terms

## Azure AI Search Schema Enhancements

### Comprehensive Field Support

#### Primary Identification
```csharp
new SimpleField("id", SearchFieldDataType.String) { IsKey = true }
```

#### Content and Searchability
```csharp
new SearchField("content", SearchFieldDataType.String) 
{ 
    IsSearchable = true, 
    IsFilterable = false,
    AnalyzerName = "standard.lucene"
}
```

#### Categorization Fields
```csharp
new SimpleField("category", SearchFieldDataType.String) 
{ 
    IsFilterable = true, 
    IsFacetable = true 
},

new SimpleField("subcategory", SearchFieldDataType.String) 
{ 
    IsFilterable = true, 
    IsFacetable = true 
},

new SimpleField("instructionType", SearchFieldDataType.String) 
{ 
    IsFilterable = true, 
    IsFacetable = true 
},

new SimpleField("priority", SearchFieldDataType.String) 
{ 
    IsFilterable = true, 
    IsFacetable = true 
},

new SimpleField("isMandatory", SearchFieldDataType.Boolean) 
{ 
    IsFilterable = true 
}
```

#### Context and Examples
```csharp
new SearchField("examples", SearchFieldDataType.String) 
{ 
    IsFilterable = false 
},

new SimpleField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) 
{ 
    IsFilterable = true, 
    IsFacetable = true 
}
```

#### Metadata Management
```csharp
new SimpleField("source", SearchFieldDataType.String) 
{ 
    IsFilterable = true 
},

new SimpleField("version", SearchFieldDataType.String) 
{ 
    IsFilterable = true 
},

new SimpleField("createdAt", SearchFieldDataType.DateTimeOffset) 
{ 
    IsFilterable = true, 
    IsSortable = true 
},

new SimpleField("updatedAt", SearchFieldDataType.DateTimeOffset) 
{ 
    IsFilterable = true, 
    IsSortable = true 
}
```

#### Vector Search Integration
```csharp
new SearchField("embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single)) 
{ 
    IsSearchable = true, 
    IsFilterable = false,
    VectorSearchDimensions = 1536,
    VectorSearchProfileName = "my-vector-config"
}
```

### Backward Compatibility

The schema maintains compatibility with existing implementations by including legacy fields:
- `chunk_id`, `section`, `dataset`, `keywords` for existing code
- New `id` field as primary key for enhanced implementations

## Enhanced JSON Format

### Complete Instruction Chunk Example
```json
{
  "chunk_id": "core_story_requirements",
  "section": "1.1",
  "title": "Core Story Requirements",
  "content": "Stories must include a minimum level of interactivity...",
  "category": "story_generation",
  "subcategory": "core_story_rules",
  "instructionType": "requirements",
  "priority": "high",
  "isMandatory": true,
  "examples": "Minimum 5 decision scenes, character Action Choices...",
  "tags": ["story_design", "interactivity", "roll_scenes"],
  "source": "mystira_instruction_schema",
  "version": "1.0",
  "createdAt": "2025-11-19T00:00:00Z",
  "updatedAt": "2025-11-19T00:00:00Z",
  "keywords": ["story_design", "interactivity", "roll_scenes"]
}
```

## Benefits for RAG Implementation

### 1. Enhanced Retrieval Precision
- **Category Filtering**: Retrieve instructions by story generation phase
- **Priority Weighting**: Emphasize high-priority requirements in results
- **Instruction Type Targeting**: Get specific types (requirements vs guidelines)
- **Mandatory Filtering**: Ensure critical rules are always included

### 2. Improved Context Management
- **Subcategory Organization**: Group related instructions for better context
- **Tag-based Search**: Flexible discovery through descriptive tags
- **Version Control**: Track instruction evolution and updates
- **Source Tracking**: Identify origin and validity of content

### 3. Better RAG Results
- **Semantic Coherence**: Group related instructions together
- **Progressive Disclosure**: Layer from core requirements to specific examples
- **Educational Alignment**: Match learning objectives to content
- **Compliance Checking**: Filter based on mandatory flags

### 4. Future-Proof Design
- **Extensible Schema**: Easy to add new categories and types
- **Standardized Values**: Consistent enums for all fields
- **Versioning Support**: Built-in update tracking
- **Metadata Rich**: Comprehensive context for AI understanding

## Implementation Notes

### Field Validation
The enhanced schema includes validation for:
- Required fields (id, content, category, subcategory)
- Enum value constraints for categorization fields
- Proper data types for all fields
- Timestamp management for versioning

### Search Optimization
- Standard.lucene analyzer for content tokenization
- Facetable fields for filtering and aggregation
- Vector search with HNSW algorithm for performance
- Sortable timestamp fields for chronological queries

### Backward Compatibility
- Legacy field support for existing implementations
- Graceful migration path from old to new schema
- Dual field population during transition period
- Clear mapping between old and new field names

This enhanced schema provides a robust foundation for implementing sophisticated RAG systems that can effectively categorize, retrieve, and utilize instruction chunks for the Mystira Story Generator.
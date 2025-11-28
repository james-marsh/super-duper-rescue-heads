# Feature Specification: Custom Item Types

**Feature ID**: 005-custom-item-types
**Created**: 2025-11-28
**Status**: Draft
**Dependencies**: Feature 001 (Collection Management), Feature 002 (Item Management)

## Overview

Allow users to define custom item types with flexible attribute schemas beyond the built-in types (Vinyl, Comic, Puzzle). Users can create reusable type templates that define which attributes are required or optional, with data type validation and constraints. This enables collectors to organize diverse items (coins, stamps, watches, vintage cameras, etc.) with type-specific metadata.

## User Scenarios & Testing

### Primary User Flow

**Scenario 1: Collector creates custom "Vintage Watch" type**
1. User navigates to "Manage Item Types" section
2. Clicks "Create Custom Type"
3. Enters type name "Vintage Watch"
4. Defines attributes:
   - Manufacturer (text, required)
   - Model Number (text, required)
   - Movement Type (choice: Automatic/Manual/Quartz, required)
   - Case Material (choice: Gold/Silver/Steel/Titanium, optional)
   - Diameter (number in mm, optional)
   - Production Year (number 1800-2024, optional)
5. Saves the custom type
6. Creates new item using "Vintage Watch" type
7. System validates required attributes (Manufacturer, Model Number, Movement Type)
8. Item is saved with type-specific structure

**Expected outcome**: User can consistently catalog watches with standardized attributes across their collection

**Scenario 2: User edits existing custom type (non-destructive)**
1. User has 15 "Coin" items using custom "Coin" type
2. User edits "Coin" type to add optional "Mint Mark" attribute
3. System updates type schema without affecting existing items
4. Existing coins remain valid (new attribute is optional)
5. New coins can include "Mint Mark" data

**Expected outcome**: Type evolution doesn't break existing items

**Scenario 3: User attempts to delete type with items**
1. User has 20 items using "Stamp" custom type
2. User attempts to delete "Stamp" type
3. System prevents deletion with message: "Cannot delete type 'Stamp' - 20 items are using this type"
4. User must reassign or delete items first

**Expected outcome**: Data integrity maintained through referential constraints

### Edge Cases

1. **Empty type name**: System rejects with "Type name is required"
2. **Duplicate type name**: System rejects with "Type name 'Vinyl' already exists"
3. **Maximum types limit**: User with 50 custom types cannot create more (shows "Maximum 50 custom types reached")
4. **Maximum attributes limit**: User cannot add 31st attribute (shows "Maximum 30 attributes per type")
5. **Invalid attribute constraints**:
   - Number range min > max → "Minimum value cannot exceed maximum"
   - Text max length < 1 → "Maximum length must be at least 1"
6. **Type with no attributes**: Allowed (uses only built-in Item fields)
7. **Attribute name conflicts**: System rejects duplicate attribute names within same type

## Functional Requirements

### FR1: Custom Type Management

**Must Have:**
- Users can create custom item types with unique names (2-50 characters)
- Users can edit custom type names and descriptions
- Users can archive custom types (soft delete) if no items reference them
- Users can permanently delete archived types after 30 days
- System enforces maximum 50 custom types per user
- Type names must be unique within user's scope
- Built-in types (Vinyl, Comic, Puzzle) cannot be modified or deleted

**Acceptance Criteria:**
- Creating duplicate type name shows error before save
- Archived types don't appear in item creation dropdowns
- Attempting to delete type with items shows count and prevents deletion
- Type list shows usage count (number of items using each type)

### FR2: Attribute Schema Definition

**Must Have:**
- Each custom type can define 0-30 custom attributes
- Each attribute has:
  - Name (2-50 characters, unique within type)
  - Data type (Text, Number, Date, Choice, Boolean)
  - Required/Optional flag
  - Validation constraints (min/max length, min/max value, allowed choices)
  - Help text (0-200 characters, optional)
- Text attributes:
  - Minimum length 0-1000
  - Maximum length 1-1000
  - Default value (optional)
- Number attributes:
  - Minimum value (optional)
  - Maximum value (optional)
  - Decimal places (0-4)
  - Default value (optional)
- Date attributes:
  - Minimum date (optional)
  - Maximum date (optional)
  - Default to today (boolean)
- Choice attributes:
  - 2-50 choices
  - Each choice 1-100 characters
  - Allow multiple selections (boolean)
- Boolean attributes:
  - Default value (true/false/null)

**Acceptance Criteria:**
- Invalid constraints (min > max) are rejected with clear error messages
- Choice lists with <2 or >50 choices are rejected
- Attribute names are case-insensitive unique within type
- Changes to required/optional don't break existing items

### FR3: Type-Based Item Validation

**Must Have:**
- When creating item with custom type, system validates all required attributes are provided
- System validates attribute values against data type and constraints
- Validation errors show field name, constraint, and expected format
- Items created before schema changes remain valid (backwards compatibility)
- Schema changes that add required attributes only apply to new items

**Acceptance Criteria:**
- Missing required attribute prevents save with specific field name in error
- Number outside min/max range shows "Must be between X and Y"
- Text exceeding max length shows "Must be at most X characters"
- Invalid choice selection shows "Must be one of: [choices]"
- Date outside range shows "Must be between [min] and [max]"

### FR4: Type Templates & Reusability

**Must Have:**
- Users can duplicate existing custom types to create new types
- Duplicating preserves attribute definitions and constraints
- System provides 5 common type templates:
  - Coin (Denomination, Year, Mint Mark, Country, Grade)
  - Stamp (Country, Year, Denomination, Condition, Catalog Number)
  - Watch (Brand, Model, Movement, Case Material, Year)
  - Camera (Brand, Model, Format, Lens Mount, Year)
  - Book (Title, Author, Publisher, ISBN, Edition, Year)

**Acceptance Criteria:**
- Duplicated type name defaults to "[Original Name] Copy"
- Template application creates new custom type with predefined attributes
- Users can modify template-created types like any custom type

## Success Criteria

**Measurable Outcomes:**
1. **Adoption**: 60% of users create at least 1 custom type within first month
2. **Usage**: Average 3-5 custom types per active user
3. **Validation**: 95% of validation errors are resolved on first correction attempt
4. **Performance**: Type schema validation completes in <50ms for 30 attributes
5. **Reliability**: Zero data loss when editing type schemas
6. **User Satisfaction**: 80% of users rate custom types as "Very Useful" or "Essential"

**Qualitative Outcomes:**
- Users can organize any collectible type without developer intervention
- Type schemas enforce data consistency across collections
- Attribute validation prevents data entry errors
- Template system accelerates common use cases

## Non-Functional Requirements

### Performance
- Type list loads in <200ms for 50 types
- Item creation with type validation <300ms
- Schema updates propagate to UI in <100ms

### Usability
- Type creation wizard guides users through attribute definition
- Inline validation provides immediate feedback
- Help text supports users in understanding attribute purposes
- Error messages are specific and actionable

### Scalability
- Support 50 custom types per user
- Support 30 attributes per type
- Support 100,000 items across all custom types per user

### Data Integrity
- Type deletion prevented if items reference it
- Schema changes preserve existing item validity
- Attribute removal marks field as deprecated (not deleted)

## Out of Scope

- Sharing custom types between users (deferred to Feature 006)
- Importing type schemas from external sources
- Attribute formulas or calculated fields
- Conditional validation (show/hide fields based on other values)
- Nested/hierarchical attribute structures
- Attribute-level permissions
- Type versioning or change history (beyond basic audit trail)

## Assumptions

1. Users understand basic data types (text, number, date, boolean, choice)
2. Most users will create 3-10 custom types maximum
3. Attribute constraints are simple (min/max, required/optional) - no complex regex
4. Types are user-scoped (not shared between users initially)
5. Built-in types (Vinyl, Comic, Puzzle) cover common cases - custom types are for specialized needs
6. JSON storage is acceptable for flexible schema definitions
7. Schema validation errors can be handled client-side before submission

## Dependencies

- **Feature 001 (Collection Management)**: Custom types exist within user context
- **Feature 002 (Item Management)**: Items reference custom types; existing JSON attributes field is foundation
- **EF Core 9.0**: JSON column support for schema storage
- **FluentValidation** (or similar): Attribute validation logic

## Key Entities

### ItemTypeSchema (Aggregate Root)
- ItemTypeSchemaId (Guid, PK)
- UserId (Guid, FK) - owner
- TypeName (string 2-50 chars, unique per user)
- Description (string 0-500 chars)
- IsBuiltIn (bool) - true for Vinyl/Comic/Puzzle
- IsArchived (bool) - soft delete flag
- ArchivedAt (DateTimeOffset, nullable)
- AttributeDefinitions (JSON) - array of attribute schemas
- CreatedAt (DateTimeOffset)
- UpdatedAt (DateTimeOffset)
- ItemCount (computed) - number of items using this type

### AttributeDefinition (Value Object, stored as JSON)
- AttributeName (string 2-50 chars)
- DataType (enum: Text, Number, Date, Choice, Boolean)
- IsRequired (bool)
- ValidationRules (JSON) - min/max, choices, etc.
- HelpText (string 0-200 chars)
- DefaultValue (object, nullable)
- DisplayOrder (int)

## Success Metrics

### Business Metrics
- Custom type creation rate
- Average attributes per custom type
- Type deletion/archive rate
- Template usage distribution

### Technical Metrics
- Validation error rate
- Schema update frequency
- Average validation time
- Database query performance for type-based filters

### User Experience Metrics
- Time to create first custom type
- Completion rate for type creation wizard
- Error correction attempts per validation failure
- User satisfaction score (survey)

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Users create overly complex schemas | Medium | Medium | Limit to 30 attributes, provide templates, guide with help text |
| Schema changes break existing items | High | Low | Backwards compatibility validation, prevent destructive changes |
| Performance degrades with many custom types | Medium | Low | Index on UserId + TypeName, cache schema definitions |
| Users confused by data type concepts | Medium | Medium | Provide examples, templates, inline help, wizard UI |
| Type name conflicts across users | Low | N/A | Types are user-scoped, no cross-user visibility initially |

## Open Questions

None - all critical decisions have reasonable defaults based on industry standards and common collection management patterns.

## Acceptance Testing

### Test Case 1: Create Custom Type with Validation
**Given**: User is logged in
**When**: User creates "Coin" type with attributes: Denomination (number, required), Year (number 1800-2024, required)
**Then**: Type is saved successfully
**And**: Creating coin without Denomination shows "Denomination is required"
**And**: Creating coin with Year=1799 shows "Year must be between 1800 and 2024"

### Test Case 2: Edit Type Schema (Non-Destructive)
**Given**: "Stamp" type exists with 10 items
**When**: User adds optional "Mint Mark" attribute
**Then**: Existing 10 stamps remain valid
**And**: New stamps can include "Mint Mark"
**And**: Editing existing stamp shows "Mint Mark" field (empty, optional)

### Test Case 3: Prevent Type Deletion with Items
**Given**: "Watch" type has 25 items
**When**: User attempts to delete "Watch" type
**Then**: System shows error "Cannot delete - 25 items use this type"
**And**: Type is not deleted

### Test Case 4: Maximum Limits Enforced
**Given**: User has 50 custom types
**When**: User attempts to create 51st type
**Then**: System shows "Maximum 50 custom types reached"
**And**: Type creation is blocked

### Test Case 5: Template Application
**Given**: User selects "Coin" template
**When**: User applies template
**Then**: New type "Coin" is created with predefined attributes (Denomination, Year, Mint Mark, Country, Grade)
**And**: User can modify attribute definitions
**And**: User can rename type to "Ancient Coin"

---

**Next Steps**: Proceed with `/speckit.plan` to generate implementation plan, or use `/speckit.clarify` if requirements need refinement.

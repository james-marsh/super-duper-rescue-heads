# Implementation Plan: Custom Item Types

**Branch**: `feature/005-implementation` | **Date**: 2025-11-28 | **Spec**: [spec.md](./spec.md)

## Summary

Allows users to define custom item types beyond the built-in types (Vinyl, Comic, Puzzle). Users can create custom schemas defining required/optional attributes with data types (Text, Number, Date, Choice, Boolean) and validation rules. Extends Feature 002's JSON attribute system with type templates, validation engine, and schema evolution support.

## Technical Context

**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, EF Core 9.0, FluentValidation, Blazor Server
**Storage**: Azure SQL Database with JSON columns
**Testing**: TUnit (unit), xUnit (integration), bUnit (Blazor components), Playwright (E2E)
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 001-002
**Performance Goals**: Schema validation <50ms, type CRUD <200ms, item validation <300ms
**Constraints**: Max 50 custom types per user, max 30 attributes per type
**Scale/Scope**: Typical 3-10 custom types per user, 10-100 items per type

## Constitution Check

| Gate | Status | Rationale |
|------|--------|-----------|
| Essential | ✅ Pass | Users need custom types to catalog diverse collectibles - built-in types only cover 3 categories |
| Simple | ✅ Pass | JSON schema storage avoids complex migrations, FluentValidation for rules, minimal UI (form builder) |
| Technology | ✅ Pass | C# 14, .NET 10, EF Core 9.0 JSON columns, FluentValidation, Blazor Server |
| Cost-Effective | ✅ Pass | No additional services - uses existing Azure SQL JSON support, no external schema registry |
| Pragmatic | ✅ Pass | JSON storage allows schema flexibility without EF migrations, templates reduce configuration |
| Cloud-Native | ✅ Pass | Azure SQL JSON columns, stateless validation service, scales with Container Apps |
| Incremental | ✅ Pass | 5 user stories deliverable independently: US1 (CRUD), US2 (validation), US3 (templates), US4 (item integration), US5 (schema evolution) |

**ALL GATES PASSED** ✅

## Project Structure

```
SuperDuperRescueHeads.Domain/
  ItemTypes/
    ItemTypeSchema.cs               # Aggregate root
    AttributeDefinition.cs          # Value object
    ValidationRule.cs               # Value object
    IItemTypeRepository.cs          # Repository interface
    ItemTypeSchemaEvents.cs         # Domain events

SuperDuperRescueHeads.Infrastructure/
  Data/
    Configurations/
      ItemTypeSchemaConfiguration.cs  # EF Core config
    Repositories/
      ItemTypeRepository.cs           # Repository implementation
  Validation/
    SchemaValidator.cs                # Attribute validation engine
    ValidationRuleEngine.cs           # Rule evaluation

SuperDuperRescueHeads.Api/
  Endpoints/
    ItemTypeEndpoints.cs              # Minimal API endpoints
  Models/
    ItemTypeDto.cs                    # API DTOs
    AttributeDefinitionDto.cs

SuperDuperRescueHeads.Web/
  Components/
    Pages/
      ItemTypes/
        Index.razor                   # List custom types
        Create.razor                  # Create/edit type
        AttributeBuilder.razor        # Schema builder UI
    Shared/
      TypeSelector.razor              # Dropdown for item creation

SuperDuperRescueHeads.Tests.Unit/
  ItemTypes/
    ItemTypeSchemaTests.cs            # Domain logic tests
    ValidationRuleEngineTests.cs      # Validation tests

SuperDuperRescueHeads.Tests.Integration/
  ItemTypes/
    ItemTypeRepositoryTests.cs        # Database tests
    SchemaValidatorTests.cs           # End-to-end validation
```

## Complexity Tracking

**Justifications**:
1. JSON schema storage: Required for flexible attribute definitions without constant migrations
2. FluentValidation: Industry-standard validation library, simpler than custom validator
3. Schema builder UI: Essential for user experience - forms alone too cumbersome for 30 attributes

**No violations** - all complexity justified by user requirements

## Implementation Phases

### Phase 1: Domain Model (US1 - CRUD)
**Goal**: Create ItemTypeSchema aggregate with basic CRUD

**Tasks**:
1. Create `ItemTypeSchema.cs` aggregate root
   - Properties: ItemTypeSchemaId, UserId, TypeName, Description, IsBuiltIn, IsArchived
   - Validation: TypeName 2-50 chars, unique per user
   - Methods: `Create()`, `Update()`, `Archive()`, `AddAttribute()`, `RemoveAttribute()`

2. Create `AttributeDefinition.cs` value object
   - Properties: AttributeName, DataType (enum), IsRequired, ValidationRules (JSON), HelpText
   - Methods: `ValidateValue(object value)` - returns validation result

3. Create `ValidationRule.cs` value object hierarchy
   - TextValidationRule: MinLength, MaxLength, DefaultValue
   - NumberValidationRule: MinValue, MaxValue, DecimalPlaces, DefaultValue
   - DateValidationRule: MinDate, MaxDate, DefaultToToday
   - ChoiceValidationRule: Choices (list), AllowMultiple
   - BooleanValidationRule: DefaultValue

4. Create `IItemTypeRepository.cs` interface
   - `GetByIdAsync(Guid id)`, `GetByUserIdAsync(Guid userId)`, `GetByNameAsync(Guid userId, string name)`
   - `AddAsync(ItemTypeSchema)`, `UpdateAsync(ItemTypeSchema)`, `DeleteAsync(Guid id)`
   - `GetItemCountAsync(Guid itemTypeId)` - for deletion validation

5. Write unit tests for `ItemTypeSchema` domain logic
   - Test name validation, uniqueness constraints
   - Test Archive() when items exist (should fail)
   - Test AddAttribute() with max 30 limit

**Acceptance**: Domain model enforces business rules, 100% unit test coverage

### Phase 2: Infrastructure & Repository (US1 continued)
**Goal**: Persist ItemTypeSchema to Azure SQL with JSON columns

**Tasks**:
1. Create `ItemTypeSchemaConfiguration.cs` EF Core configuration
   - Configure JSON column for `AttributeDefinitions`
   - Index on `(UserId, TypeName)` for uniqueness and lookup
   - Index on `(UserId, IsArchived)` for filtering

2. Create migration `AddItemTypeSchema`
   - Table: ItemTypeSchemas
   - Columns: ItemTypeSchemaId (PK), UserId (FK), TypeName, Description, IsBuiltIn, IsArchived, AttributeDefinitions (NVARCHAR(MAX) JSON), CreatedAt, UpdatedAt
   - Unique constraint: `IX_ItemTypeSchemas_UserId_TypeName` where `IsArchived = 0`

3. Implement `ItemTypeRepository.cs`
   - Use `AsNoTracking()` for read-only queries
   - Include `GetItemCount()` join with Items table
   - Validate unique constraint before save

4. Seed built-in types (Vinyl, Comic, Puzzle) in migration
   - Mark `IsBuiltIn = true`, `UserId = Guid.Empty` (system types)
   - Built-in types visible to all users but not editable

5. Write integration tests for repository
   - Test CRUD operations
   - Test unique constraint enforcement
   - Test JSON serialization/deserialization

**Acceptance**: Repository persists types correctly, JSON columns serialize properly

### Phase 3: Validation Engine (US2 - Type-Based Item Validation)
**Goal**: Validate item attribute values against type schemas

**Tasks**:
1. Create `SchemaValidator.cs` service
   - `ValidateItem(ItemTypeSchema type, Dictionary<string, object> attributes)` → `ValidationResult`
   - Check all required attributes present
   - Validate each attribute value against its `ValidationRule`

2. Create `ValidationRuleEngine.cs`
   - `Validate(AttributeDefinition attribute, object value)` → `ValidationError?`
   - Dispatch to type-specific validators (Text, Number, Date, Choice, Boolean)
   - Return detailed error messages with field name and constraint

3. Implement type-specific validators
   - `TextValidator`: Check min/max length
   - `NumberValidator`: Check min/max value, decimal places
   - `DateValidator`: Check min/max date range
   - `ChoiceValidator`: Check value in allowed choices, handle multi-select
   - `BooleanValidator`: Check true/false/null (if not required)

4. Integrate validation into `ItemRepository.SaveAsync()`
   - Load ItemTypeSchema if item has custom type
   - Call `SchemaValidator.ValidateItem()` before save
   - Throw `ValidationException` with all errors if validation fails

5. Write unit tests for each validator
   - Test boundary conditions (min/max)
   - Test invalid data types (string as number)
   - Test required vs optional attributes
   - Test multi-select choices

**Acceptance**: All validation rules enforced, error messages clear and specific

### Phase 4: API Endpoints (US1 + US2)
**Goal**: Expose ItemTypeSchema CRUD via REST API

**Tasks**:
1. Create `ItemTypeEndpoints.cs` minimal API
   - `GET /api/v1/item-types` - List all types for user (exclude archived unless `?includeArchived=true`)
   - `GET /api/v1/item-types/{id}` - Get type by ID with attribute definitions
   - `POST /api/v1/item-types` - Create new type (validate name uniqueness, max 50 types)
   - `PUT /api/v1/item-types/{id}` - Update type (validate schema changes)
   - `DELETE /api/v1/item-types/{id}` - Archive type (check item count first)
   - `POST /api/v1/item-types/{id}/validate` - Validate item attributes against schema

2. Create DTOs
   - `ItemTypeDto`: Map from ItemTypeSchema aggregate
   - `AttributeDefinitionDto`: Map from AttributeDefinition value object
   - `CreateItemTypeRequest`, `UpdateItemTypeRequest`
   - `ValidateItemRequest`, `ValidationResultResponse`

3. Register services in `Program.cs`
   - `AddScoped<IItemTypeRepository, ItemTypeRepository>()`
   - `AddScoped<SchemaValidator>()`

4. Write contract tests (OpenAPI validation)
   - Test all endpoints return correct status codes
   - Test response schemas match OpenAPI spec
   - Test authentication required for all endpoints

**Acceptance**: API endpoints functional, DTOs map correctly, validation integrated

### Phase 5: Type Templates (US3 - Templates & Reusability)
**Goal**: Provide predefined templates for common collectible types

**Tasks**:
1. Create `TypeTemplateService.cs`
   - `GetTemplates()` → List of 5 template definitions
   - Templates: Coin, Stamp, Watch, Camera, Book (as defined in spec)

2. Add `POST /api/v1/item-types/templates/{templateName}` endpoint
   - Apply template to create new custom type for user
   - Set default type name to template name + " Copy" if name exists

3. Add `POST /api/v1/item-types/{id}/duplicate` endpoint
   - Clone existing type with " Copy" suffix
   - Preserve all attribute definitions

4. Update `ItemTypeEndpoints.GET /templates` to list available templates
   - Return template names, descriptions, preview of attributes

5. Write unit tests for template application
   - Test each template creates correct attribute structure
   - Test duplicate naming conflicts resolved

**Acceptance**: All 5 templates available, duplication works, naming conflicts resolved

### Phase 6: Blazor UI (US1-US3)
**Goal**: Build user-friendly type management interface

**Tasks**:
1. Create `ItemTypes/Index.razor` page
   - List all custom types in card/table view
   - Show usage count (# of items using each type)
   - Filter: Show Archived toggle
   - Actions: Create New, Edit, Duplicate, Archive/Delete

2. Create `ItemTypes/Create.razor` page (also used for Edit)
   - Form: Type Name, Description
   - AttributeBuilder component for schema definition
   - Template selector (dropdown with 5 templates + blank)
   - Save/Cancel buttons

3. Create `AttributeBuilder.razor` component
   - List of attributes (drag-and-drop to reorder)
   - Add Attribute button (shows modal)
   - Each attribute shows: Name, Type, Required, Validation Rules, Help Text
   - Edit/Delete buttons per attribute
   - Modal for add/edit with type-specific validation rule inputs:
     - Text: Min/Max Length, Default Value
     - Number: Min/Max Value, Decimal Places, Default Value
     - Date: Min/Max Date, Default to Today checkbox
     - Choice: List of choices (add/remove), Allow Multiple checkbox
     - Boolean: Default Value (True/False/Null radio)

4. Create `TypeSelector.razor` component (for Item creation)
   - Dropdown showing built-in + custom types
   - On selection, loads attribute form fields dynamically
   - Renders appropriate input for each data type (text box, number, date picker, dropdown, checkbox)
   - Shows help text below each field
   - Validates required fields on submit

5. Update `Items/Create.razor` to use `TypeSelector`
   - Replace static Vinyl/Comic/Puzzle dropdown with dynamic TypeSelector
   - Render custom attributes from selected type
   - Submit attributes as JSON to Item API

6. Write bUnit tests for Blazor components
   - Test AttributeBuilder adds/removes attributes
   - Test TypeSelector loads correct fields for type
   - Test validation errors display correctly

**Acceptance**: UI functional, responsive, validation errors clear, drag-and-drop works

### Phase 7: Schema Evolution (US2 - Backwards Compatibility)
**Goal**: Support non-destructive schema changes

**Tasks**:
1. Update `ItemTypeSchema.UpdateSchema()` method
   - Allow adding optional attributes (safe)
   - Allow removing attributes (mark as deprecated in JSON, don't delete)
   - Prevent changing required → optional for existing attributes (breaking change)
   - Prevent changing data type (breaking change)
   - Log warnings for deprecated attributes

2. Create `SchemaMigrationValidator.cs`
   - `ValidateSchemaChange(ItemTypeSchema oldSchema, ItemTypeSchema newSchema)` → `MigrationResult`
   - Return list of safe changes, breaking changes, and warnings
   - Block breaking changes if items exist

3. Update `PUT /api/v1/item-types/{id}` endpoint
   - Call `SchemaMigrationValidator` before save
   - Return 400 Bad Request with breaking change details if validation fails
   - Allow safe changes with item count > 0

4. Update `SchemaValidator` to handle deprecated attributes
   - Ignore deprecated attributes when validating items
   - Don't require deprecated attributes even if marked required

5. Write integration tests for schema evolution
   - Test adding optional attribute with existing items
   - Test removing attribute (deprecation)
   - Test blocked breaking changes (required → optional, data type change)

**Acceptance**: Schema changes preserve existing item validity, breaking changes prevented

### Phase 8: Testing & Polish
**Goal**: Comprehensive test coverage and production readiness

**Tasks**:
1. Write E2E tests (Playwright)
   - Test complete flow: Create type → Add attributes → Create item with type → Validate item
   - Test template application
   - Test duplicate type creation
   - Test deletion prevention (type with items)

2. Performance testing
   - Benchmark schema validation with 30 attributes (<50ms)
   - Benchmark item creation with validation (<300ms)
   - Load test with 50 custom types, 10,000 items

3. Add Application Insights telemetry
   - Track custom type creation rate
   - Track template usage distribution
   - Track validation error rates by attribute type
   - Monitor schema update frequency

4. Update API documentation (OpenAPI/Swagger)
   - Document all endpoints with examples
   - Document validation error responses
   - Document schema evolution rules

5. Migration guide
   - Script to migrate existing items to use custom types (if applicable)
   - Guide for users to transition from JSON attributes to typed schemas

**Acceptance**: All tests pass, performance targets met, documentation complete

## User Stories Breakdown

### US1: Manage Custom Types (P1)
**Goal**: CRUD operations for ItemTypeSchema

**Tasks**: Phase 1-2, Phase 4 (API), Phase 6 (UI Index/Create pages)
**Estimate**: 40 tasks
**Dependencies**: None (foundation)

### US2: Validate Items Against Schemas (P1)
**Goal**: Enforce attribute validation rules

**Tasks**: Phase 3 (validation engine), Phase 4 (validate endpoint), Phase 7 (schema evolution)
**Estimate**: 30 tasks
**Dependencies**: US1 (need types to validate against)

### US3: Type Templates (P2)
**Goal**: Predefined templates for common types

**Tasks**: Phase 5 (template service, API endpoints)
**Estimate**: 15 tasks
**Dependencies**: US1 (templates create types)

### US4: Dynamic Item Forms (P1)
**Goal**: Render item creation forms based on type schema

**Tasks**: Phase 6 (TypeSelector component, Items/Create update)
**Estimate**: 20 tasks
**Dependencies**: US1 (need types), US2 (need validation)

### US5: Schema Evolution (P3)
**Goal**: Support non-destructive schema changes

**Tasks**: Phase 7 (migration validator, backwards compatibility)
**Estimate**: 15 tasks
**Dependencies**: US1 (need existing types), US2 (need validation)

**Total**: ~120 tasks

## Risk Mitigation

### Risk: JSON serialization performance with 30 attributes
**Mitigation**: Benchmark with 30 attributes, use `System.Text.Json` (faster than Newtonsoft), cache deserialized schemas in-memory

### Risk: Complex UI for attribute builder (30 attributes)
**Mitigation**: Drag-and-drop reordering, collapsible attribute cards, search/filter attributes, templates reduce manual work

### Risk: Schema changes break existing items
**Mitigation**: `SchemaMigrationValidator` prevents breaking changes, unit tests for backwards compatibility, deprecation instead of deletion

### Risk: Users create duplicate types with slightly different names
**Mitigation**: Case-insensitive name matching, search/filter on type list, suggest duplicates on create

### Risk: Validation errors too technical for users
**Mitigation**: User-friendly error messages ("Must be at least 1 character" not "MinLength constraint failed"), help text per attribute

## Success Metrics

**Implementation**:
- [ ] All 5 user stories implemented
- [ ] 100% unit test coverage for domain logic
- [ ] 80% integration test coverage
- [ ] Performance targets met (<50ms validation, <200ms CRUD, <300ms item validation)
- [ ] Zero breaking changes deployed to production

**Adoption** (post-launch):
- 60% of users create at least 1 custom type within first month
- Average 3-5 custom types per active user
- 80% template usage (vs manual creation)
- 95% validation errors resolved on first attempt

## Dependencies

**Internal**:
- Feature 001 (Collection Management): UserId context
- Feature 002 (Item Management): Item.Attributes JSON field, ItemType integration

**External**:
- EF Core 9.0: JSON column support
- FluentValidation: Validation rule engine
- Blazor Server: UI components

## Next Steps

1. Review and approve this implementation plan
2. Run `/speckit.tasks` to generate detailed task breakdown (120 tasks)
3. Begin Phase 1: Domain Model implementation
4. Proceed incrementally through phases

**Estimated Timeline**: 3-4 weeks for full implementation (all 5 user stories)
**MVP** (US1 + US2): 2 weeks (basic CRUD + validation)

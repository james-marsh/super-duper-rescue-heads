# Implementation Tasks: Custom Item Types

**Feature**: 005-custom-item-types
**Generated**: 2025-11-28
**Total Tasks**: 125 (estimated)
**MVP Scope**: Phase 1-3 (US1 - Manage Custom Types) = 48 tasks

## Overview

This feature enables users to define custom item types with flexible attribute schemas beyond built-in types (Vinyl, Comic, Puzzle). Implementation follows TDD approach with domain model, validation engine, API endpoints, and Blazor UI.

**Technology Stack**: C# 14, .NET 10, EF Core 9.0 JSON columns, FluentValidation, Blazor Server
**Testing**: TUnit (unit), xUnit (integration), bUnit (Blazor), Playwright (E2E)

---

## Phase 1: Setup & Project Structure (8 tasks)

**Goal**: Initialize project structure and install dependencies

- [ ] T001 Create SuperDuperRescueHeads.Domain/ItemTypes directory
- [ ] T002 Create SuperDuperRescueHeads.Infrastructure/Validation directory
- [ ] T003 Create SuperDuperRescueHeads.Api/Models directory (if not exists)
- [ ] T004 Create SuperDuperRescueHeads.Tests.Unit/ItemTypes directory
- [ ] T005 Create SuperDuperRescueHeads.Tests.Integration/ItemTypes directory
- [ ] T006 Install FluentValidation NuGet package in SuperDuperRescueHeads.Infrastructure
- [ ] T007 Install FluentValidation.AspNetCore NuGet package in SuperDuperRescueHeads.Api
- [ ] T008 Create SuperDuperRescueHeads.Web/Components/Pages/ItemTypes directory

---

## Phase 2: Foundational Domain Model (4 tasks)

**Goal**: Create core domain entities and value objects (blocking for all user stories)

- [ ] T009 Create DataType enum in SuperDuperRescueHeads.Domain/ItemTypes/DataType.cs
- [ ] T010 Create IValidationRule interface in SuperDuperRescueHeads.Domain/ItemTypes/IValidationRule.cs
- [ ] T011 Create ValidationResult value object in SuperDuperRescueHeads.Domain/ItemTypes/ValidationResult.cs
- [ ] T012 Create MigrationResult value object in SuperDuperRescueHeads.Domain/ItemTypes/MigrationResult.cs

---

## Phase 3: User Story 1 - Manage Custom Types (P1) (40 tasks)

**Goal**: Complete CRUD operations for custom item types

**Independent Test Criteria**:
- ✅ User can create custom type with unique name (2-50 chars)
- ✅ User can add/edit/remove attributes (max 30 per type)
- ✅ User cannot create >50 custom types
- ✅ Duplicate type names rejected
- ✅ User can archive type (if no items reference it)
- ✅ Built-in types (Vinyl, Comic, Puzzle) cannot be deleted

### Domain Layer - Aggregate & Value Objects

- [ ] T013 [P] [US1] Create ItemTypeSchema aggregate root in SuperDuperRescueHeads.Domain/ItemTypes/ItemTypeSchema.cs
- [ ] T014 [P] [US1] Create AttributeDefinition value object in SuperDuperRescueHeads.Domain/ItemTypes/AttributeDefinition.cs
- [ ] T015 [P] [US1] Create TextValidationRule value object in SuperDuperRescueHeads.Domain/ItemTypes/TextValidationRule.cs
- [ ] T016 [P] [US1] Create NumberValidationRule value object in SuperDuperRescueHeads.Domain/ItemTypes/NumberValidationRule.cs
- [ ] T017 [P] [US1] Create DateValidationRule value object in SuperDuperRescueHeads.Domain/ItemTypes/DateValidationRule.cs
- [ ] T018 [P] [US1] Create ChoiceValidationRule value object in SuperDuperRescueHeads.Domain/ItemTypes/ChoiceValidationRule.cs
- [ ] T019 [P] [US1] Create BooleanValidationRule value object in SuperDuperRescueHeads.Domain/ItemTypes/BooleanValidationRule.cs

### Domain Layer - Repository Interface

- [ ] T020 [US1] Create IItemTypeRepository interface in SuperDuperRescueHeads.Domain/ItemTypes/IItemTypeRepository.cs

### Domain Layer - Events

- [ ] T021 [P] [US1] Create ItemTypeSchemaEvents.cs with domain events in SuperDuperRescueHeads.Domain/ItemTypes/ItemTypeSchemaEvents.cs

### Unit Tests - Domain Logic

- [ ] T022 [P] [US1] Test ItemTypeSchema.Create() validates name (2-50 chars) in SuperDuperRescueHeads.Tests.Unit/ItemTypes/ItemTypeSchemaTests.cs
- [ ] T023 [P] [US1] Test ItemTypeSchema.AddAttribute() enforces max 30 attributes in SuperDuperRescueHeads.Tests.Unit/ItemTypes/ItemTypeSchemaTests.cs
- [ ] T024 [P] [US1] Test ItemTypeSchema.Archive() validates no items exist in SuperDuperRescueHeads.Tests.Unit/ItemTypes/ItemTypeSchemaTests.cs
- [ ] T025 [P] [US1] Test AttributeDefinition validates attribute name uniqueness in SuperDuperRescueHeads.Tests.Unit/ItemTypes/AttributeDefinitionTests.cs
- [ ] T026 [P] [US1] Test TextValidationRule validates min/max length in SuperDuperRescueHeads.Tests.Unit/ItemTypes/TextValidationRuleTests.cs
- [ ] T027 [P] [US1] Test NumberValidationRule validates min/max value in SuperDuperRescueHeads.Tests.Unit/ItemTypes/NumberValidationRuleTests.cs
- [ ] T028 [P] [US1] Test DateValidationRule validates date range in SuperDuperRescueHeads.Tests.Unit/ItemTypes/DateValidationRuleTests.cs
- [ ] T029 [P] [US1] Test ChoiceValidationRule validates allowed choices in SuperDuperRescueHeads.Tests.Unit/ItemTypes/ChoiceValidationRuleTests.cs

### Infrastructure - EF Core Configuration

- [ ] T030 [US1] Create ItemTypeSchemaConfiguration.cs with JSON column mapping in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ItemTypeSchemaConfiguration.cs
- [ ] T031 [US1] Add ItemTypeSchemas DbSet to ApplicationDbContext in SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T032 [US1] Create migration AddItemTypeSchema in SuperDuperRescueHeads.Infrastructure/Migrations/

### Infrastructure - Repository Implementation

- [ ] T033 [US1] Implement ItemTypeRepository in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemTypeRepository.cs
- [ ] T034 [US1] Implement GetItemCountAsync() with join to Items table in ItemTypeRepository
- [ ] T035 [US1] Seed built-in types (Vinyl, Comic, Puzzle) in migration

### Integration Tests - Repository

- [ ] T036 [P] [US1] Test ItemTypeRepository CRUD operations in SuperDuperRescueHeads.Tests.Integration/ItemTypes/ItemTypeRepositoryTests.cs
- [ ] T037 [P] [US1] Test unique constraint (UserId, TypeName) enforcement in ItemTypeRepositoryTests
- [ ] T038 [P] [US1] Test JSON serialization/deserialization in ItemTypeRepositoryTests
- [ ] T039 [P] [US1] Test GetItemCountAsync() returns correct count in ItemTypeRepositoryTests

### API Layer - DTOs

- [ ] T040 [P] [US1] Create ItemTypeDto in SuperDuperRescueHeads.Api/Models/ItemTypeDto.cs
- [ ] T041 [P] [US1] Create AttributeDefinitionDto in SuperDuperRescueHeads.Api/Models/AttributeDefinitionDto.cs
- [ ] T042 [P] [US1] Create CreateItemTypeRequest in SuperDuperRescueHeads.Api/Models/CreateItemTypeRequest.cs
- [ ] T043 [P] [US1] Create UpdateItemTypeRequest in SuperDuperRescueHeads.Api/Models/UpdateItemTypeRequest.cs

### API Layer - Endpoints

- [ ] T044 [US1] Create ItemTypeEndpoints.cs with MapGroup in SuperDuperRescueHeads.Api/Endpoints/ItemTypeEndpoints.cs
- [ ] T045 [US1] Implement GET /api/v1/item-types endpoint in ItemTypeEndpoints
- [ ] T046 [US1] Implement GET /api/v1/item-types/{id} endpoint in ItemTypeEndpoints
- [ ] T047 [US1] Implement POST /api/v1/item-types endpoint (validate max 50 types) in ItemTypeEndpoints
- [ ] T048 [US1] Implement PUT /api/v1/item-types/{id} endpoint in ItemTypeEndpoints
- [ ] T049 [US1] Implement DELETE /api/v1/item-types/{id} endpoint (check item count) in ItemTypeEndpoints
- [ ] T050 [US1] Register ItemTypeRepository and endpoints in Program.cs

### Contract Tests - API

- [ ] T051 [P] [US1] Test GET /api/v1/item-types returns 200 with types in SuperDuperRescueHeads.Tests.Contract/ItemTypeEndpointsTests.cs
- [ ] T052 [P] [US1] Test POST /api/v1/item-types validates max 50 types in ItemTypeEndpointsTests
- [ ] T053 [P] [US1] Test DELETE /api/v1/item-types prevents deletion with items in ItemTypeEndpointsTests

---

## Phase 4: User Story 2 - Validate Items Against Schemas (P1) (27 tasks)

**Goal**: Enforce attribute validation rules when creating/updating items

**Independent Test Criteria**:
- ✅ Required attributes must be provided
- ✅ Attribute values validated against data type (text, number, date, choice, boolean)
- ✅ Min/max constraints enforced (text length, number range, date range)
- ✅ Choice values must be in allowed list
- ✅ Validation errors show field name and constraint
- ✅ Schema changes preserve existing item validity (backwards compatible)

**Dependencies**: Requires US1 (need types to validate against)

### Infrastructure - Validation Engine

- [ ] T054 [P] [US2] Create SchemaValidator service in SuperDuperRescueHeads.Infrastructure/Validation/SchemaValidator.cs
- [ ] T055 [P] [US2] Create ValidationRuleEngine in SuperDuperRescueHeads.Infrastructure/Validation/ValidationRuleEngine.cs
- [ ] T056 [P] [US2] Implement TextValidator in SuperDuperRescueHeads.Infrastructure/Validation/TextValidator.cs
- [ ] T057 [P] [US2] Implement NumberValidator in SuperDuperRescueHeads.Infrastructure/Validation/NumberValidator.cs
- [ ] T058 [P] [US2] Implement DateValidator in SuperDuperRescueHeads.Infrastructure/Validation/DateValidator.cs
- [ ] T059 [P] [US2] Implement ChoiceValidator in SuperDuperRescueHeads.Infrastructure/Validation/ChoiceValidator.cs
- [ ] T060 [P] [US2] Implement BooleanValidator in SuperDuperRescueHeads.Infrastructure/Validation/BooleanValidator.cs

### Integration with Item Repository

- [ ] T061 [US2] Integrate SchemaValidator into ItemRepository.SaveAsync() in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T062 [US2] Load ItemTypeSchema if item has custom type in ItemRepository
- [ ] T063 [US2] Throw ValidationException with all errors if validation fails in ItemRepository

### Unit Tests - Validators

- [ ] T064 [P] [US2] Test TextValidator enforces min/max length in SuperDuperRescueHeads.Tests.Unit/ItemTypes/TextValidatorTests.cs
- [ ] T065 [P] [US2] Test NumberValidator enforces min/max value and decimal places in NumberValidatorTests
- [ ] T066 [P] [US2] Test DateValidator enforces date range in DateValidatorTests
- [ ] T067 [P] [US2] Test ChoiceValidator validates allowed choices in ChoiceValidatorTests
- [ ] T068 [P] [US2] Test ChoiceValidator handles multi-select in ChoiceValidatorTests
- [ ] T069 [P] [US2] Test BooleanValidator validates true/false/null in BooleanValidatorTests
- [ ] T070 [P] [US2] Test SchemaValidator checks required attributes in SchemaValidatorTests
- [ ] T071 [P] [US2] Test SchemaValidator returns all errors (not just first) in SchemaValidatorTests

### Integration Tests - End-to-End Validation

- [ ] T072 [P] [US2] Test creating item with missing required attribute fails in SuperDuperRescueHeads.Tests.Integration/ItemTypes/SchemaValidatorTests.cs
- [ ] T073 [P] [US2] Test creating item with invalid number (outside range) fails in SchemaValidatorTests
- [ ] T074 [P] [US2] Test creating item with invalid text (exceeds max length) fails in SchemaValidatorTests
- [ ] T075 [P] [US2] Test creating item with invalid choice fails in SchemaValidatorTests
- [ ] T076 [P] [US2] Test creating item with valid attributes succeeds in SchemaValidatorTests

### API - Validation Endpoint

- [ ] T077 [US2] Implement POST /api/v1/item-types/{id}/validate endpoint in ItemTypeEndpoints
- [ ] T078 [US2] Create ValidateItemRequest and ValidationResultResponse DTOs in SuperDuperRescueHeads.Api/Models/
- [ ] T079 [US2] Register SchemaValidator in Program.cs
- [ ] T080 [P] [US2] Test POST /api/v1/item-types/{id}/validate returns validation errors in ItemTypeEndpointsTests

---

## Phase 5: User Story 3 - Type Templates (P2) (13 tasks)

**Goal**: Provide predefined templates for common collectible types

**Independent Test Criteria**:
- ✅ 5 templates available: Coin, Stamp, Watch, Camera, Book
- ✅ Template application creates new custom type with predefined attributes
- ✅ User can duplicate existing types
- ✅ Duplicate naming conflicts resolved (adds " Copy" suffix)

**Dependencies**: Requires US1 (templates create types)

### Infrastructure - Template Service

- [ ] T081 [P] [US3] Create TypeTemplateService with 5 template definitions in SuperDuperRescueHeads.Infrastructure/ItemTypes/TypeTemplateService.cs
- [ ] T082 [P] [US3] Define Coin template (Denomination, Year, Mint Mark, Country, Grade) in TypeTemplateService
- [ ] T083 [P] [US3] Define Stamp template (Country, Year, Denomination, Condition, Catalog Number) in TypeTemplateService
- [ ] T084 [P] [US3] Define Watch template (Brand, Model, Movement, Case Material, Year) in TypeTemplateService
- [ ] T085 [P] [US3] Define Camera template (Brand, Model, Format, Lens Mount, Year) in TypeTemplateService
- [ ] T086 [P] [US3] Define Book template (Title, Author, Publisher, ISBN, Edition, Year) in TypeTemplateService

### API - Template Endpoints

- [ ] T087 [US3] Implement GET /api/v1/item-types/templates endpoint in ItemTypeEndpoints
- [ ] T088 [US3] Implement POST /api/v1/item-types/templates/{templateName} endpoint in ItemTypeEndpoints
- [ ] T089 [US3] Implement POST /api/v1/item-types/{id}/duplicate endpoint in ItemTypeEndpoints
- [ ] T090 [US3] Register TypeTemplateService in Program.cs

### Unit Tests - Templates

- [ ] T091 [P] [US3] Test each template creates correct attribute structure in SuperDuperRescueHeads.Tests.Unit/ItemTypes/TypeTemplateServiceTests.cs
- [ ] T092 [P] [US3] Test duplicate naming conflicts resolved (" Copy" suffix) in TypeTemplateServiceTests
- [ ] T093 [P] [US3] Test POST /api/v1/item-types/templates/{templateName} in ItemTypeEndpointsTests

---

## Phase 6: User Story 4 - Dynamic Item Forms (P1) (18 tasks)

**Goal**: Render item creation forms dynamically based on type schema

**Independent Test Criteria**:
- ✅ TypeSelector shows built-in + custom types
- ✅ Selecting type loads correct attribute fields dynamically
- ✅ Each data type renders appropriate input (text, number, date picker, dropdown, checkbox)
- ✅ Help text displays below fields
- ✅ Required fields validated on submit
- ✅ Validation errors display inline

**Dependencies**: Requires US1 (need types), US2 (need validation)

### Blazor Components - Type Management UI

- [ ] T094 [P] [US4] Create ItemTypes/Index.razor page in SuperDuperRescueHeads.Web/Components/Pages/ItemTypes/Index.razor
- [ ] T095 [P] [US4] Create ItemTypes/Create.razor page in SuperDuperRescueHeads.Web/Components/Pages/ItemTypes/Create.razor
- [ ] T096 [P] [US4] Create AttributeBuilder.razor component in SuperDuperRescueHeads.Web/Components/Pages/ItemTypes/AttributeBuilder.razor
- [ ] T097 [US4] Implement Index.razor: List types with usage count, filter archived
- [ ] T098 [US4] Implement Create.razor: Type name/description form, template selector
- [ ] T099 [US4] Implement AttributeBuilder: Add/edit/delete attributes, drag-and-drop reordering

### Blazor Components - Dynamic Item Forms

- [ ] T100 [P] [US4] Create TypeSelector.razor component in SuperDuperRescueHeads.Web/Components/Shared/TypeSelector.razor
- [ ] T101 [US4] Implement TypeSelector dropdown with built-in + custom types
- [ ] T102 [US4] Load attribute fields dynamically on type selection in TypeSelector
- [ ] T103 [US4] Render text input for Text attributes in TypeSelector
- [ ] T104 [US4] Render number input for Number attributes in TypeSelector
- [ ] T105 [US4] Render date picker for Date attributes in TypeSelector
- [ ] T106 [US4] Render dropdown for Choice attributes in TypeSelector
- [ ] T107 [US4] Render checkbox for Boolean attributes in TypeSelector
- [ ] T108 [US4] Display help text below each field in TypeSelector
- [ ] T109 [US4] Validate required fields on submit in TypeSelector
- [ ] T110 [US4] Update Items/Create.razor to use TypeSelector in SuperDuperRescueHeads.Web/Components/Pages/Items/Create.razor

### bUnit Tests - Blazor Components

- [ ] T111 [P] [US4] Test AttributeBuilder adds/removes attributes in SuperDuperRescueHeads.Tests.UI/ItemTypes/AttributeBuilderTests.cs

---

## Phase 7: User Story 5 - Schema Evolution (P3) (12 tasks)

**Goal**: Support non-destructive schema changes

**Independent Test Criteria**:
- ✅ Adding optional attributes allowed with existing items
- ✅ Removing attributes marks as deprecated (not deleted)
- ✅ Changing required → optional blocked if items exist
- ✅ Changing data type blocked if items exist
- ✅ Breaking changes return 400 Bad Request with details
- ✅ Safe changes allowed with item count > 0

**Dependencies**: Requires US1 (need types), US2 (need validation)

### Infrastructure - Schema Migration Validator

- [ ] T112 [P] [US5] Create SchemaMigrationValidator in SuperDuperRescueHeads.Infrastructure/Validation/SchemaMigrationValidator.cs
- [ ] T113 [P] [US5] Implement ValidateSchemaChange() detects breaking changes in SchemaMigrationValidator
- [ ] T114 [P] [US5] Detect removed required attributes in SchemaMigrationValidator
- [ ] T115 [P] [US5] Detect data type changes in SchemaMigrationValidator
- [ ] T116 [US5] Update ItemTypeSchema.UpdateSchema() with migration validation
- [ ] T117 [US5] Update PUT /api/v1/item-types/{id} to call SchemaMigrationValidator
- [ ] T118 [US5] Update SchemaValidator to handle deprecated attributes

### Integration Tests - Schema Evolution

- [ ] T119 [P] [US5] Test adding optional attribute with existing items in SuperDuperRescueHeads.Tests.Integration/ItemTypes/SchemaMigrationValidatorTests.cs
- [ ] T120 [P] [US5] Test removing attribute (deprecation) in SchemaMigrationValidatorTests
- [ ] T121 [P] [US5] Test blocked breaking change (required → optional) in SchemaMigrationValidatorTests
- [ ] T122 [P] [US5] Test blocked breaking change (data type change) in SchemaMigrationValidatorTests
- [ ] T123 [P] [US5] Test existing items remain valid after safe schema change in SchemaMigrationValidatorTests

---

## Phase 8: Polish & Cross-Cutting Concerns (2 tasks)

**Goal**: Production readiness, performance optimization, documentation

- [ ] T124 Add Application Insights telemetry for custom type creation rate, template usage, validation errors
- [ ] T125 Update OpenAPI/Swagger documentation for all ItemType endpoints

---

## Dependencies

### User Story Completion Order

```
Phase 1 (Setup) → Phase 2 (Foundational)
                   ↓
            ┌──────┴──────┬───────────────┐
            ↓             ↓               ↓
    US1 (Manage)    US3 (Templates)   US4 (Forms)
    [Phase 3]       [Phase 5]         [Phase 6]
            ↓             ↓               ↓
    US2 (Validation) ────┘               │
    [Phase 4]                            │
            ↓                             ↓
    US5 (Evolution)              (depends on US1+US2)
    [Phase 7]
            ↓
    Phase 8 (Polish)
```

**Critical Path**: Phase 1 → Phase 2 → Phase 3 (US1) → Phase 4 (US2) → Phase 7 (US5) → Phase 8
**Parallel Opportunities**:
- US3 (Templates) can start after US1 completes
- US4 (Forms) can start after US1 completes
- Within each phase, tasks marked [P] can run in parallel

---

## Parallel Execution Examples

### Phase 3 (US1) - 19 parallel tasks:
- All domain model tasks (T013-T019) can run in parallel
- All unit test tasks (T022-T029) can run in parallel (after domain models complete)
- All DTO tasks (T040-T043) can run in parallel
- All contract test tasks (T051-T053) can run in parallel (after endpoints complete)

### Phase 4 (US2) - 16 parallel tasks:
- All validator implementations (T054-T060) can run in parallel
- All unit tests (T064-T071) can run in parallel (after validators complete)
- All integration tests (T072-T076) can run in parallel (after integration complete)

### Phase 5 (US3) - 6 parallel tasks:
- All template definitions (T082-T086) can run in parallel
- All unit tests (T091-T093) can run in parallel (after endpoints complete)

### Phase 6 (US4) - 4 parallel tasks:
- Blazor page creation tasks (T094-T096) can run in parallel
- TypeSelector component can be built independently (T100)

### Phase 7 (US5) - 7 parallel tasks:
- Migration validator logic (T112-T115) can run in parallel
- Integration tests (T119-T123) can run in parallel (after validator complete)

---

## Implementation Strategy

### MVP Scope (Phases 1-3): US1 - Manage Custom Types

**Deliverables** (48 tasks):
- Complete CRUD for custom item types
- JSON schema storage with EF Core 9.0
- Built-in types seeded (Vinyl, Comic, Puzzle)
- API endpoints functional
- 100% unit test coverage for domain logic
- Integration tests for repository

**Outcome**: Users can create, edit, archive custom types with attributes. Foundation for all other stories.

**Timeline**: ~1.5 weeks

### Full Feature (All Phases): US1-US5

**Deliverables** (125 tasks):
- All user stories implemented
- Validation engine enforces schemas
- 5 type templates available
- Dynamic Blazor UI for type management
- Schema evolution with backwards compatibility
- Production-ready with telemetry

**Timeline**: ~3-4 weeks

### Incremental Delivery Plan

1. **Week 1**: MVP (US1) - CRUD operations, repository, API
2. **Week 2**: US2 (Validation) + US3 (Templates) in parallel
3. **Week 3**: US4 (Forms) + US5 (Evolution) in parallel
4. **Week 4**: Polish, E2E tests, documentation, performance tuning

---

## Task Count Summary

| Phase | User Story | Tasks | Parallel | Dependencies |
|-------|------------|-------|----------|--------------|
| 1 | Setup | 8 | 8 | None |
| 2 | Foundational | 4 | 4 | Phase 1 |
| 3 | US1 (Manage Types, P1) | 40 | 19 | Phase 2 |
| 4 | US2 (Validation, P1) | 27 | 16 | Phase 3 |
| 5 | US3 (Templates, P2) | 13 | 6 | Phase 3 |
| 6 | US4 (Dynamic Forms, P1) | 18 | 4 | Phase 3, 4 |
| 7 | US5 (Evolution, P3) | 12 | 7 | Phase 3, 4 |
| 8 | Polish | 2 | 2 | All |
| **Total** | | **125** | **66** | |

**Parallelization**: 53% of tasks can run in parallel (66/125)
**MVP**: 48 tasks (Phases 1-3)
**Full Feature**: 125 tasks

---

## Notes

- Tasks follow strict checklist format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
- [P] marker indicates task can run in parallel with others
- [US#] label maps task to user story for independent testing
- File paths are explicit for immediate executability
- Each phase delivers independently testable increment
- TDD approach: Tests written before/alongside implementation
- FluentValidation used for schema and attribute validation
- EF Core 9.0 JSON columns for flexible schema storage
- Blazor Server for dynamic UI rendering

**Ready for implementation!** Start with Phase 1 (Setup) and proceed incrementally through phases. Each phase should be committed independently for clean git history.

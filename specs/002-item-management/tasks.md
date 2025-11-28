# Tasks: Basic Item Management

**Input**: Design documents from `/specs/002-item-management/`
**Prerequisites**: Feature 001 (Collection Management) must be complete
**Framework**: .NET 9.0 with .NET Aspire, Entity Framework Core 9.0, Blazor Server, MediatR

**Tests**: Following TDD approach - tests written FIRST, must FAIL before implementation

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on Feature 001 structure (from plan.md):
- **Domain**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Domain/Items/`
- **Infrastructure**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Infrastructure/`
- **API**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Api/`
- **Web**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Web/`
- **Tests**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Tests.{Type}/`

---

## Phase 1: Setup (No New Projects Needed)

**Purpose**: Extend existing Feature 001 solution with Item Management capabilities

- [ ] T001 Create Items subdirectory in SuperDuperRescueHeads.Domain/
- [ ] T002 [P] Create Repositories subdirectory in SuperDuperRescueHeads.Infrastructure/Data/
- [ ] T003 [P] Create Configurations subdirectory in SuperDuperRescueHeads.Infrastructure/Data/ (if not exists)
- [ ] T004 [P] Create ItemsEndpoints.cs placeholder in SuperDuperRescueHeads.Api/Endpoints/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Update ApplicationDbContext with Items DbSet in SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T006 [P] Create IItemRepository interface in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T007 [P] Create ItemDomainEvents.cs with ItemCreatedEvent, ItemUpdatedEvent, ItemDeletedEvent in SuperDuperRescueHeads.Domain/Items/
- [ ] T008 Update Collection entity to include Items navigation property in SuperDuperRescueHeads.Domain/Collections/Collection.cs
- [ ] T009 Register IItemRepository with DI in SuperDuperRescueHeads.Infrastructure or SuperDuperRescueHeads.Api/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Add Items to Collections (Priority: P1) 🎯 MVP

**Goal**: Users can add items to their collections with type-specific attributes (artist/album for vinyl, issue# for comics, etc.)

**Independent Test**: Create a collection, add an item via POST /collections/{id}/items, verify item appears with correct attributes

### Tests for User Story 1 (TDD - Write Tests FIRST)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] Unit test: Create Item with valid data succeeds in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T011 [P] [US1] Unit test: Create Item with empty name throws ArgumentException in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T012 [P] [US1] Unit test: Create Item with >10KB attributes throws ArgumentException in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T013 [P] [US1] Unit test: Create ItemName with valid value succeeds in SuperDuperRescueHeads.Tests.Unit/Domain/ItemNameTests.cs
- [ ] T014 [P] [US1] Unit test: Create ItemName with empty value throws ArgumentException in SuperDuperRescueHeads.Tests.Unit/Domain/ItemNameTests.cs
- [ ] T015 [P] [US1] Unit test: Create ItemName with >200 chars throws ArgumentException in SuperDuperRescueHeads.Tests.Unit/Domain/ItemNameTests.cs
- [ ] T016 [P] [US1] Unit test: ItemName trims whitespace in SuperDuperRescueHeads.Tests.Unit/Domain/ItemNameTests.cs
- [ ] T017 [P] [US1] Integration test: ItemRepository.AddAsync persists item and increments Collection.ItemCount in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T018 [P] [US1] Integration test: POST /collections/{id}/items returns 201 Created in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T019 [P] [US1] Integration test: POST /collections/{id}/items with invalid name returns 400 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T020 [P] [US1] Integration test: POST /collections/{id}/items for non-owned collection returns 403 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T021 [P] [US1] Integration test: POST /collections/{id}/items when user has 50,000 items returns 409 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs

### Implementation for User Story 1

- [ ] T022 [P] [US1] Create ItemName value object in SuperDuperRescueHeads.Domain/Items/ItemName.cs
- [ ] T023 [P] [US1] Create Item aggregate root in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T024 [US1] Implement Item.Create factory method with attribute validation in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T025 [US1] Implement ItemRepository.AddAsync with ItemCount increment in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T026 [US1] Implement ItemRepository.CountByUserIdAsync in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T027 [US1] Create ItemConfiguration with JSON column mapping in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ItemConfiguration.cs
- [ ] T028 [US1] Create EF migration 002_AddItems in SuperDuperRescueHeads.Infrastructure/Migrations/
- [ ] T029 [US1] Apply migration to database using dotnet ef database update
- [ ] T030 [P] [US1] Create CreateItemRequest DTO in SuperDuperRescueHeads.Api/Models/CreateItemRequest.cs
- [ ] T031 [P] [US1] Create ItemResponse DTO in SuperDuperRescueHeads.Api/Models/ItemResponse.cs
- [ ] T032 [US1] Create CreateItemRequestValidator with FluentValidation in SuperDuperRescueHeads.Api/Validators/CreateItemRequestValidator.cs
- [ ] T033 [US1] Implement POST /collections/{collectionId}/items endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T034 [US1] Add collection ownership verification to POST endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T035 [US1] Add user item limit check (50,000) to POST endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T036 [US1] Register ItemsEndpoints in SuperDuperRescueHeads.Api/Program.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - users can add items to collections via API

---

## Phase 4: User Story 2 - View Items in Collection (Priority: P2)

**Goal**: Users can view a paginated list of all items in their collections

**Independent Test**: Add 150 items to a collection, GET /collections/{id}/items?skip=0&take=100, verify returns 100 items and pagination.hasMore=true

### Tests for User Story 2 (TDD - Write Tests FIRST)

- [ ] T037 [P] [US2] Integration test: GET /collections/{id}/items returns paginated items in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T038 [P] [US2] Integration test: GET /collections/{id}/items with skip=100&take=50 returns correct page in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T039 [P] [US2] Integration test: GET /collections/{id}/items for non-owned collection returns 403 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T040 [P] [US2] Integration test: ItemRepository.GetByCollectionIdAsync with 10,000 items completes <200ms in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T041 [P] [US2] Integration test: Keyset pagination with GetByCollectionIdPagedAsync returns correct items in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs

### Implementation for User Story 2

- [ ] T042 [P] [US2] Implement ItemRepository.GetByCollectionIdAsync with offset pagination in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T043 [P] [US2] Implement ItemRepository.CountByCollectionIdAsync in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T044 [US2] Implement ItemRepository.GetByCollectionIdPagedAsync with keyset pagination in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T045 [US2] Create PaginationResponse DTO in SuperDuperRescueHeads.Api/Models/PaginationResponse.cs
- [ ] T046 [US2] Implement GET /collections/{collectionId}/items endpoint with pagination in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T047 [US2] Add collection ownership verification to GET list endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can add and view items

---

## Phase 5: User Story 3 - View Item Details (Priority: P3)

**Goal**: Users can view full details of a specific item including all attributes

**Independent Test**: Add an item, GET /items/{itemId}, verify returns complete item details with all attributes

### Tests for User Story 3 (TDD - Write Tests FIRST)

- [ ] T048 [P] [US3] Integration test: GET /items/{id} returns item details in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T049 [P] [US3] Integration test: GET /items/{id} for non-owned item returns 403 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T050 [P] [US3] Integration test: GET /items/{id} for non-existent item returns 404 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T051 [P] [US3] Integration test: ItemRepository.GetByIdAsync loads Collection navigation property in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs

### Implementation for User Story 3

- [ ] T052 [US3] Implement ItemRepository.GetByIdAsync with Include(Collection) in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T053 [US3] Implement Item.BelongsToUser method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T054 [US3] Implement GET /items/{itemId} endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T055 [US3] Add authorization check (item owner) to GET single endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently - users can add, list, and view item details

---

## Phase 6: User Story 4 - Edit Item Details (Priority: P4)

**Goal**: Users can update item name, notes, attributes, and acquisition date

**Independent Test**: Add an item, PUT /items/{itemId} with updated name, verify item updated successfully

### Tests for User Story 4 (TDD - Write Tests FIRST)

- [ ] T056 [P] [US4] Unit test: Item.UpdateName with valid name succeeds and publishes event in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T057 [P] [US4] Unit test: Item.UpdateAttributes with valid JSON succeeds in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T058 [P] [US4] Unit test: Item.UpdateAttributes with >10KB JSON throws exception in SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs
- [ ] T059 [P] [US4] Integration test: PUT /items/{id} updates item successfully in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T060 [P] [US4] Integration test: PUT /items/{id} for non-owned item returns 403 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T061 [P] [US4] Integration test: PUT /items/{id} with invalid data returns 400 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T062 [P] [US4] Integration test: ItemRepository.UpdateAsync persists changes in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs

### Implementation for User Story 4

- [ ] T063 [P] [US4] Implement Item.UpdateName method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T064 [P] [US4] Implement Item.UpdateNotes method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T065 [P] [US4] Implement Item.UpdateAttributes method with validation in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T066 [P] [US4] Implement Item.UpdateAcquisitionDate method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T067 [US4] Implement ItemRepository.UpdateAsync in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T068 [US4] Create UpdateItemRequest DTO in SuperDuperRescueHeads.Api/Models/UpdateItemRequest.cs
- [ ] T069 [US4] Create UpdateItemRequestValidator in SuperDuperRescueHeads.Api/Validators/UpdateItemRequestValidator.cs
- [ ] T070 [US4] Implement PUT /items/{itemId} endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T071 [US4] Add authorization check to PUT endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs

**Checkpoint**: At this point, all CRUD operations should work - users can add, list, view, and edit items

---

## Phase 7: User Story 5 - Remove Items from Collection (Priority: P5)

**Goal**: Users can delete items from their collections (hard delete)

**Independent Test**: Add an item, DELETE /items/{itemId}, verify item removed and Collection.ItemCount decremented

### Tests for User Story 5 (TDD - Write Tests FIRST)

- [ ] T072 [P] [US5] Integration test: DELETE /items/{id} returns 204 No Content in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T073 [P] [US5] Integration test: DELETE /items/{id} decrements Collection.ItemCount in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T074 [P] [US5] Integration test: DELETE /items/{id} for non-owned item returns 403 in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T075 [P] [US5] Integration test: DELETE /items/{id} publishes ItemDeletedEvent in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T076 [P] [US5] Integration test: ItemRepository.DeleteAsync removes item and decrements count in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T077 [P] [US5] Integration test: Cascade delete - deleting collection deletes all items in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs

### Implementation for User Story 5

- [ ] T078 [US5] Implement ItemRepository.DeleteAsync with ItemCount decrement in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T079 [US5] Implement DELETE /items/{itemId} endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T080 [US5] Add authorization check to DELETE endpoint in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs

**Checkpoint**: All API endpoints complete - full CRUD operations available for items

---

## Phase 8: Blazor UI for Items (Priority: P6)

**Goal**: Users can manage items through the Blazor web interface

**Independent Test**: Navigate to /collections/{id}/items, add item via form, verify appears in list

### Tests for UI (bUnit Component Tests)

- [ ] T081 [P] [UI] Component test: ItemCard displays item name and attributes in SuperDuperRescueHeads.Tests.UI/Components/ItemCardTests.cs
- [ ] T082 [P] [UI] Component test: ItemForm validates required fields in SuperDuperRescueHeads.Tests.UI/Components/ItemFormTests.cs
- [ ] T083 [P] [UI] Component test: ItemAttributeField renders correct input type in SuperDuperRescueHeads.Tests.UI/Components/ItemAttributeFieldTests.cs
- [ ] T084 [P] [UI] Page test: ItemsIndex loads and displays items in SuperDuperRescueHeads.Tests.UI/Pages/ItemsIndexTests.cs
- [ ] T085 [P] [UI] E2E test: Add item workflow completes successfully in SuperDuperRescueHeads.Tests.E2E/UserJourneys/AddItemE2ETests.cs
- [ ] T086 [P] [UI] E2E test: Edit item workflow completes successfully in SuperDuperRescueHeads.Tests.E2E/UserJourneys/EditItemE2ETests.cs

### Implementation for UI

- [ ] T087 [UI] Create ItemService API client in SuperDuperRescueHeads.Web/Services/ItemService.cs
- [ ] T088 [UI] Register ItemService in DI in SuperDuperRescueHeads.Web/Program.cs
- [ ] T089 [P] [UI] Create ItemCard component in SuperDuperRescueHeads.Web/Components/Shared/ItemCard.razor
- [ ] T090 [P] [UI] Create ItemForm component in SuperDuperRescueHeads.Web/Components/Shared/ItemForm.razor
- [ ] T091 [P] [UI] Create ItemAttributeField component in SuperDuperRescueHeads.Web/Components/Shared/ItemAttributeField.razor
- [ ] T092 [UI] Create Items/Index.razor page (list items with pagination) in SuperDuperRescueHeads.Web/Components/Pages/Items/
- [ ] T093 [UI] Create Items/Create.razor page (add item form) in SuperDuperRescueHeads.Web/Components/Pages/Items/
- [ ] T094 [UI] Create Items/Detail.razor page (view item details) in SuperDuperRescueHeads.Web/Components/Pages/Items/
- [ ] T095 [UI] Create Items/Edit.razor page (edit item form) in SuperDuperRescueHeads.Web/Components/Pages/Items/
- [ ] T096 [UI] Create Items/Delete.razor page (delete confirmation) in SuperDuperRescueHeads.Web/Components/Pages/Items/
- [ ] T097 [UI] Add navigation link to Items in CollectionDetail page in SuperDuperRescueHeads.Web/Components/Pages/Collections/Detail.razor
- [ ] T098 [UI] Style Items pages with Tailwind CSS in SuperDuperRescueHeads.Web/Components/Pages/Items/

**Checkpoint**: Complete UI for item management - users can perform all operations through web interface

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T099 [P] Create ItemPaginationBenchmarks for performance testing in SuperDuperRescueHeads.Tests.Performance/CriticalPaths/
- [ ] T100 [P] Run pagination benchmark with 10,000 items, verify P95 <200ms in SuperDuperRescueHeads.Tests.Performance/
- [ ] T101 [P] Implement ItemCountReconciliationJob for nightly count verification in SuperDuperRescueHeads.Infrastructure/Jobs/
- [ ] T102 [P] Add structured logging to all Item operations using Serilog in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T103 [P] Add OpenTelemetry tracing to ItemRepository methods in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T104 [P] Create ProblemDetailsFactory.ItemLimitExceeded for 50,000 item limit in SuperDuperRescueHeads.Api/ProblemDetails/
- [ ] T105 [P] Update API documentation with Items endpoints in SuperDuperRescueHeads.Api/
- [ ] T106 [P] Add XML comments to Item domain classes in SuperDuperRescueHeads.Domain/Items/
- [ ] T107 Validate quickstart.md implementation guide accuracy
- [ ] T108 Run all tests and verify 80%+ code coverage
- [ ] T109 Performance tuning: Add AsNoTracking() to read-only queries in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T110 Security review: Verify all endpoints check authorization

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (US1 → US2 → US3 → US4 → US5)
- **Blazor UI (Phase 8)**: Depends on US1-US5 API endpoints being complete
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - No dependencies on other stories (independently testable)
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories (independently testable)
- **User Story 4 (P4)**: Depends on US1 (needs Item aggregate) - But independently testable
- **User Story 5 (P5)**: Depends on US1 (needs Item aggregate) - But independently testable

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Value objects before aggregates
- Aggregates before repositories
- Repositories before endpoints
- DTOs and endpoints can be done in parallel
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, US1, US2, US3 can start in parallel (different files)
- US4 and US5 can run in parallel after US1 completes
- All tests within a user story marked [P] can run in parallel
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (TDD - these should FAIL initially):
Task T010: "Unit test: Create Item with valid data succeeds"
Task T011: "Unit test: Create Item with empty name throws"
Task T012: "Unit test: Create Item with >10KB attributes throws"
Task T013: "Unit test: Create ItemName with valid value succeeds"
Task T014: "Unit test: Create ItemName with empty value throws"
# ... (all other parallel unit tests)

# After tests fail, launch implementations in parallel:
Task T022: "Create ItemName value object"
Task T023: "Create Item aggregate root"

# Then sequential tasks:
Task T024: "Implement Item.Create factory method"
# ...
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Add Items)
4. **STOP and VALIDATE**: Test User Story 1 independently via API
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP - users can add items!)
3. Add User Story 2 → Test independently → Deploy/Demo (users can now list items)
4. Add User Story 3 → Test independently → Deploy/Demo (users can view details)
5. Add User Story 4 → Test independently → Deploy/Demo (users can edit items)
6. Add User Story 5 → Test independently → Deploy/Demo (full CRUD complete)
7. Add Blazor UI (Phase 8) → Deploy/Demo (web interface ready)
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Add Items) - Priority 1, MVP
   - Developer B: User Story 2 (List Items) - Can start immediately, different files
   - Developer C: User Story 3 (View Details) - Can start immediately, different files
3. After US1 completes:
   - Developer A moves to US4 (Edit)
   - Developer D joins for US5 (Delete)
4. Stories complete and integrate independently

---

## Summary

- **Total Tasks**: 110 tasks
- **Test Tasks**: 41 tasks (37% - following TDD approach)
- **Implementation Tasks**: 69 tasks
- **Parallel Opportunities**: 52 tasks marked [P] can run concurrently
- **User Stories**: 5 independent stories + Blazor UI
- **MVP Scope**: Phase 1 + Phase 2 + Phase 3 (User Story 1) = 36 tasks

**Task Distribution by User Story**:
- US1 (Add Items): 27 tasks (12 tests + 15 implementation)
- US2 (List Items): 11 tasks (5 tests + 6 implementation)
- US3 (View Details): 8 tasks (4 tests + 4 implementation)
- US4 (Edit Items): 16 tasks (7 tests + 9 implementation)
- US5 (Delete Items): 9 tasks (6 tests + 3 implementation)
- Blazor UI: 18 tasks (6 tests + 12 implementation)
- Polish: 12 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- All tests must FAIL before writing implementation (TDD Red-Green-Refactor)
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Follow quickstart.md for detailed implementation guidance
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

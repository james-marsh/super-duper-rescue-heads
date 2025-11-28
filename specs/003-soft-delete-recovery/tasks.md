# Tasks: Soft Delete & Recovery

**Input**: Design documents from `/specs/003-soft-delete-recovery/`
**Prerequisites**: Features 001 (Collection Management) and 002 (Item Management) must be complete
**Framework**: .NET 9.0 with .NET Aspire, Entity Framework Core 9.0, Hangfire (background jobs)

**Tests**: Following TDD approach - tests written FIRST, must FAIL before implementation

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

Based on Features 001-002 structure:
- **Domain**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Domain/Items/`
- **Infrastructure**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Infrastructure/`
- **API**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Api/`
- **Web**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Web/`
- **Tests**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Tests.{Type}/`

---

## Phase 1: Setup (Infrastructure Dependencies)

**Purpose**: Install Hangfire and prepare for background job processing

- [ ] T001 Add Hangfire.AspNetCore NuGet package to SuperDuperRescueHeads.Infrastructure project
- [ ] T002 Add Hangfire.SqlServer NuGet package to SuperDuperRescueHeads.Infrastructure project
- [ ] T003 Create BackgroundJobs directory in SuperDuperRescueHeads.Infrastructure/

---

## Phase 2: Foundational (Core Soft Delete Infrastructure)

**Purpose**: Add soft delete support to domain and database

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Update IItemRepository interface with soft delete methods in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T005 Create EF migration 003_AddSoftDelete for IsDeleted and DeletedAt columns

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Soft Delete Items (Priority: P1) 🎯 MVP

**Goal**: When users delete an item, it's soft-deleted (marked as deleted) instead of permanently removed

**Independent Test**: Delete an item via DELETE /items/{id}, verify it no longer appears in list but exists in database with IsDeleted=true

### Tests for User Story 1 (TDD - Write Tests FIRST)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T006 [P] [US1] Unit test: Item.MarkAsDeleted sets IsDeleted=true and DeletedAt in SuperDuperRescueHeads.Tests.Unit/Domain/ItemSoftDeleteTests.cs
- [ ] T007 [P] [US1] Unit test: Deleted item publishes ItemDeletedEvent in SuperDuperRescueHeads.Tests.Unit/Domain/ItemSoftDeleteTests.cs
- [ ] T008 [P] [US1] Integration test: EF query filter excludes deleted items from queries in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T009 [P] [US1] Integration test: DELETE endpoint marks item as deleted, not hard delete in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs
- [ ] T010 [P] [US1] Integration test: Deleted items decrease Collection.ItemCount in SuperDuperRescueHeads.Tests.Integration/Api/ItemsEndpointsTests.cs

### Implementation for User Story 1

- [ ] T011 [US1] Add IsDeleted and DeletedAt properties to Item aggregate in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T012 [US1] Implement Item.MarkAsDeleted method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T013 [US1] Update ItemConfiguration with HasQueryFilter for soft delete in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ItemConfiguration.cs
- [ ] T014 [US1] Apply migration 003_AddSoftDelete to database using dotnet ef database update
- [ ] T015 [US1] Update ItemRepository.DeleteAsync to call MarkAsDeleted instead of Remove in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T016 [US1] Verify DELETE endpoint behavior unchanged (API contract preserved) in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs

**Checkpoint**: Items are now soft-deleted - they disappear from lists but remain in database for recovery

---

## Phase 4: User Story 2 - View Deleted Items (Priority: P2)

**Goal**: Users can view a list of their deleted items with deletion dates

**Independent Test**: Soft-delete 3 items, GET /deleted-items, verify returns all 3 with DeletedAt timestamps

### Tests for User Story 2 (TDD - Write Tests FIRST)

- [ ] T017 [P] [US2] Integration test: GET /deleted-items returns only soft-deleted items for user in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T018 [P] [US2] Integration test: GET /deleted-items for user with no deleted items returns empty list in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T019 [P] [US2] Integration test: ItemRepository.GetDeletedItemsAsync uses IgnoreQueryFilters() in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs

### Implementation for User Story 2

- [ ] T020 [US2] Add GetDeletedItemsAsync method to IItemRepository in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T021 [US2] Implement ItemRepository.GetDeletedItemsAsync with IgnoreQueryFilters() in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T022 [US2] Create DeletedItemsEndpoints.cs in SuperDuperRescueHeads.Api/Endpoints/
- [ ] T023 [US2] Implement GET /api/v1/deleted-items endpoint in SuperDuperRescueHeads.Api/Endpoints/DeletedItemsEndpoints.cs
- [ ] T024 [US2] Register DeletedItemsEndpoints in SuperDuperRescueHeads.Api/Program.cs

**Checkpoint**: Users can now see their deleted items

---

## Phase 5: User Story 3 - Restore Deleted Items (Priority: P3)

**Goal**: Users can restore (undelete) items within the 30-day retention period

**Independent Test**: Soft-delete an item, POST /deleted-items/{id}/restore, verify item reappears in collection list

### Tests for User Story 3 (TDD - Write Tests FIRST)

- [ ] T025 [P] [US3] Unit test: Item.Restore clears IsDeleted and DeletedAt in SuperDuperRescueHeads.Tests.Unit/Domain/ItemSoftDeleteTests.cs
- [ ] T026 [P] [US3] Unit test: Restored item publishes ItemRestoredEvent in SuperDuperRescueHeads.Tests.Unit/Domain/ItemSoftDeleteTests.cs
- [ ] T027 [P] [US3] Integration test: POST /deleted-items/{id}/restore restores item in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T028 [P] [US3] Integration test: Restored item increments Collection.ItemCount in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T029 [P] [US3] Integration test: Restore non-existent item returns 404 in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T030 [P] [US3] E2E test: Delete item, restore from deleted items, verify in collection in SuperDuperRescueHeads.Tests.E2E/UserJourneys/RecoverItemE2ETests.cs

### Implementation for User Story 3

- [ ] T031 [US3] Implement Item.Restore method in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T032 [US3] Add ItemRestoredEvent to ItemDomainEvents.cs in SuperDuperRescueHeads.Domain/Items/
- [ ] T033 [US3] Add GetDeletedItemByIdAsync method to IItemRepository in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T034 [US3] Implement ItemRepository.GetDeletedItemByIdAsync in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T035 [US3] Implement POST /api/v1/deleted-items/{id}/restore endpoint in SuperDuperRescueHeads.Api/Endpoints/DeletedItemsEndpoints.cs
- [ ] T036 [US3] Add authorization check (user owns item) to restore endpoint in SuperDuperRescueHeads.Api/Endpoints/DeletedItemsEndpoints.cs

**Checkpoint**: Users can now restore deleted items - full soft delete/recovery cycle working

---

## Phase 6: User Story 4 - Permanent Delete (Priority: P4)

**Goal**: Users can permanently delete items immediately (bypass 30-day retention)

**Independent Test**: Soft-delete item, POST /deleted-items/{id}/purge, verify item completely removed from database

### Tests for User Story 4 (TDD - Write Tests FIRST)

- [ ] T037 [P] [US4] Integration test: POST /deleted-items/{id}/purge permanently deletes item in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T038 [P] [US4] Integration test: Purge decrements Collection.ItemCount in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs
- [ ] T039 [P] [US4] Integration test: Purge non-existent item returns 404 in SuperDuperRescueHeads.Tests.Integration/Api/DeletedItemsEndpointsTests.cs

### Implementation for User Story 4

- [ ] T040 [US4] Add PurgeAsync method to IItemRepository in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T041 [US4] Implement ItemRepository.PurgeAsync (hard delete) in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T042 [US4] Implement POST /api/v1/deleted-items/{id}/purge endpoint in SuperDuperRescueHeads.Api/Endpoints/DeletedItemsEndpoints.cs

**Checkpoint**: Users have full control - soft delete, restore, or permanent delete

---

## Phase 7: User Story 5 - Automated Purge After 30 Days (Priority: P5)

**Goal**: Automatically purge items deleted >30 days ago via daily background job

**Independent Test**: Create items with DeletedAt = 31 days ago, run purge job, verify items removed from database

### Tests for User Story 5 (TDD - Write Tests FIRST)

- [ ] T043 [P] [US5] Integration test: GetExpiredDeletedItemsAsync returns items >30 days old in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T044 [P] [US5] Integration test: PurgeExpiredItemsAsync removes expired items in SuperDuperRescueHeads.Tests.Integration/Data/ItemRepositoryTests.cs
- [ ] T045 [P] [US5] Integration test: PurgeDeletedItemsJob executes successfully in SuperDuperRescueHeads.Tests.Integration/BackgroundJobs/PurgeDeletedItemsJobTests.cs
- [ ] T046 [P] [US5] Integration test: Purge job processes items in batches in SuperDuperRescueHeads.Tests.Integration/BackgroundJobs/PurgeDeletedItemsJobTests.cs

### Implementation for User Story 5

- [ ] T047 [US5] Add GetExpiredDeletedItemsAsync to IItemRepository in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T048 [US5] Add PurgeExpiredItemsAsync to IItemRepository in SuperDuperRescueHeads.Domain/Items/IItemRepository.cs
- [ ] T049 [US5] Implement ItemRepository.GetExpiredDeletedItemsAsync in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T050 [US5] Implement ItemRepository.PurgeExpiredItemsAsync with batch processing in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs
- [ ] T051 [US5] Create PurgeDeletedItemsJob in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/
- [ ] T052 [US5] Configure Hangfire in SuperDuperRescueHeads.Api/Program.cs or AppHost
- [ ] T053 [US5] Register recurring job to run daily at 2 AM in SuperDuperRescueHeads.Api/Program.cs
- [ ] T054 [US5] Add structured logging to purge job in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/PurgeDeletedItemsJob.cs
- [ ] T055 [US5] Add alerting for purge job failures in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/PurgeDeletedItemsJob.cs

**Checkpoint**: Automated purging working - items auto-delete after 30 days

---

## Phase 8: Blazor UI for Deleted Items (Priority: P6)

**Goal**: Users can view and restore deleted items through the web interface

**Independent Test**: Navigate to /deleted-items, see deleted items, click Restore, verify item back in collection

### Tests for UI (bUnit Component Tests)

- [ ] T056 [P] [UI] Page test: DeletedItems/Index displays deleted items with dates in SuperDuperRescueHeads.Tests.UI/Pages/DeletedItemsIndexTests.cs
- [ ] T057 [P] [UI] Page test: Restore button calls ItemService.RestoreAsync in SuperDuperRescueHeads.Tests.UI/Pages/DeletedItemsIndexTests.cs
- [ ] T058 [P] [UI] E2E test: Restore workflow completes successfully in SuperDuperRescueHeads.Tests.E2E/UserJourneys/RecoverItemE2ETests.cs

### Implementation for UI

- [ ] T059 [UI] Add GetDeletedItemsAsync to ItemService in SuperDuperRescueHeads.Web/Services/ItemService.cs
- [ ] T060 [UI] Add RestoreItemAsync to ItemService in SuperDuperRescueHeads.Web/Services/ItemService.cs
- [ ] T061 [UI] Add PurgeItemAsync to ItemService in SuperDuperRescueHeads.Web/Services/ItemService.cs
- [ ] T062 [UI] Create DeletedItems/Index.razor page in SuperDuperRescueHeads.Web/Components/Pages/DeletedItems/
- [ ] T063 [UI] Create DeletedItems/Restore.razor confirmation modal in SuperDuperRescueHeads.Web/Components/Pages/DeletedItems/
- [ ] T064 [UI] Add navigation link to "Deleted Items" in main menu in SuperDuperRescueHeads.Web/Components/Layout/NavMenu.razor
- [ ] T065 [UI] Style Deleted Items pages with Tailwind CSS in SuperDuperRescueHeads.Web/Components/Pages/DeletedItems/

**Checkpoint**: Full UI for deleted items management

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T066 [P] Add retention period configuration (default 30 days) in SuperDuperRescueHeads.Api/appsettings.json
- [ ] T067 [P] Create indexes on IsDeleted and DeletedAt columns for query performance
- [ ] T068 [P] Add metrics for purge job (items purged, batch time) in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/
- [ ] T069 [P] Add telemetry for soft delete/restore operations in SuperDuperRescueHeads.Api/Endpoints/
- [ ] T070 [P] Update API documentation with deleted items endpoints
- [ ] T071 [P] Add XML comments to soft delete methods in SuperDuperRescueHeads.Domain/Items/
- [ ] T072 Validate quickstart.md implementation guide accuracy
- [ ] T073 Run all tests and verify 80%+ code coverage
- [ ] T074 Performance test: Verify purge job processes 10,000 items in <5 minutes
- [ ] T075 Security review: Verify authorization on all deleted items endpoints

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - install Hangfire packages
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (Soft Delete) is foundational for US2-5
  - US2-4 can proceed in parallel after US1
  - US5 (Automated Purge) can proceed in parallel with US2-4
- **Blazor UI (Phase 8)**: Depends on US1-3 endpoints being complete
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundational - BLOCKS US2-5
- **User Story 2 (P2)**: Depends on US1 - Can run parallel with US3-5
- **User Story 3 (P3)**: Depends on US1 - Can run parallel with US2, US4-5
- **User Story 4 (P4)**: Depends on US1 - Can run parallel with US2-3, US5
- **User Story 5 (P5)**: Depends on US1 - Can run parallel with US2-4

### Parallel Opportunities

- After US1 completes, US2-5 can all run in parallel
- All tests within a user story marked [P] can run in parallel
- All Polish tasks marked [P] can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1-3 Only)

1. Complete Phase 1: Setup (Hangfire)
2. Complete Phase 2: Foundational (soft delete properties + migration)
3. Complete Phase 3: User Story 1 (Soft Delete)
4. Complete Phase 4: User Story 2 (View Deleted)
5. Complete Phase 5: User Story 3 (Restore)
6. **STOP and VALIDATE**: Test soft delete → view → restore workflow
7. Deploy/demo if ready

### Incremental Delivery

1. Add US1 (Soft Delete) → Test → Deploy (items now soft-deleted instead of hard-deleted)
2. Add US2 (View Deleted) → Test → Deploy (users can see what was deleted)
3. Add US3 (Restore) → Test → Deploy (users can recover mistakes - MVP complete!)
4. Add US4 (Permanent Delete) → Test → Deploy (power users can force delete)
5. Add US5 (Automated Purge) → Test → Deploy (automatic cleanup)
6. Add Blazor UI → Test → Deploy (web interface ready)

---

## Summary

- **Total Tasks**: 75 tasks
- **Test Tasks**: 26 tasks (35% - following TDD approach)
- **Implementation Tasks**: 49 tasks
- **Parallel Opportunities**: 28 tasks marked [P] can run concurrently
- **User Stories**: 5 independent stories + Blazor UI
- **MVP Scope**: Phase 1 + Phase 2 + Phase 3-5 (US1-3) = 36 tasks

**Task Distribution by User Story**:
- US1 (Soft Delete Items): 13 tasks (5 tests + 8 implementation)
- US2 (View Deleted Items): 8 tasks (3 tests + 5 implementation)
- US3 (Restore Deleted Items): 12 tasks (6 tests + 6 implementation)
- US4 (Permanent Delete): 6 tasks (3 tests + 3 implementation)
- US5 (Automated Purge): 13 tasks (4 tests + 9 implementation)
- Blazor UI: 10 tasks (3 tests + 7 implementation)
- Polish: 10 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- All tests must FAIL before writing implementation (TDD Red-Green-Refactor)
- This feature modifies existing Item aggregate from Feature 002
- EF Core query filter automatically excludes deleted items from all queries
- Use IgnoreQueryFilters() to access deleted items explicitly
- Hangfire provides reliable background job processing for automated purge
- 30-day retention period is configurable
- Follow quickstart.md for detailed implementation guidance

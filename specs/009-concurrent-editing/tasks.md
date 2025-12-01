# Tasks: Concurrent Editing & Collaboration

**Input**: Design documents from `/specs/009-concurrent-editing/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md

**Tests**: Not explicitly requested - implementation focused

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Project structure (from plan.md):
- **Domain**: `SuperDuperRescueHeads.Domain/`
- **Infrastructure**: `SuperDuperRescueHeads.Infrastructure/`
- **API**: `SuperDuperRescueHeads.Api/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Review existing codebase and plan concurrent editing integration

- [ ] T001 Review existing Item entity in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T002 [P] Review existing Collection entity structure and relationships
- [ ] T003 [P] Review ItemRepository in SuperDuperRescueHeads.Infrastructure/Repositories/ItemRepository.cs
- [ ] T004 [P] Review ItemsEndpoints in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T005 [P] Review NotificationService integration from Feature 008
- [ ] T006 [P] Review SignalR infrastructure from Feature 008 (NotificationHub)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core concurrency infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 Add RowVersion byte[] property to Item entity in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T008 [P] Add RowVersion byte[] property to Collection entity (if not already present)
- [ ] T009 Update ItemConfiguration to include RowVersion in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ItemConfiguration.cs
- [ ] T010 Create migration for RowVersion columns using dotnet ef migrations add
- [ ] T011 Create ConcurrencyException custom exception in SuperDuperRescueHeads.Domain/Shared/ConcurrencyException.cs
- [ ] T012 [P] Create ConflictResolutionResult value object in SuperDuperRescueHeads.Domain/Items/ConflictResolutionResult.cs
- [ ] T013 Apply migration and verify database schema update

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 4 - Optimistic Locking with Version Control (Priority: P1) 🎯 MVP Foundation

**Goal**: Implement version tracking for items to detect concurrent modifications

**Independent Test**: Load same item in two contexts, modify and save first, verify second save detects version conflict

### Implementation for User Story 4

- [ ] T014 [US4] Update Item.UpdateDetails() to preserve RowVersion in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T015 [US4] Update Item.UpdateType() to preserve RowVersion in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T016 [US4] Add GetCurrentVersion() method to Item entity in SuperDuperRescueHeads.Domain/Items/Item.cs
- [ ] T017 [US4] Update ItemRepository.UpdateAsync to handle DbUpdateConcurrencyException in SuperDuperRescueHeads.Infrastructure/Repositories/ItemRepository.cs
- [ ] T018 [US4] Add TryReloadAsync method to ItemRepository for fetching latest version in SuperDuperRescueHeads.Infrastructure/Repositories/ItemRepository.cs
- [ ] T019 [US4] Update ItemsEndpoints PUT /api/v1/items/{id} to handle concurrency conflicts in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T020 [US4] Add version information to item response DTOs in SuperDuperRescueHeads.Api/Endpoints/ItemsEndpoints.cs
- [ ] T021 [US4] Add If-Match header support for optimistic concurrency in ItemsEndpoints
- [ ] T022 [US4] Test version increment on successful save
- [ ] T023 [US4] Test conflict detection when version mismatch occurs

**Checkpoint**: Version tracking is working - conflicts can be detected

---

## Phase 4: User Story 1 - Concurrent Item Editing with Conflict Resolution (Priority: P2) 🎯 MVP

**Goal**: Enable multiple users to edit items with proper conflict resolution and notifications

**Independent Test**: Two users edit same item simultaneously, first save succeeds, second receives conflict notification with details

### Implementation for User Story 1

- [ ] T024 [P] [US1] Create ConflictEvent entity in SuperDuperRescueHeads.Domain/Items/ConflictEvent.cs
- [ ] T025 [P] [US1] Create IConflictEventRepository interface in SuperDuperRescueHeads.Domain/Items/IConflictEventRepository.cs
- [ ] T026 [US1] Create ConflictEventConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ConflictEventConfiguration.cs
- [ ] T027 [US1] Create ConflictEventRepository implementation in SuperDuperRescueHeads.Infrastructure/Repositories/ConflictEventRepository.cs
- [ ] T028 [US1] Add ConflictEvent DbSet to ApplicationDbContext in SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T029 [US1] Create migration for ConflictEvent table
- [ ] T030 [US1] Create IConflictResolutionService interface in SuperDuperRescueHeads.Domain/Items/IConflictResolutionService.cs
- [ ] T031 [US1] Implement ConflictResolutionService in SuperDuperRescueHeads.Infrastructure/Services/ConflictResolutionService.cs
- [ ] T032 [US1] Add HandleConcurrencyConflict method to ConflictResolutionService (last-write-wins logic)
- [ ] T033 [US1] Add RecordConflictEvent method to log conflicts in ConflictResolutionService
- [ ] T034 [US1] Integrate ConflictResolutionService with ItemRepository.UpdateAsync
- [ ] T035 [US1] Create conflict notification using NotificationService when conflict occurs
- [ ] T036 [US1] Add ConflictDetected notification type support (already exists in Feature 008)
- [ ] T037 [US1] Update ItemsEndpoints to return conflict details with 409 Conflict status
- [ ] T038 [US1] Add GET /api/v1/items/{id}/current endpoint to fetch latest version after conflict
- [ ] T039 [US1] Register ConflictResolutionService in Program.cs
- [ ] T040 [US1] Test conflict notification delivery to losing user
- [ ] T041 [US1] Test conflict details include winner user ID and changes made
- [ ] T042 [US1] Test user can fetch current version after conflict

**Checkpoint**: Conflict resolution working - users notified and can recover from conflicts

---

## Phase 5: User Story 2 - Concurrent Item Addition (Priority: P3)

**Goal**: Verify multiple users can add items concurrently without conflicts

**Independent Test**: Multiple users add different items to same collection simultaneously, all succeed

### Implementation for User Story 2

- [ ] T043 [US2] Review existing POST /api/v1/items endpoint for concurrent addition support
- [ ] T044 [US2] Verify ItemRepository.AddAsync handles concurrent additions correctly
- [ ] T045 [US2] Add transaction isolation level review for item additions
- [ ] T046 [US2] Test concurrent item additions from multiple users (integration test scenario)
- [ ] T047 [US2] Test all concurrent additions are persisted successfully
- [ ] T048 [US2] Document concurrent addition guarantees in code comments

**Checkpoint**: Concurrent additions verified working - no conflicts occur

---

## Phase 6: User Story 3 - Real-Time Collaboration Awareness (Priority: P4)

**Goal**: Show real-time indicators when users are viewing/editing items in shared collections

**Independent Test**: User opens item for editing, other user sees editing indicator with username

### Implementation for User Story 3

- [ ] T049 [P] [US3] Create EditSession entity in SuperDuperRescueHeads.Domain/Items/EditSession.cs
- [ ] T050 [P] [US3] Create ActivityType enum (Viewing, Editing) in SuperDuperRescueHeads.Domain/Items/ActivityType.cs
- [ ] T051 [P] [US3] Create IEditSessionRepository interface in SuperDuperRescueHeads.Domain/Items/IEditSessionRepository.cs
- [ ] T052 [US3] Create EditSessionConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/EditSessionConfiguration.cs
- [ ] T053 [US3] Create EditSessionRepository in SuperDuperRescueHeads.Infrastructure/Repositories/EditSessionRepository.cs
- [ ] T054 [US3] Add EditSession DbSet to ApplicationDbContext
- [ ] T055 [US3] Create migration for EditSession table
- [ ] T056 [US3] Create PresenceHub (SignalR) in SuperDuperRescueHeads.Api/Hubs/PresenceHub.cs
- [ ] T057 [US3] Implement OnConnectedAsync for presence tracking in PresenceHub
- [ ] T058 [US3] Implement OnDisconnectedAsync for cleanup in PresenceHub
- [ ] T059 [US3] Add StartEditing(itemId) method to PresenceHub
- [ ] T060 [US3] Add StopEditing(itemId) method to PresenceHub
- [ ] T061 [US3] Add StartViewing(itemId) method to PresenceHub
- [ ] T062 [US3] Add StopViewing(itemId) method to PresenceHub
- [ ] T063 [US3] Add BroadcastEditingIndicator method to PresenceHub
- [ ] T064 [US3] Add BroadcastViewingIndicator method to PresenceHub
- [ ] T065 [US3] Create IPresenceService interface in SuperDuperRescueHeads.Domain/Items/IPresenceService.cs
- [ ] T066 [US3] Implement PresenceService in SuperDuperRescueHeads.Infrastructure/Services/PresenceService.cs
- [ ] T067 [US3] Add StartEditSession method to PresenceService
- [ ] T068 [US3] Add EndEditSession method to PresenceService
- [ ] T069 [US3] Add GetActiveEditSessions method to PresenceService (for collection or item)
- [ ] T070 [US3] Add UpdateLastActivity method to PresenceService
- [ ] T071 [US3] Add CleanupInactiveSessions method to PresenceService (5 minute timeout)
- [ ] T072 [US3] Create background job for session cleanup in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/CleanupEditSessionsJob.cs
- [ ] T073 [US3] Register CleanupEditSessionsJob with Hangfire in Program.cs (run every minute)
- [ ] T074 [US3] Add GET /api/v1/items/{id}/active-sessions endpoint in ItemsEndpoints
- [ ] T075 [US3] Add GET /api/v1/collections/{id}/active-sessions endpoint (new endpoint file or existing)
- [ ] T076 [US3] Register PresenceHub at /hubs/presence in Program.cs
- [ ] T077 [US3] Register PresenceService in Program.cs
- [ ] T078 [US3] Test editing indicator appears within 2 seconds
- [ ] T079 [US3] Test editing indicator removed on StopEditing
- [ ] T080 [US3] Test session timeout after 5 minutes of inactivity
- [ ] T081 [US3] Test multiple users editing different items show separate indicators

**Checkpoint**: Real-time presence indicators working - users see collaborative activity

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Performance, monitoring, and integration improvements

- [ ] T082 [P] Add conflict rate monitoring and logging in ConflictResolutionService
- [ ] T083 [P] Add performance metrics for presence updates
- [ ] T084 [P] Create GET /api/v1/admin/conflicts endpoint for monitoring conflict events
- [ ] T085 [P] Add indexes for ConflictEvent queries (UserId, ItemId, CreatedAt)
- [ ] T086 [P] Add indexes for EditSession queries (ItemId, UserId, IsActive, LastActivityAt)
- [ ] T087 [P] Update CLAUDE.md with Feature 009 architecture notes
- [ ] T088 Performance test: 50 concurrent users editing different items
- [ ] T089 Performance test: Conflict detection latency < 50ms
- [ ] T090 Performance test: Presence update latency < 200ms
- [ ] T091 Integration: Test concurrent editing with shared collections (Feature 006)
- [ ] T092 Integration: Test concurrent editing with group-shared collections (Feature 007)
- [ ] T093 Integration: Test conflict notifications delivered via NotificationHub (Feature 008)
- [ ] T094 Edge case: Test edit when item deleted by another user
- [ ] T095 Edge case: Test edit when permission revoked while editing
- [ ] T096 Edge case: Test 3+ users editing same item simultaneously
- [ ] T097 Edge case: Test network disconnect during editing session
- [ ] T098 Code review: Ensure all DbUpdateConcurrencyException handled correctly
- [ ] T099 Code review: Verify transaction isolation levels appropriate
- [ ] T100 Documentation: Add concurrent editing examples to quickstart.md
- [ ] T101 Documentation: Document conflict resolution behavior
- [ ] T102 Build and run full test suite

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 4 (Phase 3)**: Depends on Foundational - Must complete before US1
- **User Story 1 (Phase 4)**: Depends on US4 completion (needs version tracking)
- **User Story 2 (Phase 5)**: Depends on Foundational only - Can run parallel with US1/US4
- **User Story 3 (Phase 6)**: Depends on Foundational only - Can run parallel with US1
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 4 (P1)**: Foundation - Required by US1
- **User Story 1 (P2)**: Depends on US4 (version tracking needed for conflict detection)
- **User Story 2 (P3)**: Independent - Can start after Foundational
- **User Story 3 (P4)**: Independent - Can start after Foundational (can run parallel with US1)

### Within Each User Story

- Phase 3 (US4): Version tracking must work before moving to conflict resolution
- Phase 4 (US1): Entity/Repository before Service, Service before Endpoints
- Phase 6 (US3): EditSession entity/repo before PresenceService, PresenceService before Hub

### Parallel Opportunities

- Phase 1: All review tasks marked [P] can run in parallel
- Phase 2: T008, T012 can run in parallel with other foundational tasks
- Phase 4: T024, T025 (entity/interface) can run in parallel
- Phase 6: T049, T050, T051 (entity/enum/interface) can run in parallel
- Once US4 completes: US2 and US3 can be worked in parallel
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch entity and interface creation together:
Task: "Create ConflictEvent entity in SuperDuperRescueHeads.Domain/Items/ConflictEvent.cs"
Task: "Create IConflictEventRepository interface in SuperDuperRescueHeads.Domain/Items/IConflictEventRepository.cs"
```

---

## Parallel Example: User Story 3

```bash
# Launch foundational models together:
Task: "Create EditSession entity in SuperDuperRescueHeads.Domain/Items/EditSession.cs"
Task: "Create ActivityType enum in SuperDuperRescueHeads.Domain/Items/ActivityType.cs"
Task: "Create IEditSessionRepository interface in SuperDuperRescueHeads.Domain/Items/IEditSessionRepository.cs"
```

---

## Implementation Strategy

### MVP First (User Story 4 + User Story 1)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 4 (Version tracking foundation)
4. Complete Phase 4: User Story 1 (Conflict resolution)
5. **STOP and VALIDATE**: Test version tracking and conflict resolution
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 4 → Test version tracking works
3. Add User Story 1 → Test conflict resolution → Deploy/Demo (MVP!)
4. Add User Story 2 → Test concurrent additions → Deploy/Demo
5. Add User Story 3 → Test presence indicators → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Complete User Story 4 together (foundation for US1)
3. Once US4 is done:
   - Developer A: User Story 1 (depends on US4)
   - Developer B: User Story 2 (independent)
   - Developer C: User Story 3 (independent)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- EF Core RowVersion is byte[] that automatically updates on save
- DbUpdateConcurrencyException thrown when RowVersion mismatch detected
- Last-write-wins: First save succeeds, second save rejected with notification
- EditSession tracks active users (5 minute timeout via background job)
- SignalR PresenceHub provides real-time presence indicators
- Concurrent additions never conflict (new GUIDs for each item)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently

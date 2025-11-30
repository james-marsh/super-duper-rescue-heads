# Implementation Tasks: Group Sharing

**Feature**: 007-group-sharing
**Generated**: 2025-11-30
**Total Tasks**: 93
**Parallel Opportunities**: 47 tasks (51%)

## Task Summary

| Phase | User Story | Tasks | Parallel | Priority |
|-------|-----------|-------|----------|----------|
| 1 | Setup | 5 | 2 | - |
| 2 | Foundational | 18 | 10 | - |
| 3 | US1: Share with Groups | 28 | 14 | P1 |
| 4 | US2: Auto Access Updates | 24 | 13 | P2 |
| 5 | US3: Manage Permissions | 12 | 6 | P3 |
| 6 | Polish | 6 | 2 | - |

## Implementation Strategy

**MVP Scope** (Phase 1-3): User Story 1 only
- Complete group sharing foundation
- Independently testable: Share collection with group, verify all members have access
- Estimated: 51 tasks (5 setup + 18 foundational + 28 US1)

**Incremental Delivery**:
1. MVP: US1 - Manual group sharing (no auto-sync yet)
2. Phase 2: US2 - Add automatic membership sync
3. Phase 3: US3 - Add advanced permission management

**Parallel Execution**: Each user story phase can be parallelized:
- US1: API endpoints + UI components + repository methods
- US2: Sync service + background jobs + event handlers
- US3: Permission logic + management endpoints + UI

---

## Phase 1: Setup & Prerequisites

**Goal**: Review Feature 006 implementation and prepare project structure for group sharing extensions.

**Prerequisites**: Feature 006 (Basic Sharing) must be fully implemented and deployed.

### Tasks

- [ ] T001 Review Feature 006 CollectionShare implementation in SuperDuperRescueHeads.Domain/Sharing/CollectionShare.cs
- [ ] T002 [P] Review Feature 006 authorization handlers in SuperDuperRescueHeads.Api/Authorization/CollectionPermissionHandler.cs
- [ ] T003 [P] Review Feature 006 API endpoints in SuperDuperRescueHeads.Api/Endpoints/CollectionSharingEndpoints.cs
- [ ] T004 Create feature branch and update CLAUDE.md with Feature 007 context
- [ ] T005 Document extension strategy: how group sharing extends individual sharing without breaking existing functionality

---

## Phase 2: Foundational Components

**Goal**: Create core group management entities, repositories, and database schema that will be used by all user stories.

**Independent Test**: Cannot test in isolation - these are blocking prerequisites for all user stories.

**Why Blocking**: All user stories depend on UserGroup entity and basic group management infrastructure.

### Tasks

#### Domain Models

- [ ] T006 [P] Create GroupMemberRole enum in SuperDuperRescueHeads.Domain/Groups/GroupMemberRole.cs (Owner, Admin, Member)
- [ ] T007 [P] Create GroupMember entity in SuperDuperRescueHeads.Domain/Groups/GroupMember.cs with UserId, Role, JoinedAt
- [ ] T008 Create UserGroup aggregate root in SuperDuperRescueHeads.Domain/Groups/UserGroup.cs with AddMember(), RemoveMember(), ChangeRole() methods
- [ ] T009 [P] Add validation to UserGroup: max 50 members per group, unique member constraint
- [ ] T010 [P] Create IUserGroupRepository interface in SuperDuperRescueHeads.Domain/Groups/IUserGroupRepository.cs with GetByIdAsync(), GetByUserIdAsync(), GetMembersAsync()

#### Group Sync Infrastructure

- [ ] T011 [P] Create GroupSyncStatus enum in SuperDuperRescueHeads.Domain/Groups/GroupSyncStatus.cs (Pending, InProgress, Completed, Failed)
- [ ] T012 [P] Create GroupSyncEvent entity in SuperDuperRescueHeads.Domain/Groups/GroupSyncEvent.cs to track sync history
- [ ] T013 [P] Create IGroupSyncService interface in SuperDuperRescueHeads.Domain/Groups/IGroupSyncService.cs for external system integration

#### Infrastructure Layer

- [ ] T014 [P] Create UserGroupConfiguration EF Core mapping in SuperDuperRescueHeads.Infrastructure/Data/Configurations/UserGroupConfiguration.cs
- [ ] T015 [P] Create GroupMemberConfiguration EF Core mapping in SuperDuperRescueHeads.Infrastructure/Data/Configurations/GroupMemberConfiguration.cs
- [ ] T016 [P] Create GroupSyncEventConfiguration EF Core mapping in SuperDuperRescueHeads.Infrastructure/Data/Configurations/GroupSyncEventConfiguration.cs
- [ ] T017 Add DbSet<UserGroup>, DbSet<GroupMember>, DbSet<GroupSyncEvent> to SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T018 Create migration AddUserGroups in SuperDuperRescueHeads.Infrastructure/Migrations/ with indexes on GroupId, UserId, and (GroupId, UserId)
- [ ] T019 [P] Implement UserGroupRepository in SuperDuperRescueHeads.Infrastructure/Repositories/UserGroupRepository.cs with all CRUD operations
- [ ] T020 [P] Create stub GroupSyncService in SuperDuperRescueHeads.Infrastructure/Services/GroupSyncService.cs (logs only, ready for external integration)

#### Service Registration

- [ ] T021 Register IUserGroupRepository in SuperDuperRescueHeads.Api/Program.cs
- [ ] T022 Register IGroupSyncService in SuperDuperRescueHeads.Api/Program.cs
- [ ] T023 Build solution and verify 0 errors, 0 warnings

---

## Phase 3: User Story 1 - Share Collection with User Group

**Story Goal**: Collection owners can share collections with user groups, granting all group members access according to the group's permission level.

**Priority**: P1 (MVP)

**Independent Test Criteria**:
1. Create a user group with 3 members
2. Share a collection with the group (ViewOnly permission)
3. Verify all 3 members can view the collection
4. Verify members cannot edit (permission enforcement)
5. Share another collection with Edit permission
6. Verify all members can now edit

**Acceptance Criteria**:
- ✅ Group members receive access immediately after group is granted permission
- ✅ Permission level (ViewOnly/Edit) applies to all group members
- ✅ Groups appear in recipient search alongside individual users
- ✅ Group permissions show member count in permission list

### Tasks

#### Extend Domain Model for Group Sharing

- [ ] T024 [P] [US1] Add GroupId property (nullable) to CollectionShare entity in SuperDuperRescueHeads.Domain/Sharing/CollectionShare.cs
- [ ] T025 [P] [US1] Add IsGroupShare computed property to CollectionShare in SuperDuperRescueHeads.Domain/Sharing/CollectionShare.cs
- [ ] T026 [US1] Create CreateGroupInvitation() factory method in CollectionShare for group-based shares
- [ ] T027 [P] [US1] Add validation: CollectionShare must have either SharedWithUserId OR GroupId (not both, not neither)
- [ ] T028 [P] [US1] Update CollectionShare.Revoke() to handle group shares differently (affects all members)

#### Repository Extensions

- [ ] T029 [P] [US1] Add GetByGroupIdAsync() method to ICollectionShareRepository interface
- [ ] T030 [P] [US1] Add GetGroupSharesByCollectionIdAsync() method to ICollectionShareRepository interface
- [ ] T031 [P] [US1] Implement new methods in CollectionShareRepository in SuperDuperRescueHeads.Infrastructure/Repositories/CollectionShareRepository.cs

#### Database Migration

- [ ] T032 [US1] Update CollectionShareConfiguration to add GroupId column and constraints in SuperDuperRescueHeads.Infrastructure/Data/Configurations/CollectionShareConfiguration.cs
- [ ] T033 [US1] Create migration AddGroupSharingSupport with GroupId column, index, and check constraint
- [ ] T034 [US1] Verify migration applies cleanly and rollback works

#### API Endpoints for Group Sharing

- [ ] T035 [P] [US1] Create ShareWithGroupRequest model in SuperDuperRescueHeads.Api/Models/ShareWithGroupRequest.cs (GroupId, Permission)
- [ ] T036 [P] [US1] Create GroupShareResponse model in SuperDuperRescueHeads.Api/Models/GroupShareResponse.cs (includes member count)
- [ ] T037 [US1] Add POST /api/v1/collections/{collectionId}/share/group endpoint to CollectionSharingEndpoints.cs
- [ ] T038 [US1] Add GET /api/v1/groups endpoint to list available groups for current user
- [ ] T039 [US1] Add GET /api/v1/collections/{collectionId}/shares/groups endpoint to list group shares

#### Authorization Updates

- [ ] T040 [US1] Update CollectionPermissionHandler to resolve group memberships in SuperDuperRescueHeads.Api/Authorization/CollectionPermissionHandler.cs
- [ ] T041 [P] [US1] Add GetEffectivePermissionAsync() method that checks both individual and group shares
- [ ] T042 [P] [US1] Cache group membership lookups for 5 minutes to reduce database queries

#### UI Components (Blazor)

- [ ] T043 [P] [US1] Create GroupSelector.razor component in SuperDuperRescueHeads.Web/Components/Sharing/ for group search/selection
- [ ] T044 [P] [US1] Create GroupShareList.razor component in SuperDuperRescueHeads.Web/Components/Sharing/ to display group shares with member counts
- [ ] T045 [US1] Update ShareCollectionDialog.razor to include group sharing option
- [ ] T046 [US1] Add visual distinction between individual and group shares in permission list UI

#### Testing & Validation

- [ ] T047 [US1] Test sharing collection with group (3 members) - verify all can access per spec.md acceptance scenario 1
- [ ] T048 [US1] Test ViewOnly vs Edit permissions for group members per spec.md acceptance scenario 2
- [ ] T049 [US1] Test group appears in search results per spec.md acceptance scenario 3
- [ ] T050 [US1] Test permission list shows group with member count per spec.md acceptance scenario 4
- [ ] T051 [US1] Build and verify 0 errors, 0 warnings

---

## Phase 4: User Story 2 - Automatic Access Updates on Group Membership Changes

**Story Goal**: When users are added/removed from groups, their collection access automatically updates without manual intervention.

**Priority**: P2

**Independent Test Criteria**:
1. Share collection with group (2 members)
2. Add 3rd user to group via sync
3. Verify new user automatically has access within 30 seconds
4. Remove original member from group via sync
5. Verify removed user loses access within 30 seconds
6. Add user with existing individual access to group
7. Verify user retains highest permission level

**Acceptance Criteria**:
- ✅ New group members receive access within 30 seconds
- ✅ Removed members lose access within 30 seconds (unless individual access exists)
- ✅ Multiple group memberships handled correctly (access retained if in any group)
- ✅ Individual + group access coexist correctly

### Tasks

#### Group Sync Service Implementation

- [ ] T052 [P] [US2] Implement SyncGroupMembershipAsync() in GroupSyncService to query external system
- [ ] T053 [P] [US2] Create GroupMembershipChangedEvent domain event in SuperDuperRescueHeads.Domain/Groups/GroupMembershipChangedEvent.cs
- [ ] T054 [P] [US2] Add ProcessMemberAddedAsync() handler in GroupSyncService for new members
- [ ] T055 [P] [US2] Add ProcessMemberRemovedAsync() handler in GroupSyncService for removed members
- [ ] T056 [US2] Implement sync logging to GroupSyncEvent table for audit trail

#### Permission Resolution Logic

- [ ] T057 [P] [US2] Create EffectivePermission value object in SuperDuperRescueHeads.Domain/Sharing/EffectivePermission.cs
- [ ] T058 [US2] Implement ComputeEffectivePermission() that aggregates individual + all group permissions (most permissive wins)
- [ ] T059 [P] [US2] Add GetAccessSourcesAsync() method to show individual vs group sources in UI
- [ ] T060 [P] [US2] Handle edge case: user in multiple groups with different permissions (most permissive wins per spec.md assumption 4)

#### Background Job for Periodic Sync

- [ ] T061 [P] [US2] Create SyncGroupMembershipsJob in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/SyncGroupMembershipsJob.cs
- [ ] T062 [US2] Register recurring job in Program.cs to run every 30 seconds (per spec.md SC-044)
- [ ] T063 [P] [US2] Add error handling and retry logic for sync failures
- [ ] T064 [P] [US2] Log sync events to telemetry for monitoring

#### Real-time Webhook Handler (Optional Enhancement)

- [ ] T065 [P] [US2] Create GroupMembershipWebhookEndpoint in SuperDuperRescueHeads.Api/Endpoints/ for external system callbacks
- [ ] T066 [P] [US2] Validate webhook signatures for security
- [ ] T067 [P] [US2] Process membership changes in real-time when webhook received (faster than 30s polling)

#### Access Revocation Logic

- [ ] T068 [US2] Implement CheckForIndividualAccessAsync() before revoking group-based access per spec.md assumption 11
- [ ] T069 [P] [US2] Handle multi-group edge case: user removed from GroupA but still in GroupB (retain access) per spec.md acceptance scenario 3
- [ ] T070 [P] [US2] Update authorization handler to refresh permissions after membership changes

#### Testing & Validation

- [ ] T071 [US2] Test user added to group receives access within 30 seconds per spec.md acceptance scenario 1 and SC-044
- [ ] T072 [US2] Test user removed from group loses access within 30 seconds per spec.md acceptance scenario 2 and SC-045
- [ ] T073 [US2] Test multi-group membership (user in 2 groups, removed from 1, retains access) per spec.md acceptance scenario 3
- [ ] T074 [US2] Test individual + group access coexistence per spec.md acceptance scenario 4
- [ ] T075 [US2] Build and verify 0 errors, 0 warnings

---

## Phase 5: User Story 3 - Manage Group-Based Permissions

**Story Goal**: Collection owners can view, modify, and revoke group permissions independently, with clear visibility into group vs individual access.

**Priority**: P3

**Independent Test Criteria**:
1. Share collection with 2 groups and 2 individual users
2. View permission list - verify groups and individuals are distinguished
3. Change Group1 permission from ViewOnly to Edit
4. Verify all Group1 members now have Edit access
5. Revoke Group2 access
6. Verify Group2 members lose access (unless individual access exists)
7. Check access source for mixed-access user (shows both group + individual)

**Acceptance Criteria**:
- ✅ Permission list clearly distinguishes group vs individual shares
- ✅ Changing group permission updates all member access immediately
- ✅ Revoking group access handles individual access correctly
- ✅ Access source visible to users (group, individual, or both)

### Tasks

#### Permission Management Endpoints

- [ ] T076 [P] [US3] Add PATCH /api/v1/collections/{collectionId}/shares/group/{groupId}/permission endpoint
- [ ] T077 [P] [US3] Add DELETE /api/v1/collections/{collectionId}/shares/group/{groupId} endpoint to revoke group access
- [ ] T078 [P] [US3] Add GET /api/v1/collections/{collectionId}/access-sources endpoint showing individual vs group sources

#### Access Source Tracking

- [ ] T079 [P] [US3] Create AccessSource value object in SuperDuperRescueHeads.Domain/Sharing/AccessSource.cs (Individual, Group, Both)
- [ ] T080 [P] [US3] Implement GetUserAccessSourceAsync() in CollectionShareRepository
- [ ] T081 [US3] Update ShareResponse model to include AccessSource information

#### UI for Permission Management

- [ ] T082 [P] [US3] Update GroupShareList.razor to show Edit/Revoke actions for group shares
- [ ] T083 [P] [US3] Add visual badges showing "via Group X" vs "Direct" access in permission list
- [ ] T084 [US3] Create PermissionPrecedenceInfo.razor component explaining most-permissive-wins rule

#### Testing & Validation

- [ ] T085 [US3] Test permission list distinguishes groups vs individuals per spec.md acceptance scenario 1
- [ ] T086 [US3] Test changing group permission updates all members per spec.md acceptance scenarios 2
- [ ] T087 [US3] Test revoking group access with individual access fallback per spec.md acceptance scenario 3

---

## Phase 6: Polish & Cross-Cutting Concerns

**Goal**: Integration testing, performance optimization, and documentation.

### Tasks

- [ ] T088 [P] Performance test: Share collection with group of 100 members, verify <100ms permission resolution per plan.md
- [ ] T089 [P] Integration test: Full workflow across all 3 user stories with mixed individual/group access
- [ ] T090 [P] Add comprehensive logging for group sync events and permission resolution
- [ ] T091 Update API documentation with group sharing endpoints
- [ ] T092 Create migration guide: upgrading from Feature 006 to Feature 007
- [ ] T093 Final build and verification: 0 errors, 0 warnings

---

## Dependencies & Execution Order

### User Story Dependencies

```
Setup (Phase 1)
  ↓
Foundational (Phase 2) ← BLOCKING for all user stories
  ↓
US1: Share with Groups (Phase 3) ← MVP scope
  ↓
US2: Auto Access Updates (Phase 4) ← Depends on US1
  ↓
US3: Manage Permissions (Phase 5) ← Depends on US1
  ↓
Polish (Phase 6)
```

### Critical Path

1. **T001-T005**: Setup (review Feature 006)
2. **T006-T023**: Foundational (UserGroup entity, repositories, migration)
3. **T024-T051**: US1 implementation (basic group sharing)
4. **T052-T075**: US2 implementation (auto sync)
5. **T076-T087**: US3 implementation (advanced management)
6. **T088-T093**: Polish

### Parallel Execution Examples

**Phase 2 (Foundational) - Can parallelize**:
- Developer A: T006-T013 (Domain models)
- Developer B: T014-T018 (Infrastructure/migrations)
- Developer C: T019-T020 (Repositories)

**Phase 3 (US1) - Can parallelize**:
- Developer A: T024-T028 (Domain extensions)
- Developer B: T035-T039 (API endpoints)
- Developer C: T043-T046 (UI components)
- Developer D: T040-T042 (Authorization)

**Phase 4 (US2) - Can parallelize**:
- Developer A: T052-T056 (Sync service)
- Developer B: T057-T060 (Permission logic)
- Developer C: T061-T064 (Background jobs)
- Developer D: T065-T067 (Webhooks)

---

## Notes

- **External System Integration**: T052 requires configuration for external user management system API
- **Performance Critical**: T088 validates <100ms requirement from plan.md
- **Security**: T066 validates webhook signatures to prevent unauthorized access
- **Caching**: T042 implements 5-minute cache for group lookups to reduce database load
- **Audit Trail**: T056 logs all sync events for compliance and debugging
- **Migration Safety**: T034 ensures rollback capability for database changes

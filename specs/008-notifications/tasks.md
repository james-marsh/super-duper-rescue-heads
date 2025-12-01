# Tasks: Real-Time Notifications

**Input**: Design documents from `/specs/008-notifications/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, quickstart.md

**Tests**: Tests are NOT explicitly requested in the specification, so test tasks are omitted per template guidelines.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

This is a .NET Aspire project with the following structure:
- **Domain**: `SuperDuperRescueHeads.Domain/` - Domain models, interfaces
- **Infrastructure**: `SuperDuperRescueHeads.Infrastructure/` - Data access, external services
- **API**: `SuperDuperRescueHeads.Api/` - REST API endpoints, SignalR hubs
- **Web**: `SuperDuperRescueHeads.Web/` - Blazor Server UI components

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Review existing features and plan notification integration points

### Documentation & Analysis

- [ ] T001 Review Feature 006 (Basic Sharing) to identify sharing event integration points
- [ ] T002 Review Feature 007 (Group Sharing) to identify group membership event integration points
- [ ] T003 Review Feature 003 (Soft Delete) to identify deletion warning event integration points
- [ ] T004 Document all notification event types required per spec.md FR-091 through FR-095
- [ ] T005 Review SignalR setup requirements and authentication approach for real-time connections

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core notification infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Models

- [ ] T006 [P] Create NotificationType enum in SuperDuperRescueHeads.Domain/Notifications/NotificationType.cs (CollectionShared, AccessRevoked, PermissionChanged, ItemAdded, ItemModified, ItemDeleted, DeletionWarning, ConflictDetected, GroupAccessGranted, GroupAccessRevoked)
- [ ] T007 [P] Create NotificationPriority enum in SuperDuperRescueHeads.Domain/Notifications/NotificationPriority.cs (Low, Normal, High, Urgent)
- [ ] T008 Create Notification aggregate root in SuperDuperRescueHeads.Domain/Notifications/Notification.cs
- [ ] T009 Add Notification factory methods: CreateSharingNotification(), CreateEditNotification(), CreateSystemNotification()
- [ ] T010 Add MarkAsRead(), MarkAsUnread(), Dismiss() methods to Notification entity
- [ ] T011 [P] Create INotificationRepository interface in SuperDuperRescueHeads.Domain/Notifications/INotificationRepository.cs

### Infrastructure - Database

- [ ] T012 Create NotificationConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/NotificationConfiguration.cs
- [ ] T013 Add DbSet<Notification> to ApplicationDbContext in SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T014 Create migration AddNotifications with Notifications table, indexes on UserId, CreatedAt, IsRead
- [ ] T015 Implement NotificationRepository in SuperDuperRescueHeads.Infrastructure/Repositories/NotificationRepository.cs

### Infrastructure - SignalR Setup

- [ ] T016 [P] Add SignalR NuGet package to SuperDuperRescueHeads.Api project
- [ ] T017 [P] Configure SignalR services in Program.cs (AddSignalR with Azure SignalR Service configuration if needed)
- [ ] T018 Create base NotificationHub in SuperDuperRescueHeads.Api/Hubs/NotificationHub.cs with OnConnectedAsync/OnDisconnectedAsync

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Receive Real-Time Notifications (Priority: P1) 🎯 MVP

**Story Goal**: Users receive immediate in-app notifications when important events occur, such as collections being shared with them or collaborators making changes.

**Priority**: P1 - Essential for collaborative awareness

**Independent Test Criteria**:
1. Share a collection with user → notification appears within 1 second
2. Edit item in shared collection → other users receive notification within 1 second
3. Delete item in shared collection → other users receive notification within 1 second
4. User makes own changes → does NOT receive notification for own actions
5. Item approaching 30-day deletion → user receives warning 3 days before

**Acceptance Criteria**:
- ✅ In-app notifications delivered within 1 second (FR-038, SC-016)
- ✅ Users do NOT receive notifications for their own actions (FR-041)
- ✅ Notifications for sharing, edits, and system warnings all work (FR-091, FR-092, FR-093)

### Tasks

#### SignalR Hub Implementation

- [ ] T019 [US1] Implement SendNotificationAsync method in NotificationHub to send notifications to specific user
- [ ] T020 [US1] Implement SendNotificationToGroupAsync method in NotificationHub for multi-device scenarios
- [ ] T021 [US1] Add connection tracking: Store userId to connectionId mapping on connect
- [ ] T022 [US1] Add connection cleanup: Remove mapping on disconnect
- [ ] T023 [US1] Add authentication to NotificationHub using [Authorize] attribute

#### Notification Service

- [ ] T024 [P] [US1] Create INotificationService interface in SuperDuperRescueHeads.Domain/Notifications/INotificationService.cs
- [ ] T025 [US1] Implement NotificationService in SuperDuperRescueHeads.Infrastructure/Services/NotificationService.cs
- [ ] T026 [US1] Add CreateAndSendNotificationAsync method that saves to DB and sends via SignalR
- [ ] T027 [US1] Add logic to filter out notifications for actions performed by the current user (FR-041)
- [ ] T028 [US1] Add GetActiveConnectionsForUserAsync to handle multi-device scenarios
- [ ] T029 [US1] Register INotificationService in Program.cs

#### Event Handlers for Sharing (Feature 006/007 Integration)

- [ ] T030 [P] [US1] Create CollectionSharedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/CollectionSharedEventHandler.cs
- [ ] T031 [P] [US1] Create AccessRevokedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/AccessRevokedEventHandler.cs
- [ ] T032 [P] [US1] Create PermissionChangedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/PermissionChangedEventHandler.cs
- [ ] T033 [P] [US1] Create GroupAccessGrantedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/GroupAccessGrantedEventHandler.cs
- [ ] T034 [US1] Integrate event handlers with CollectionSharingEndpoints to trigger notifications on share actions

#### Event Handlers for Collaborative Edits (Feature 002 Integration)

- [ ] T035 [P] [US1] Create ItemAddedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/ItemAddedEventHandler.cs
- [ ] T036 [P] [US1] Create ItemModifiedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/ItemModifiedEventHandler.cs
- [ ] T037 [P] [US1] Create ItemDeletedEventHandler in SuperDuperRescueHeads.Infrastructure/EventHandlers/ItemDeletedEventHandler.cs
- [ ] T038 [US1] Modify ItemsEndpoints to trigger domain events for add/edit/delete operations

#### Deletion Warning Notifications (Feature 003 Integration)

- [ ] T039 [US1] Create DeletionWarningJob in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/DeletionWarningJob.cs
- [ ] T040 [US1] Add logic to find items 3 days from permanent deletion (27 days since soft delete)
- [ ] T041 [US1] Create notifications for users with items approaching permanent deletion
- [ ] T042 [US1] Register DeletionWarningJob with Hangfire to run daily in Program.cs

#### API Endpoints

- [ ] T043 [US1] Create NotificationEndpoints.cs in SuperDuperRescueHeads.Api/Endpoints/ with GET /api/v1/notifications/unread
- [ ] T044 [US1] Add MapNotificationEndpoints() call in Program.cs
- [ ] T045 [US1] Map SignalR hub endpoint with app.MapHub<NotificationHub>("/hubs/notifications") in Program.cs

#### UI Components (Blazor)

- [ ] T046 [P] [US1] Create NotificationBell.razor component in SuperDuperRescueHeads.Web/Components/Notifications/
- [ ] T047 [P] [US1] Add SignalR client connection in NotificationBell.razor using Microsoft.AspNetCore.SignalR.Client
- [ ] T048 [US1] Implement real-time notification reception and display in NotificationBell.razor
- [ ] T049 [US1] Add unread count badge to notification bell icon
- [ ] T050 [US1] Add NotificationBell component to MainLayout.razor

**Checkpoint**: At this point, User Story 1 should be fully functional - users receive real-time notifications for all event types

---

## Phase 4: User Story 2 - View Notification History (Priority: P2)

**Story Goal**: Users can review past notifications to catch up on events they may have missed or see historical activity.

**Priority**: P2 - Important for offline users and historical context

**Independent Test Criteria**:
1. Generate 10 notifications → all 10 visible in notification panel
2. Click on notification → navigates to relevant collection/item
3. Mix of read and unread notifications → unread visually distinguished
4. Mark notification as read → moves to read section
5. Generate 100+ notifications → pagination loads older notifications

**Acceptance Criteria**:
- ✅ Notification history displays recent notifications with timestamps (SC-037)
- ✅ Notifications link to relevant collections/items
- ✅ Read/unread visual distinction (FR-096)
- ✅ Pagination for 50+ notifications (FR-100)

### Tasks

#### Repository Extensions

- [ ] T051 [P] [US2] Add GetNotificationsForUserAsync method to NotificationRepository with pagination (skip, take)
- [ ] T052 [P] [US2] Add GetUnreadCountForUserAsync method to NotificationRepository
- [ ] T053 [P] [US2] Add MarkAsReadAsync method to NotificationRepository
- [ ] T054 [P] [US2] Add MarkAllAsReadAsync method to NotificationRepository for batch operations (FR-098)

#### API Endpoints

- [ ] T055 [P] [US2] Add GET /api/v1/notifications endpoint to NotificationEndpoints.cs with pagination support
- [ ] T056 [P] [US2] Add PATCH /api/v1/notifications/{id}/read endpoint to mark single notification as read
- [ ] T057 [P] [US2] Add POST /api/v1/notifications/mark-all-read endpoint to mark all as read (FR-098)
- [ ] T058 [P] [US2] Add DELETE /api/v1/notifications/{id} endpoint to dismiss/clear individual notifications (FR-097)

#### API Models

- [ ] T059 [P] [US2] Create NotificationResponse model in SuperDuperRescueHeads.Api/Models/NotificationResponse.cs
- [ ] T060 [P] [US2] Create NotificationHistoryResponse model in SuperDuperRescueHeads.Api/Models/NotificationHistoryResponse.cs with pagination metadata

#### UI Components (Blazor)

- [ ] T061 [P] [US2] Create NotificationPanel.razor component in SuperDuperRescueHeads.Web/Components/Notifications/
- [ ] T062 [US2] Implement notification history list with virtualized scrolling for performance
- [ ] T063 [US2] Add visual distinction for read vs unread notifications (bold text, different background)
- [ ] T064 [US2] Implement click-to-navigate functionality for each notification
- [ ] T065 [US2] Add "Mark as read" button for individual notifications
- [ ] T066 [US2] Add "Mark all as read" button at top of panel
- [ ] T067 [US2] Implement infinite scroll pagination for loading older notifications
- [ ] T068 [US2] Update NotificationBell.razor to toggle NotificationPanel visibility

**Checkpoint**: At this point, User Story 2 should be fully functional - users can view and manage notification history

---

## Phase 5: User Story 3 - Manage Notification Preferences (Priority: P2)

**Story Goal**: Users can control which types of notifications they receive to avoid notification overload while ensuring important updates aren't missed.

**Priority**: P2 - Important for user control and preventing notification fatigue

**Independent Test Criteria**:
1. View preferences → see toggles for each notification type
2. Disable "collaborative edit" notifications → no notifications when others edit
3. Disable "sharing" notifications → no notifications when collections shared (still get access)
4. Mute specific collection → no notifications for that collection only
5. Change preferences → take effect immediately (within 1 second)
6. Enable "Do Not Disturb" → notifications queued but not displayed until disabled

**Acceptance Criteria**:
- ✅ Preferences for each notification type (FR-040, FR-101)
- ✅ Disabled notification types receive zero notifications (SC-051)
- ✅ Changes take effect immediately (FR-103, SC-017)
- ✅ Do Not Disturb mode queues notifications (FR-102)

### Tasks

#### Domain Models

- [ ] T069 [P] [US3] Create NotificationPreference entity in SuperDuperRescueHeads.Domain/Notifications/NotificationPreference.cs
- [ ] T070 [US3] Add properties: EnableSharingNotifications, EnableEditNotifications, EnableSystemNotifications, DoNotDisturbEnabled
- [ ] T071 [US3] Add MutedCollectionIds collection to NotificationPreference
- [ ] T072 [US3] Add factory method CreateDefaultPreferences() with all notifications enabled by default
- [ ] T073 [P] [US3] Create INotificationPreferenceRepository interface in SuperDuperRescueHeads.Domain/Notifications/INotificationPreferenceRepository.cs

#### Infrastructure - Database

- [ ] T074 [US3] Create NotificationPreferenceConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/NotificationPreferenceConfiguration.cs
- [ ] T075 [US3] Add DbSet<NotificationPreference> to ApplicationDbContext
- [ ] T076 [US3] Create migration AddNotificationPreferences with NotificationPreferences table
- [ ] T077 [US3] Implement NotificationPreferenceRepository in SuperDuperRescueHeads.Infrastructure/Repositories/NotificationPreferenceRepository.cs

#### Service Logic

- [ ] T078 [US3] Add GetOrCreatePreferencesAsync method to NotificationPreferenceRepository
- [ ] T079 [US3] Update NotificationService.CreateAndSendNotificationAsync to check preferences before sending
- [ ] T080 [US3] Add ShouldSendNotificationAsync method to check: notification type enabled, collection not muted, DND mode
- [ ] T081 [US3] Implement notification queuing logic for Do Not Disturb mode
- [ ] T082 [US3] Create NotificationQueue entity in SuperDuperRescueHeads.Domain/Notifications/NotificationQueue.cs
- [ ] T083 [US3] Add FlushQueuedNotificationsAsync method to deliver queued notifications when DND disabled

#### API Endpoints

- [ ] T084 [P] [US3] Add GET /api/v1/notifications/preferences endpoint to NotificationEndpoints.cs
- [ ] T085 [P] [US3] Add PUT /api/v1/notifications/preferences endpoint to update preferences
- [ ] T086 [P] [US3] Add POST /api/v1/notifications/preferences/mute/{collectionId} endpoint to mute collection
- [ ] T087 [P] [US3] Add DELETE /api/v1/notifications/preferences/mute/{collectionId} endpoint to unmute collection

#### API Models

- [ ] T088 [P] [US3] Create NotificationPreferenceResponse model in SuperDuperRescueHeads.Api/Models/NotificationPreferenceResponse.cs
- [ ] T089 [P] [US3] Create UpdateNotificationPreferencesRequest model in SuperDuperRescueHeads.Api/Models/UpdateNotificationPreferencesRequest.cs

#### UI Components (Blazor)

- [ ] T090 [P] [US3] Create NotificationPreferences.razor component in SuperDuperRescueHeads.Web/Components/Notifications/
- [ ] T091 [US3] Add toggles for each notification type (sharing, edits, system events)
- [ ] T092 [US3] Add Do Not Disturb toggle with visual indicator
- [ ] T093 [US3] Add section showing muted collections with unmute buttons
- [ ] T094 [US3] Add "Mute this collection" button in collection view
- [ ] T095 [US3] Implement real-time preference updates (changes take effect immediately)
- [ ] T096 [US3] Add NotificationPreferences link to user settings menu

**Checkpoint**: At this point, User Story 3 should be fully functional - users can manage notification preferences

---

## Phase 6: User Story 4 - Multi-Device Notification Sync (Priority: P3)

**Story Goal**: Users logged in on multiple devices want notifications to sync across all devices appropriately.

**Priority**: P3 - Nice to have for power users with multiple devices

**Independent Test Criteria**:
1. Log in on 2 devices → notification appears on both when generated
2. Mark as read on device 1 → shows as read on device 2 within 2 seconds
3. Change preferences on device 1 → applied on device 2 immediately
4. Dismiss notification on device 1 → removed from device 2

**Acceptance Criteria**:
- ✅ Notifications appear on all active devices (FR-105)
- ✅ Read status syncs within 2 seconds (FR-106, SC-049)
- ✅ Preferences sync across devices (FR-104)
- ✅ No duplicate notifications on same device (FR-107)

### Tasks

#### Connection Tracking

- [ ] T097 [P] [US4] Create NotificationConnection entity in SuperDuperRescueHeads.Domain/Notifications/NotificationConnection.cs
- [ ] T098 [US4] Add properties: UserId, ConnectionId, ConnectedAt, LastHeartbeat, DeviceInfo
- [ ] T099 [US4] Update NotificationHub.OnConnectedAsync to store connection in database
- [ ] T100 [US4] Update NotificationHub.OnDisconnectedAsync to remove connection from database
- [ ] T101 [US4] Add heartbeat mechanism: ReceiveHeartbeatAsync method in NotificationHub

#### Multi-Device Delivery

- [ ] T102 [US4] Update NotificationService to get all active connections for user from NotificationConnection
- [ ] T103 [US4] Send notification to all user connections via SignalR Groups
- [ ] T104 [US4] Implement deduplication logic to prevent same notification appearing twice on one device

#### Real-Time Sync

- [ ] T105 [US4] Add NotificationReadSync SignalR method to broadcast read status changes
- [ ] T106 [US4] When marking notification as read, broadcast to all user connections
- [ ] T107 [US4] Add client-side handler in NotificationPanel.razor to update UI when read status changes
- [ ] T108 [US4] Add PreferenceChangedSync SignalR method to broadcast preference changes
- [ ] T109 [US4] When preferences updated, broadcast to all user connections
- [ ] T110 [US4] Add client-side handler in NotificationPreferences.razor to refresh when changes detected

#### Connection Resilience

- [ ] T111 [US4] Implement automatic reconnection logic in SignalR client with exponential backoff
- [ ] T112 [US4] Add connection state indicator in NotificationBell.razor (connected, disconnected, reconnecting)
- [ ] T113 [US4] Implement queued notification delivery: when reconnecting, fetch missed notifications
- [ ] T114 [US4] Add GetNotificationsSinceAsync method to NotificationRepository (timestamp parameter)

**Checkpoint**: At this point, User Story 4 should be fully functional - notifications sync across all user devices

---

## Phase 7: Polish & Cross-Cutting Concerns

**Goal**: Performance optimization, comprehensive testing, and production readiness

### Tasks

#### Performance Optimization

- [ ] T115 [P] Performance test: 1,000 concurrent SignalR connections (verify SC-038)
- [ ] T116 [P] Performance test: Notification delivery latency <1 second (verify SC-016)
- [ ] T117 [P] Optimize notification queries with indexes on (UserId, IsRead, CreatedAt)
- [ ] T118 [P] Implement notification retention policy: archive notifications older than 30 days (FR-099)

#### Background Jobs

- [ ] T119 Create CleanupOldNotificationsJob in SuperDuperRescueHeads.Infrastructure/BackgroundJobs/CleanupOldNotificationsJob.cs
- [ ] T120 Schedule CleanupOldNotificationsJob to run daily at 3 AM via Hangfire
- [ ] T121 Create CleanupStaleConnectionsJob to remove connections with no heartbeat for 10+ minutes
- [ ] T122 Schedule CleanupStaleConnectionsJob to run every 5 minutes via Hangfire

#### Integration Testing

- [ ] T123 [P] Integration test: End-to-end notification flow (share collection → receive notification → mark as read)
- [ ] T124 [P] Integration test: Multi-device sync (mark as read on device 1 → syncs to device 2)
- [ ] T125 [P] Integration test: Notification preferences filtering (disable type → no notifications of that type)
- [ ] T126 [P] Integration test: Do Not Disturb mode (notifications queued then delivered when disabled)

#### Documentation

- [ ] T127 Update API documentation with notification endpoints
- [ ] T128 Add SignalR connection documentation for frontend developers
- [ ] T129 Document notification event types and when they're triggered
- [ ] T130 Create notification testing guide for QA

#### Final Verification

- [ ] T131 Final build and verification: 0 errors, 0 warnings
- [ ] T132 Verify all functional requirements (FR-037 through FR-107) are met
- [ ] T133 Verify all success criteria (SC-016, SC-017, SC-037, SC-038, SC-049, SC-050, SC-051) are met

---

## Dependencies & Execution Order

### User Story Dependencies

```
Setup (Phase 1)
  ↓
Foundational (Phase 2) ← BLOCKING for all user stories
  ↓
US1: Receive Real-Time Notifications (Phase 3) ← MVP scope, foundation for other stories
  ↓
├─ US2: View Notification History (Phase 4) ← Depends on US1
├─ US3: Manage Notification Preferences (Phase 5) ← Depends on US1
└─ US4: Multi-Device Sync (Phase 6) ← Depends on US1, US2
  ↓
Polish (Phase 7)
```

### Critical Path

1. **T001-T005**: Setup (review integration points)
2. **T006-T018**: Foundational (domain models, SignalR setup, database)
3. **T019-T050**: US1 implementation (real-time notification delivery)
4. **T051-T068**: US2 implementation (notification history)
5. **T069-T096**: US3 implementation (notification preferences)
6. **T097-T114**: US4 implementation (multi-device sync)
7. **T115-T133**: Polish

### Parallel Execution Examples

**Phase 2 (Foundational) - Can parallelize**:
- Developer A: T006-T011 (Domain models and interfaces)
- Developer B: T012-T015 (Database configuration and migration)
- Developer C: T016-T018 (SignalR setup)

**Phase 3 (US1) - Can parallelize**:
- Developer A: T019-T023 (SignalR hub)
- Developer B: T024-T029 (Notification service)
- Developer C: T030-T034 (Sharing event handlers)
- Developer D: T035-T038 (Edit event handlers)
- Developer E: T039-T042 (Deletion warnings)
- Developer F: T043-T045 (API endpoints)
- Developer G: T046-T050 (UI components)

**Phase 4 (US2) - Can parallelize**:
- Developer A: T051-T054 (Repository extensions)
- Developer B: T055-T058 (API endpoints)
- Developer C: T059-T060 (API models)
- Developer D: T061-T068 (UI components)

**Phase 5 (US3) - Can parallelize**:
- Developer A: T069-T073 (Domain models)
- Developer B: T074-T077 (Database layer)
- Developer C: T084-T087 (API endpoints)
- Developer D: T088-T089 (API models)
- Developer E: T090-T096 (UI components)

---

## Implementation Strategy

### MVP Scope (Recommended)

**Phase 1 + Phase 2 + Phase 3 (US1)** = Minimal viable product

This provides:
- ✅ Real-time notification delivery
- ✅ Basic notification types (sharing, edits, system warnings)
- ✅ SignalR infrastructure
- ✅ Notification bell UI component

**Remaining user stories can be added incrementally:**
- Phase 4 (US2): Add notification history viewing
- Phase 5 (US3): Add preference management
- Phase 6 (US4): Add multi-device sync

### Testing Strategy

Tests are **NOT** explicitly requested in spec.md, so test tasks are omitted per template guidelines. However, integration testing is included in Phase 7 (Polish) for production readiness.

Each user story has **Independent Test Criteria** that can be verified manually or via integration tests after implementation.

---

## Notes

- **SignalR Configuration**: T017 requires Azure SignalR Service configuration for production scale (1,000+ concurrent connections per SC-038)
- **Authentication**: T023 requires SignalR hub authentication to ensure notifications only sent to authorized users
- **Performance Critical**: T115-T117 validate <1s notification delivery and 1,000+ concurrent connection requirements
- **Retention Policy**: T118 implements 30-day retention per FR-099
- **Event Integration**: T030-T038 require modifying existing feature endpoints to trigger notification events
- **Deduplication**: T104 prevents duplicate notifications when user has multiple browser tabs open
- **Offline Support**: T113 ensures users receive missed notifications when reconnecting (up to 100 queued per Assumption 13)

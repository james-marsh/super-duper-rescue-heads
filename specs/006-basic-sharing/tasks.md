# Implementation Tasks: Basic Sharing

**Feature**: 006-basic-sharing
**Generated**: 2025-11-28
**Total Tasks**: 95 (estimated)
**MVP Scope**: Phase 1-3 (Invitation System + Permission Levels) = 42 tasks

## Overview

Enable users to share collections with collaborators via email invitation. Supports View Only and Can Edit permissions. Includes invitation management, real-time sync, and audit logging.

**Technology Stack**: C# 14, .NET 10, EF Core 9.0, SendGrid (email), SignalR (real-time), Policy-based authorization
**Testing**: TUnit (unit), xUnit (integration), bUnit (Blazor), Playwright (E2E)

---

## Phase 1: Setup & Domain Model (10 tasks)

**Goal**: Initialize sharing infrastructure and domain entities

- [ ] T001 Create SuperDuperRescueHeads.Domain/Sharing directory
- [ ] T002 Create SharePermission enum (ViewOnly, CanEdit) in SuperDuperRescueHeads.Domain/Sharing/SharePermission.cs
- [ ] T003 Create ShareStatus enum (Pending, Accepted, Declined, Expired, Revoked) in SuperDuperRescueHeads.Domain/Sharing/ShareStatus.cs
- [ ] T004 Create CollectionShare aggregate root in SuperDuperRescueHeads.Domain/Sharing/CollectionShare.cs
- [ ] T005 Create ShareInvitation value object in SuperDuperRescueHeads.Domain/Sharing/ShareInvitation.cs
- [ ] T006 Create ShareAuditLog aggregate in SuperDuperRescueHeads.Domain/Sharing/ShareAuditLog.cs
- [ ] T007 Create ICollectionShareRepository interface in SuperDuperRescueHeads.Domain/Sharing/ICollectionShareRepository.cs
- [ ] T008 Create sharing domain events in SuperDuperRescueHeads.Domain/Sharing/SharingEvents.cs
- [ ] T009 Install SendGrid NuGet package in SuperDuperRescueHeads.Infrastructure
- [ ] T010 Install Microsoft.AspNetCore.SignalR NuGet package in SuperDuperRescueHeads.Api

---

## Phase 2: Invitation System (FR1) (32 tasks)

**Goal**: Send, accept, decline, and manage invitations

**Independent Test Criteria**:
- ✅ User can invite collaborator by email
- ✅ Invitation email sent within 1 minute
- ✅ Accept/Decline links are single-use, expire in 7 days
- ✅ Max 10 collaborators per collection enforced
- ✅ Duplicate invitations prevented

### Domain Logic

- [ ] T011 [P] [FR1] Implement CollectionShare.CreateInvitation() in SuperDuperRescueHeads.Domain/Sharing/CollectionShare.cs
- [ ] T012 [P] [FR1] Implement CollectionShare.Accept() in CollectionShare.cs
- [ ] T013 [P] [FR1] Implement CollectionShare.Decline() in CollectionShare.cs
- [ ] T014 [P] [FR1] Implement CollectionShare.Expire() in CollectionShare.cs
- [ ] T015 [P] [FR1] Implement CollectionShare.Resend() in CollectionShare.cs
- [ ] T016 [FR1] Validate max 10 collaborators in CollectionShare.CreateInvitation()
- [ ] T017 [FR1] Generate secure invitation token (128-bit) in ShareInvitation
- [ ] T018 [FR1] Calculate expiration (7 days from creation) in ShareInvitation

### Unit Tests - Domain Logic

- [ ] T019 [P] [FR1] Test CreateInvitation() validates max 10 collaborators in SuperDuperRescueHeads.Tests.Unit/Sharing/CollectionShareTests.cs
- [ ] T020 [P] [FR1] Test CreateInvitation() prevents self-invitation in CollectionShareTests
- [ ] T021 [P] [FR1] Test CreateInvitation() prevents duplicate invitations in CollectionShareTests
- [ ] T022 [P] [FR1] Test Accept() changes status to Accepted in CollectionShareTests
- [ ] T023 [P] [FR1] Test Decline() changes status to Declined in CollectionShareTests
- [ ] T024 [P] [FR1] Test Expire() marks invitation as Expired in CollectionShareTests

### Infrastructure - Repository

- [ ] T025 [FR1] Create CollectionShareConfiguration EF Core config in SuperDuperRescueHeads.Infrastructure/Data/Configurations/CollectionShareConfiguration.cs
- [ ] T026 [FR1] Add CollectionShares DbSet to ApplicationDbContext
- [ ] T027 [FR1] Create migration AddCollectionShares in SuperDuperRescueHeads.Infrastructure/Migrations/
- [ ] T028 [FR1] Implement CollectionShareRepository in SuperDuperRescueHeads.Infrastructure/Data/Repositories/CollectionShareRepository.cs
- [ ] T029 [FR1] Implement GetPendingInvitationsByCollectionId() in CollectionShareRepository
- [ ] T030 [FR1] Implement GetActiveSharesByUserId() in CollectionShareRepository

### Infrastructure - Email Service

- [ ] T031 [P] [FR1] Create IEmailService interface in SuperDuperRescueHeads.Infrastructure/Email/IEmailService.cs
- [ ] T032 [FR1] Implement SendGridEmailService in SuperDuperRescueHeads.Infrastructure/Email/SendGridEmailService.cs
- [ ] T033 [FR1] Create invitation email template with Accept/Decline links
- [ ] T034 [FR1] Implement SendInvitationEmailAsync() in SendGridEmailService

### API - Invitation Endpoints

- [ ] T035 [FR1] Create CollectionSharingEndpoints in SuperDuperRescueHeads.Api/Endpoints/CollectionSharingEndpoints.cs
- [ ] T036 [FR1] Implement POST /api/v1/collections/{id}/share (create invitation)
- [ ] T037 [FR1] Implement GET /api/v1/invitations/{token} (view invitation details)
- [ ] T038 [FR1] Implement POST /api/v1/invitations/{token}/accept (accept invitation)
- [ ] T039 [FR1] Implement POST /api/v1/invitations/{token}/decline (decline invitation)
- [ ] T040 [FR1] Implement POST /api/v1/collections/{id}/invitations/{invitationId}/resend
- [ ] T041 [FR1] Implement DELETE /api/v1/collections/{id}/invitations/{invitationId} (cancel)
- [ ] T042 [FR1] Register services in Program.cs (CollectionShareRepository, EmailService)

---

## Phase 3: Permission Levels (FR2) (15 tasks)

**Goal**: Enforce View Only and Can Edit permissions

**Independent Test Criteria**:
- ✅ View Only users cannot modify items
- ✅ Can Edit users can add/edit/delete items but not collection
- ✅ Permission changes take effect immediately
- ✅ UI shows permission level clearly

**Dependencies**: Requires Phase 2 (invitations must exist)

### Infrastructure - Authorization Policies

- [ ] T043 [P] [FR2] Create CollectionPermissionRequirement in SuperDuperRescueHeads.Infrastructure/Authorization/CollectionPermissionRequirement.cs
- [ ] T044 [P] [FR2] Create CollectionPermissionHandler in SuperDuperRescueHeads.Infrastructure/Authorization/CollectionPermissionHandler.cs
- [ ] T045 [FR2] Register authorization policies in Program.cs (CanViewCollection, CanEditCollection)

### API - Permission Enforcement

- [ ] T046 [FR2] Add [Authorize(Policy = "CanViewCollection")] to GET collection endpoints
- [ ] T047 [FR2] Add [Authorize(Policy = "CanEditCollection")] to POST/PUT/DELETE item endpoints
- [ ] T048 [FR2] Implement permission check caching (30-second in-memory cache)

### API - Permission Management

- [ ] T049 [FR2] Implement PUT /api/v1/collections/{id}/collaborators/{userId}/permission (change permission)
- [ ] T050 [FR2] Implement DELETE /api/v1/collections/{id}/collaborators/{userId} (revoke access)
- [ ] T051 [FR2] Implement POST /api/v1/collections/{id}/leave (collaborator leaves)

### Unit Tests - Authorization

- [ ] T052 [P] [FR2] Test CollectionPermissionHandler grants access for View Only in SuperDuperRescueHeads.Tests.Unit/Authorization/CollectionPermissionHandlerTests.cs
- [ ] T053 [P] [FR2] Test CollectionPermissionHandler grants access for Can Edit in CollectionPermissionHandlerTests
- [ ] T054 [P] [FR2] Test CollectionPermissionHandler denies access for revoked share in CollectionPermissionHandlerTests
- [ ] T055 [P] [FR2] Test permission change takes effect immediately (cache invalidation) in CollectionPermissionHandlerTests

### Integration Tests - End-to-End Permissions

- [ ] T056 [P] [FR2] Test View Only user cannot edit items in SuperDuperRescueHeads.Tests.Integration/Sharing/PermissionTests.cs
- [ ] T057 [P] [FR2] Test Can Edit user can add/edit/delete items in PermissionTests

---

## Phase 4: Shared Collection Views (FR3) (12 tasks)

**Goal**: Display shared collections in collaborator's collection list

**Independent Test Criteria**:
- ✅ Shared collections appear with "Shared by [Owner]" badge
- ✅ Permission level visible ("View Only" / "Can Edit")
- ✅ Filter: "My Collections" / "Shared With Me" / "All"
- ✅ Owner's edits reflect within 5 seconds

**Dependencies**: Requires Phase 2 + 3

### API - Shared Collection Endpoints

- [ ] T058 [FR3] Implement GET /api/v1/collections/shared (list shared collections for current user)
- [ ] T059 [FR3] Add owner information to collection DTOs (ownerName, isShared, sharedBy, permission)
- [ ] T060 [FR3] Implement filter query parameter (?filter=my|shared|all)

### Blazor UI - Shared Collections

- [ ] T061 [P] [FR3] Update Collections/Index.razor to show shared collections
- [ ] T062 [FR3] Add "Shared by [Owner]" badge to collection cards
- [ ] T063 [FR3] Add permission indicator ("View Only" / "Can Edit") to collection cards
- [ ] T064 [FR3] Implement filter dropdown (My Collections / Shared With Me / All)
- [ ] T065 [FR3] Disable Edit/Delete buttons for View Only shared collections

### Real-Time Sync (SignalR)

- [ ] T066 [P] [FR3] Create CollectionHub SignalR hub in SuperDuperRescueHeads.Api/Hubs/CollectionHub.cs
- [ ] T067 [FR3] Implement NotifyCollaboratorsAsync() in CollectionHub (item added/edited/deleted)
- [ ] T068 [FR3] Register SignalR in Program.cs
- [ ] T069 [FR3] Connect Blazor client to CollectionHub for real-time updates

---

## Phase 5: Collaborator Management (FR4) (14 tasks)

**Goal**: Manage collaborators, change permissions, audit log

**Independent Test Criteria**:
- ✅ Owner sees list of collaborators with permissions
- ✅ Owner can change permissions and remove collaborators
- ✅ Audit log tracks all sharing actions
- ✅ User attribution for item edits

**Dependencies**: Requires Phase 2 + 3

### API - Collaborator Management

- [ ] T070 [FR4] Implement GET /api/v1/collections/{id}/collaborators (list collaborators)
- [ ] T071 [FR4] Add lastAccessedAt tracking to CollectionShare

### Audit Log

- [ ] T072 [P] [FR4] Create ShareAuditLogConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/ShareAuditLogConfiguration.cs
- [ ] T073 [P] [FR4] Create ShareAuditLogRepository in SuperDuperRescueHeads.Infrastructure/Data/Repositories/ShareAuditLogRepository.cs
- [ ] T074 [FR4] Implement LogInvitation() in ShareAuditLogRepository
- [ ] T075 [FR4] Implement LogAcceptance() in ShareAuditLogRepository
- [ ] T076 [FR4] Implement LogPermissionChange() in ShareAuditLogRepository
- [ ] T077 [FR4] Implement LogRevocation() in ShareAuditLogRepository
- [ ] T078 [FR4] Add user attribution to item add/edit/delete events

### Blazor UI - Collaborators Page

- [ ] T079 [P] [FR4] Create Collections/Collaborators.razor page
- [ ] T080 [FR4] Display collaborator list with name, email, permission, date added
- [ ] T081 [FR4] Implement permission change dropdown (View Only ↔ Can Edit)
- [ ] T082 [FR4] Implement Remove button with confirmation modal
- [ ] T083 [FR4] Display audit log for collection (optional, admin view)

---

## Phase 6: Notifications (FR5) (10 tasks)

**Goal**: Email and in-app notifications for sharing events

**Independent Test Criteria**:
- ✅ Email sent for invitation, acceptance, revocation, permission change
- ✅ In-app notifications for collaborator events
- ✅ Notification preferences respected

**Dependencies**: Requires Phase 2

### Email Notifications

- [ ] T084 [P] [FR5] Send invitation email in SendGridEmailService
- [ ] T085 [P] [FR5] Send acceptance notification email (to owner)
- [ ] T086 [P] [FR5] Send revocation notification email (to collaborator)
- [ ] T087 [P] [FR5] Send permission change notification email

### In-App Notifications

- [ ] T088 [P] [FR5] Create NotificationService in SuperDuperRescueHeads.Infrastructure/Notifications/NotificationService.cs
- [ ] T089 [FR5] Implement CreateNotification() for sharing events
- [ ] T090 [FR5] Integrate notifications with SignalR for real-time delivery

### Notification Preferences

- [ ] T091 [P] [FR5] Create NotificationPreferences entity
- [ ] T092 [FR5] Implement notification preference settings UI
- [ ] T093 [FR5] Respect notification preferences (email: always/daily/never, in-app: instant/hourly/never)

---

## Phase 7: Polish & Testing (2 tasks)

**Goal**: E2E tests, performance optimization, documentation

- [ ] T094 Write E2E test: Complete sharing flow (invite → accept → collaborate → revoke) using Playwright
- [ ] T095 Add Application Insights telemetry for sharing adoption, invitation acceptance rate, permission distribution

---

## Dependencies

### Functional Requirement Completion Order

```
Phase 1 (Setup) → Phase 2 (FR1: Invitations)
                        ↓
                  Phase 3 (FR2: Permissions)
                        ↓
            ┌───────────┴───────────┐
            ↓                       ↓
    Phase 4 (FR3: Views)    Phase 5 (FR4: Management)
            ↓                       ↓
            └───────────┬───────────┘
                        ↓
            Phase 6 (FR5: Notifications)
                        ↓
                 Phase 7 (Polish)
```

**Critical Path**: Phase 1 → 2 → 3 → 4 → 7
**Parallel Opportunities**:
- Phase 5 (Collaborator Management) can start after Phase 3
- Phase 6 (Notifications) can start after Phase 2

---

## Parallel Execution Examples

### Phase 2 (FR1) - 12 parallel tasks:
- Domain logic tasks (T011-T018) can run in parallel
- Unit tests (T019-T024) can run in parallel after domain logic
- Email template creation (T033) can run independently

### Phase 3 (FR2) - 6 parallel tasks:
- Authorization policies (T043-T044) can run in parallel
- Unit tests (T052-T055) can run in parallel after policies
- Integration tests (T056-T057) can run in parallel after API endpoints

### Phase 4 (FR3) - 4 parallel tasks:
- Blazor UI tasks (T061-T065) can partially overlap
- SignalR hub (T066) can be built independently

### Phase 5 (FR4) - 4 parallel tasks:
- Audit log tasks (T072-T073) can run in parallel
- Blazor UI (T079) can be built independently

### Phase 6 (FR5) - 7 parallel tasks:
- All email notification tasks (T084-T087) can run in parallel
- In-app notification tasks (T088-T090) can run in parallel

---

## Implementation Strategy

### MVP Scope (Phases 1-3): FR1 + FR2

**Deliverables** (42 tasks):
- Complete invitation system (send, accept, decline, resend, cancel)
- Email integration with SendGrid
- Permission enforcement (View Only, Can Edit)
- Policy-based authorization
- Max 10 collaborators enforced

**Outcome**: Users can share collections and collaborators can view/edit based on permissions.

**Timeline**: ~2 weeks

### Full Feature (All Phases)

**Deliverables** (95 tasks):
- All functional requirements implemented
- Shared collection views with real-time sync
- Collaborator management with audit log
- Email + in-app notifications
- Production-ready with telemetry

**Timeline**: ~4 weeks

### Incremental Delivery Plan

1. **Week 1**: MVP (Phases 1-2) - Invitation system
2. **Week 2**: Permissions (Phase 3) - Authorization policies
3. **Week 3**: Views + Management (Phases 4-5) in parallel
4. **Week 4**: Notifications + Polish (Phases 6-7)

---

## Task Count Summary

| Phase | Functional Requirement | Tasks | Parallel | Dependencies |
|-------|------------------------|-------|----------|--------------|
| 1 | Setup | 10 | 10 | None |
| 2 | FR1 (Invitations) | 32 | 12 | Phase 1 |
| 3 | FR2 (Permissions) | 15 | 6 | Phase 2 |
| 4 | FR3 (Views) | 12 | 4 | Phase 2, 3 |
| 5 | FR4 (Management) | 14 | 4 | Phase 2, 3 |
| 6 | FR5 (Notifications) | 10 | 7 | Phase 2 |
| 7 | Polish | 2 | 2 | All |
| **Total** | | **95** | **45** | |

**Parallelization**: 47% of tasks can run in parallel (45/95)
**MVP**: 42 tasks (Phases 1-3)
**Full Feature**: 95 tasks

---

## Notes

- Tasks follow strict checklist format: `- [ ] [TaskID] [P?] [FR?] Description with file path`
- [P] marker indicates task can run in parallel
- [FR#] label maps task to functional requirement
- SendGrid API key required for email (configure in appsettings.json)
- SignalR for real-time sync (optional for MVP, can use polling fallback)
- Policy-based authorization is standard ASP.NET Core pattern
- Audit log is write-only (no undo/redo functionality)

**Ready for implementation!** Start with Phase 1 (Setup) and proceed incrementally.

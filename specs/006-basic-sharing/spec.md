# Feature Specification: Basic Sharing

**Feature ID**: 006-basic-sharing
**Created**: 2025-11-28
**Status**: Draft
**Dependencies**: Feature 001 (Collection Management), Feature 002 (Item Management)

## Overview

Enable users to share their collections with other users via email invitation or username lookup. Collaborators can be granted read-only (view) or edit permissions. Shared collections appear in the collaborator's collection list alongside their own collections. This feature supports single-user sharing only (one owner, multiple individual collaborators) - group sharing is deferred to Feature 007.

## User Scenarios & Testing

### Primary User Flow

**Scenario 1: Owner shares collection with read-only access**
1. User (Alice) owns "Vintage Vinyl Records" collection with 50 items
2. Alice navigates to collection settings
3. Clicks "Share Collection"
4. Enters Bob's email address: bob@example.com
5. Selects permission level: "View Only"
6. Clicks "Send Invitation"
7. System sends email to Bob with invitation link
8. Bob receives email, clicks invitation link
9. Bob sees "Accept Invitation" page with collection details (name, owner, item count, permission level)
10. Bob clicks "Accept"
11. "Vintage Vinyl Records" appears in Bob's collection list with "Shared by Alice" badge
12. Bob can view all 50 items but cannot add/edit/delete items
13. Alice sees Bob in "Collaborators" list with "View Only" permission

**Expected outcome**: Bob can view Alice's collection but cannot modify it. Alice maintains full ownership and control.

**Scenario 2: Owner shares collection with edit access**
1. User (Carol) owns "Comic Book Collection" with 30 items
2. Carol shares collection with Dave (dave@example.com), permission: "Can Edit"
3. Dave accepts invitation
4. "Comic Book Collection" appears in Dave's collection list
5. Dave can:
   - View all 30 items
   - Add new items to the collection
   - Edit existing item details (name, notes, attributes)
   - Mark items as deleted (soft delete to trash)
6. Dave cannot:
   - Delete the collection itself
   - Change collection name or description
   - Remove Carol as owner
   - Invite additional collaborators (only owner can share)
7. Carol sees Dave's edits in real-time (item added notification)

**Expected outcome**: Dave can collaboratively manage items but cannot modify collection-level settings or ownership.

**Scenario 3: Owner revokes access**
1. Alice previously shared "Vintage Vinyl Records" with Bob (View Only)
2. Alice navigates to collection settings → Collaborators
3. Clicks "Remove" next to Bob's name
4. Confirms revocation
5. "Vintage Vinyl Records" immediately disappears from Bob's collection list
6. Bob can no longer view any items in the collection
7. Bob receives email notification: "Access to 'Vintage Vinyl Records' has been revoked"
8. Alice's collection remains unchanged

**Expected outcome**: Bob loses all access immediately. No data loss for Alice.

**Scenario 4: Collaborator declines invitation**
1. Eve receives invitation to "Stamp Collection" from Frank
2. Eve clicks invitation link
3. Eve reviews collection details
4. Eve clicks "Decline"
5. Invitation is marked as declined
6. Frank sees "Invitation declined" status in Collaborators list
7. Eve does not see collection in her list

**Expected outcome**: Declined invitations don't grant access. Frank is aware of the decline.

### Edge Cases

1. **Invitation to non-existent email**: System sends invitation email; if recipient signs up later with that email, invitation is auto-applied
2. **Invitation to existing user**: User receives both email and in-app notification; collection appears in "Pending Invitations" section
3. **Duplicate invitation**: System shows "Already invited" error; option to resend invitation
4. **Self-invitation**: System rejects with "Cannot share with yourself"
5. **Maximum collaborators (10) reached**: System shows "Maximum 10 collaborators per collection" error
6. **Owner deletes collection**: All collaborator access revoked; collaborators receive email notification
7. **Collaborator deletes their account**: Collection share automatically removed from owner's collaborator list
8. **Permission change**: Owner changes Bob from "View Only" to "Can Edit" → Bob receives email notification and sees updated permissions immediately
9. **Concurrent edits**: Two collaborators edit same item simultaneously → last write wins (eventual consistency); conflict notification shown

## Functional Requirements

### FR1: Invitation System

**Must Have:**
- Users can invite collaborators by email address or username
- System sends email invitation with:
  - Collection name, item count, owner name
  - Permission level (View Only / Can Edit)
  - Accept/Decline buttons (secure links)
  - Expiration: 7 days (configurable)
- In-app notification for existing users
- Pending invitations visible in owner's Collaborators list with status:
  - Pending (awaiting response)
  - Accepted (active collaboration)
  - Declined (invitation rejected)
  - Expired (not accepted within 7 days)
- Resend invitation option (resets expiration)
- Cancel pending invitation option

**Acceptance Criteria:**
- Email sent within 1 minute of invitation creation
- Accept/Decline links are single-use, time-bound (7 days)
- Expired invitations automatically removed from Pending list after 30 days
- User cannot invite same collaborator twice (unless previous invitation declined/expired)
- Maximum 10 active collaborators per collection enforced

### FR2: Permission Levels

**Must Have:**
- Two permission levels:
  - **View Only**: Can view collection and items (read-only)
  - **Can Edit**: Can view, add, edit, delete items (but not collection itself)
- Owner retains exclusive rights:
  - Change collection name/description
  - Delete collection
  - Invite/remove collaborators
  - Change collaborator permissions
- Collaborators cannot:
  - Transfer ownership
  - Invite additional collaborators
  - Delete collection
  - Modify collection-level settings

**Acceptance Criteria:**
- View Only users cannot access Create/Edit/Delete item buttons
- Can Edit users can modify items but not collection settings
- Permission changes take effect immediately (no cache staleness)
- UI clearly shows permission level on shared collections ("View Only" / "Can Edit" badge)

### FR3: Shared Collection Views

**Must Have:**
- Shared collections appear in collaborator's collection list with:
  - Collection name
  - "Shared by [Owner Name]" badge
  - Permission level indicator ("View Only" / "Can Edit")
  - Item count
  - Last updated timestamp
- Visual distinction from owned collections (icon, color, badge)
- Filter: "My Collections" / "Shared With Me" / "All"
- Shared collections sortable by:
  - Name (alphabetical)
  - Last updated (newest first)
  - Item count (highest first)

**Acceptance Criteria:**
- Shared collections clearly distinguishable from owned collections
- Filter toggles work instantly (<100ms)
- Clicking shared collection opens item list view (respecting permissions)
- Owner's edits reflect in collaborator's view within 5 seconds (real-time sync)

### FR4: Collaborator Management

**Must Have:**
- Owner can view list of collaborators with:
  - Name, email, permission level
  - Date added
  - Last accessed timestamp
- Owner can:
  - Change permission level (View Only ↔ Can Edit)
  - Remove collaborator (revoke access)
  - Resend invitation (if pending)
  - Cancel invitation (if pending)
- Collaborator can:
  - View their own permission level
  - Leave shared collection (stop collaboration)
- Audit log tracks:
  - Invitation sent/accepted/declined/expired
  - Permission changes
  - Collaborator added/removed
  - Items added/edited/deleted by collaborators (user attribution)

**Acceptance Criteria:**
- Removing collaborator immediately revokes access (<5 seconds)
- Permission change takes effect immediately
- Audit log shows user attribution for all actions
- "Leave" action removes collection from collaborator's view instantly

### FR5: Notifications

**Must Have:**
- Email notifications for:
  - Invitation received (with Accept/Decline links)
  - Invitation accepted by collaborator (to owner)
  - Access revoked (to collaborator)
  - Permission changed (to collaborator)
  - Collection deleted by owner (to all collaborators)
- In-app notifications for:
  - New invitation received
  - Collaborator joined collection
  - Collaborator left collection
  - Items added/edited/deleted by collaborators (optional, user preference)
- Notification preferences:
  - Email: Always, Daily digest, Never
  - In-app: Instant, Hourly summary, Never

**Acceptance Criteria:**
- Email delivery within 5 minutes of event
- In-app notifications appear within 30 seconds (WebSocket/SignalR)
- Notification preferences saved and respected
- Unsubscribe link in all emails

## Success Criteria

**Measurable Outcomes:**
1. **Adoption**: 40% of users share at least 1 collection within first 3 months
2. **Collaboration**: Average 2-3 active collaborators per shared collection
3. **Invitation Acceptance Rate**: 70% of invitations accepted within 7 days
4. **Permission Distribution**: 60% View Only, 40% Can Edit (indicates trust balance)
5. **Performance**: Invitation sent in <500ms, permission check in <50ms
6. **User Satisfaction**: 85% of users rate sharing as "Easy to use" or "Very easy"

**Qualitative Outcomes:**
- Users can safely collaborate on collections without ownership transfer
- Clear permission boundaries prevent accidental data loss
- Real-time sync enables collaborative cataloging sessions
- Email invitations work seamlessly for users not yet signed up

## Non-Functional Requirements

### Security
- Invitation tokens are cryptographically secure (128-bit random)
- Tokens are single-use (invalidated after accept/decline)
- Permission checks enforced at API layer (policy-based authorization)
- Collaborators cannot escalate permissions
- SQL injection prevention (parameterized queries)

### Performance
- Invitation creation: <500ms
- Permission check: <50ms (cached in-memory for 30 seconds)
- Shared collection list load: <300ms for 100 collections
- Real-time sync latency: <5 seconds (SignalR/WebSocket)

### Usability
- Invitation flow: 3 clicks (Share → Enter email → Send)
- Permission change: 2 clicks (Collaborators → Change permission dropdown)
- Clear visual distinction between owned and shared collections
- Mobile-responsive sharing UI

### Scalability
- Support 10 collaborators per collection
- Support 100 shared collections per user
- Support 10,000 active invitations system-wide

## Out of Scope

- Group-based sharing (deferred to Feature 007)
- Role-based permissions beyond View/Edit (e.g., Comment-only, Download-only)
- Shared collection folders or hierarchies
- Collection templates/duplication across users
- Public sharing (share link without authentication)
- External integrations (share to social media, export to Google Sheets)
- Offline collaboration or conflict resolution UI
- Granular item-level permissions (all items inherit collection permission)

## Assumptions

1. Users trust collaborators they invite (no approval workflow for collaborator actions)
2. Email is primary invitation method (username lookup is secondary)
3. Last write wins for concurrent edits (no CRDT or OT conflict resolution)
4. Collaborators have existing accounts or will sign up to accept invitations
5. SendGrid or similar email service is available for invitation emails
6. Real-time sync is "best effort" (5-second SLA acceptable, not milliseconds)
7. Audit log is read-only (no undo/redo for collaborative actions)
8. Max 10 collaborators is sufficient for most personal collections

## Dependencies

- **Feature 001 (Collection Management)**: Collections must exist to be shared
- **Feature 002 (Item Management)**: Items must exist to be viewed/edited by collaborators
- **Email Service**: SendGrid (or alternative) for invitation emails
- **Real-time Service**: SignalR or WebSockets for real-time sync (optional for MVP)
- **Authentication**: User accounts (user IDs, email addresses)

## Key Entities

### CollectionShare (Aggregate Root)
- CollectionShareId (Guid, PK)
- CollectionId (Guid, FK to Collections)
- SharedWithUserId (Guid, FK to Users)
- Permission (enum: ViewOnly, CanEdit)
- InvitedByUserId (Guid, FK to Users)
- InvitedAt (DateTimeOffset)
- AcceptedAt (DateTimeOffset, nullable)
- Status (enum: Pending, Accepted, Declined, Expired, Revoked)
- ExpiresAt (DateTimeOffset) - invitation expiration
- LastAccessedAt (DateTimeOffset, nullable)

### ShareInvitation (Value Object)
- InvitationToken (string, unique) - secure token for accept/decline links
- Email (string) - invited user's email
- CollectionId (Guid)
- Permission (enum: ViewOnly, CanEdit)
- ExpiresAt (DateTimeOffset)

### ShareAuditLog (Aggregate Root)
- AuditLogId (Guid, PK)
- CollectionId (Guid, FK)
- UserId (Guid, FK) - user who performed action
- Action (enum: Invited, Accepted, Declined, PermissionChanged, Revoked, ItemAdded, ItemEdited, ItemDeleted)
- Details (JSON) - action metadata
- Timestamp (DateTimeOffset)

## Success Metrics

### Business Metrics
- Number of collections shared
- Invitation acceptance rate
- Average collaborators per shared collection
- Permission distribution (View vs Edit)
- Collaboration session duration

### Technical Metrics
- Invitation creation latency (p50, p95, p99)
- Permission check latency (cached vs uncached)
- Email delivery success rate
- Real-time sync latency
- Concurrent edit conflicts

### User Experience Metrics
- Time to send first invitation
- Invitation flow completion rate
- Permission change frequency (indicates trust evolution)
- Collaborator churn rate (removed/left)

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Email spam filters block invitations | High | Medium | Use reputable email service (SendGrid), test deliverability, provide in-app fallback |
| Concurrent edits cause data loss | High | Low | Last write wins + conflict notification, eventual consistency acceptable |
| Permission escalation vulnerability | High | Low | Policy-based authorization at API layer, unit tests for permission checks |
| Collaborators abuse edit permissions | Medium | Medium | Audit log tracks all actions, owner can revoke access, soft delete enables recovery |
| Invitation token leakage | Medium | Low | Tokens are single-use, expire in 7 days, HTTPS only |
| Real-time sync failure | Low | Medium | Graceful degradation - manual refresh fallback, 5-second SLA not critical |

## Open Questions

None - all critical decisions have reasonable defaults:
- Email as primary invitation method (industry standard)
- 7-day invitation expiration (common practice)
- 10 collaborator limit (sufficient for personal collections)
- Last write wins for conflicts (acceptable for collaborative cataloging)

## Acceptance Testing

### Test Case 1: Send and Accept Invitation
**Given**: Alice owns "Vintage Cameras" collection
**When**: Alice invites Bob (bob@example.com) with "View Only" permission
**Then**: Bob receives email with Accept/Decline links
**And**: Bob clicks Accept
**And**: "Vintage Cameras" appears in Bob's collection list with "Shared by Alice" badge
**And**: Bob can view items but cannot edit
**And**: Alice sees Bob in Collaborators list with "View Only" permission

### Test Case 2: Change Permission Level
**Given**: Carol shared "Stamps" collection with Dave (View Only)
**When**: Carol changes Dave's permission to "Can Edit"
**Then**: Dave receives email notification of permission change
**And**: Dave can now add/edit/delete items in "Stamps" collection
**And**: Dave sees "Can Edit" permission indicator

### Test Case 3: Revoke Access
**Given**: Eve shared "Coins" with Frank (Can Edit)
**When**: Eve clicks "Remove" next to Frank in Collaborators list
**Then**: Frank loses access immediately (<5 seconds)
**And**: "Coins" disappears from Frank's collection list
**And**: Frank receives email notification: "Access revoked"

### Test Case 4: Maximum Collaborators Limit
**Given**: George has shared "Watches" with 10 collaborators
**When**: George attempts to invite 11th collaborator (Helen)
**Then**: System shows error "Maximum 10 collaborators per collection"
**And**: Invitation is not sent

### Test Case 5: Invitation Expiration
**Given**: Ian sent invitation to Jane 8 days ago
**And**: Jane has not accepted
**When**: Jane clicks invitation link
**Then**: System shows "Invitation expired"
**And**: Jane cannot accept
**And**: Invitation status is "Expired" in Ian's Collaborators list

---

**Next Steps**: Proceed with `/speckit.plan` to generate implementation plan.

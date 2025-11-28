# Feature Specification: Sharing & Permissions

**Feature Branch**: `006-sharing-permissions`
**Created**: 2025-11-23
**Status**: Draft

## Overview

This feature enables collaborative collection management by allowing collection owners to share collections with other users or groups. Includes permission levels (view/edit), real-time notifications for sharing events and collaborative edits, and conflict resolution for concurrent editing.

**Scope**: This feature handles collection sharing, permission management, real-time notifications, and concurrent edit handling.

**Dependencies**:
- Feature 001: Core Collection Management (collections must exist)
- Feature 002: Basic Item Management (collaborative editing of items)

**Related Features**:
- Feature 003: Soft Delete & Recovery (notifications for approaching deletions)
- Feature 005: Custom Item Types (sharing collections with custom types)

## Clarifications

### Session 2025-11-23

- Q: Should notifications be real-time within the application? → A: Yes, real-time in-app notifications are required
- Q: Which events should trigger real-time notifications? → A: Collection sharing events (shared with you, access revoked), collaborative edits by others in shared collections, system events (item approaching 30-day deletion, conflict resolution)
- Two permission levels: view (read-only) and edit (full access)
- WebSocket or Server-Sent Events (SSE) required for real-time delivery
- Email notifications are optional fallback

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Share Collections with Permissions (Priority: P9)

A user wants to share their collection with specific users or groups, granting them either view-only or edit permissions.

**Why this priority**: Enables collaboration and social aspects but not critical for solo collection management. This adds significant scope and should be implemented after core collection features are stable.

**Independent Test**: Can be tested by sharing a collection with another user with view permissions, verifying that user can see but not edit, then changing permissions to edit and verifying they can now modify items.

**Acceptance Scenarios**:

1. **Given** a user owns a collection, **When** they choose to share it and specify another user's email with "view" permission, **Then** that user receives access and can view the collection but not edit
2. **Given** a user owns a collection, **When** they grant "edit" permission to another user, **Then** that user can add, edit, and remove items from the collection
3. **Given** a collection is shared with multiple users, **When** the owner views permissions, **Then** they see a list of all users with their respective permission levels
4. **Given** a user has been granted access to a shared collection, **When** the owner revokes their access, **Then** the user can no longer view or access the collection
5. **Given** a user is viewing shared collections, **When** they filter their collection list, **Then** they can distinguish between owned collections and collections shared with them
6. **Given** a collection owner wants to share with a group, **When** they select a user group, **Then** all members of that group receive access according to the specified permission level

---

### User Story 2 - Real-Time Notifications

A user wants to receive immediate in-app notifications when collections are shared with them, access is revoked, or collaborators make changes to shared collections.

**Why this priority**: Essential for collaborative awareness and keeping users informed of important events without requiring email or external notifications.

**Independent Test**: Can be tested by having two users share and edit collections while monitoring notification delivery in real-time.

**Acceptance Scenarios**:

1. **Given** a collection is shared with a user, **When** the sharing occurs, **Then** the user receives an in-app notification within 1 second
2. **Given** a user has edit access to a shared collection, **When** another user adds an item, **Then** they receive a real-time notification of the addition
3. **Given** a user has edit access to a shared collection, **When** another user modifies an item, **Then** they receive a notification showing what changed
4. **Given** a user has items in "Deleted Items", **When** an item is 3 days from permanent deletion, **Then** they receive a notification warning
5. **Given** a user receives notifications, **When** they click on a notification, **Then** they are navigated to the relevant collection or item
6. **Given** a user has notification history, **When** they open the notification panel, **Then** they see all recent notifications with timestamps
7. **Given** a user makes changes to a collection, **When** those changes occur, **Then** they do NOT receive notifications for their own actions

---

### User Story 3 - Manage Notification Preferences

A user wants to control which types of notifications they receive to avoid notification overload.

**Why this priority**: Provides user control over notification experience, preventing fatigue while ensuring important updates aren't missed.

**Independent Test**: Can be tested by disabling specific notification types and verifying those notifications are no longer delivered.

**Acceptance Scenarios**:

1. **Given** a user accesses notification settings, **When** they view preferences, **Then** they see toggles for each notification type
2. **Given** a user disables "collaborative edit" notifications, **When** another user edits a shared collection, **Then** no notification is delivered
3. **Given** a user disables notifications for a specific collection, **When** changes occur in that collection, **Then** no notifications are delivered
4. **Given** a user changes notification preferences, **When** they save changes, **Then** the new preferences take effect immediately

---

### User Story 4 - Concurrent Edit Handling

Multiple users with edit permission want to work on the same collection simultaneously without losing data.

**Why this priority**: Essential for collaborative editing to prevent data loss and user frustration.

**Independent Test**: Can be tested by having two users edit the same item simultaneously and verifying conflict resolution works correctly.

**Acceptance Scenarios**:

1. **Given** two users are editing the same item simultaneously, **When** both try to save, **Then** last write wins and the losing user receives a notification
2. **Given** a user is editing an item, **When** another user saves changes first, **Then** the slower user sees an update notification before their save
3. **Given** a user's changes are overwritten by concurrent edit, **When** they are notified, **Then** they can view the other user's changes before re-editing
4. **Given** multiple users are adding items to the same collection, **When** they save concurrently, **Then** all items are added successfully without conflict

---

### Edge Cases

- What happens when a user tries to share a collection with someone who doesn't have an account (invitation workflow needed)?
- What happens when two users with edit permission edit the same item simultaneously?
- What happens when a collection owner tries to remove their own ownership?
- What happens when a collection owner deletes a collection that has been shared with others?
- What happens when a user's access to a shared collection is revoked while they are actively editing?
- What happens when a user group is deleted but collections are still shared with that group?
- What happens when notification delivery fails (user offline, connection issues)?
- What happens when a user has hundreds of unread notifications?
- What happens when a shared collection uses a private custom item type (Feature 005)?
- What happens when a user blocks another user - should sharing still work?

## Requirements *(mandatory)*

### Functional Requirements

**Sharing & Permissions:**
- **FR-022**: System MUST support full permission system allowing collection owners to share collections with specific users
- **FR-023**: System MUST support two permission levels for shared collections: "view" (read-only) and "edit" (full access)
- **FR-024**: System MUST allow collection owners to grant, modify, and revoke access permissions for shared users
- **FR-025**: System MUST enforce permission controls, preventing unauthorized access to private or restricted collections
- **FR-026**: System MUST distinguish between owned collections and collections shared with the user in the collection list
- **FR-027**: System MUST notify users when a collection is shared with them or when their access is revoked
- **FR-028**: System MUST support sharing with groups of users (predefined user groups)
- **FR-029**: System MUST track and display who shared access and when for audit purposes

**Real-Time Notifications:**
- **FR-037**: System MUST support real-time in-app notifications for the following event types:
  - Collection sharing: when a collection is shared with the user or access is revoked
  - Collaborative edits: when another user adds, modifies, or removes items in a shared collection
  - System events: when an item in "Deleted Items" is approaching 30-day expiration (3 days before), when concurrent edit conflicts are resolved
- **FR-038**: System MUST deliver notifications instantly (within 1 second) to all active user sessions
- **FR-039**: System MUST maintain notification history for users to review past notifications
- **FR-040**: System MUST allow users to manage notification preferences (enable/disable specific notification types)
- **FR-041**: System MUST NOT send notifications for user's own actions (only actions by other users in shared collections)

**Concurrent Editing:**
- **FR-070**: System MUST implement optimistic concurrency control for shared collections with edit access
- **FR-071**: System MUST use last-write-wins strategy when conflicts occur with notification to the losing user
- **FR-072**: System MUST show real-time indicators when other users are actively viewing/editing a shared collection
- **FR-073**: System MUST allow multiple users to add items concurrently without conflicts

**User Groups:**
- **FR-074**: System MUST allow creation and management of user groups (assumes external user management system)
- **FR-075**: System MUST automatically grant/revoke access when users are added/removed from groups
- **FR-076**: System MUST show group-based permissions separately from individual user permissions

### Key Entities

- **Collection Permission**: Defines access rights for a user or group to a specific collection. Contains the collection reference, user/group reference, permission level (view or edit), and grant metadata (who shared, when shared). Enforces access control for shared collections.
- **User Group**: Represents a named group of users that can be granted collective access to collections. Simplifies permission management when sharing with multiple users. Contains group name and member list.
- **Notification**: Represents a real-time event notification delivered to a user. Contains event type, timestamp, related entity references (collection, item, user who triggered action), read/unread status, and notification content. Delivered via real-time connection and stored for notification history.
- **Notification Preference**: Defines user's notification settings. Contains user reference, enabled notification types, collection-specific overrides, and delivery preferences.
- **Edit Lock** (optional): Tracks active editing sessions for optimistic concurrency control. Contains item reference, user reference, session start time, and last activity timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Sharing:**
- **SC-009**: Shared collections with view permission prevent 100% of unauthorized edit attempts
- **SC-010**: Users can share a collection and grant access to another user in under 2 minutes
- **SC-011**: Permission changes (grant, revoke, modify) take effect within 5 seconds for all active sessions
- **SC-012**: System supports at least 50 concurrent users with edit access to a single shared collection without conflicts

**Notifications:**
- **SC-016**: Real-time notifications are delivered to active users within 1 second of the triggering event
- **SC-017**: Users can manage their notification preferences with changes taking effect immediately
- **SC-037**: Notification history displays at least 100 most recent notifications
- **SC-038**: Notification system handles 1,000+ concurrent active connections without performance degradation

**Concurrent Editing:**
- **SC-039**: Concurrent edit conflicts are resolved within 2 seconds with proper notification
- **SC-040**: Users receive update notifications before potential conflicts 95% of the time

## Assumptions *(mandatory)*

1. **User Authentication**: Assumes users are authenticated and user accounts exist before sharing can occur.

2. **User Groups**: Assumes user groups are managed outside this feature (existing user management system). This feature only consumes group data for permission assignment.

3. **Real-Time Infrastructure**: Assumes system supports WebSocket or Server-Sent Events (SSE) for real-time notification delivery. Assumes persistent connections maintained for active users.

4. **Email Fallback**: Assumes email notifications are optional fallback for users who are offline when events occur. Email implementation is future enhancement.

5. **Invitation Workflow**: Assumes users can only share with existing registered users for MVP. Invitation workflow for non-users is future enhancement.

6. **Permission Model**: Assumes two-tier permission model (view, edit) is sufficient for MVP. More granular permissions are future enhancements.

7. **Ownership Transfer**: Assumes collection ownership cannot be transferred. Owner can revoke their own access, which would delete the collection.

8. **Concurrent Edit Strategy**: Assumes optimistic locking with last-write-wins conflict resolution. Pessimistic locking (edit locks) is future enhancement.

9. **Notification Storage**: Assumes notifications stored for at least 30 days. Older notifications are archived or purged to manage storage.

10. **Collection Deletion**: Assumes when owner deletes a shared collection, all shared users immediately lose access and receive notification.

11. **Access Revocation**: Assumes when user's access is revoked while actively editing, changes in progress are lost and user receives notification.

12. **Performance**: Assumes typical collections shared with 2-10 users. System must handle edge case of 50+ concurrent editors for large organizations.

13. **Custom Type Visibility**: Assumes collections using private custom item types (Feature 005) can be shared, and shared users can see custom attributes even though type isn't globally available.

14. **Notification Deduplication**: Assumes system prevents duplicate notifications for the same event (e.g., if user has multiple active sessions).

15. **Group Permission Changes**: Assumes when permissions are changed for a group, all group members' access is updated immediately.

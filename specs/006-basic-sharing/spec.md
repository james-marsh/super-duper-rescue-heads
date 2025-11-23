# Feature Specification: Basic Sharing & Permissions

**Feature Branch**: `006-basic-sharing`
**Created**: 2025-11-23
**Status**: Draft

## Overview

This feature enables collection owners to share collections with individual users, granting either view-only or edit permissions. Users can manage permissions and distinguish between owned and shared collections.

**Scope**: This feature handles basic sharing with individual users only. Group sharing is covered in Feature 007, real-time notifications in Feature 008, and concurrent editing in Feature 009.

**Dependencies**:
- Feature 001: Core Collection Management (collections must exist)
- Feature 002: Basic Item Management (shared users can view/edit items)

**Related Features**:
- Feature 007: Group Sharing (extends sharing to user groups)
- Feature 008: Real-Time Notifications (notifies users of sharing events)
- Feature 009: Concurrent Editing (handles multi-user editing conflicts)

## Clarifications

### Session 2025-11-23

- Q: Should collections be shareable? → A: Yes, with permission levels
- Two permission levels: view (read-only) and edit (full access)
- Share with individual users only (groups in Feature 007)
- Owner can grant, modify, and revoke permissions
- Shared users receive basic notification (infrastructure in Feature 008)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Share Collection with View Permission

A user wants to share their collection with another user, granting view-only access so they can browse but not modify items.

**Why this priority**: Enables basic collaboration and collection sharing without risk of accidental modifications.

**Independent Test**: Can be tested by sharing a collection with another user with view permissions, verifying that user can see all items but cannot add, edit, or delete anything.

**Acceptance Scenarios**:

1. **Given** a user owns a collection, **When** they choose to share it and specify another user's email with "view" permission, **Then** that user receives access and can view the collection but not edit
2. **Given** a user has view permission on a shared collection, **When** they try to add an item, **Then** the system prevents the action and shows a permission error
3. **Given** a user has view permission on a shared collection, **When** they try to edit an item, **Then** the system prevents the action and shows a permission error
4. **Given** a user has view permission on a shared collection, **When** they try to delete an item, **Then** the system prevents the action and shows a permission error

---

### User Story 2 - Share Collection with Edit Permission

A user wants to share their collection with another user, granting full edit access so they can add, modify, and remove items.

**Why this priority**: Enables full collaboration where multiple users can contribute to building and maintaining a collection.

**Independent Test**: Can be tested by sharing a collection with another user with edit permissions, verifying that user can add, edit, and delete items just like the owner.

**Acceptance Scenarios**:

1. **Given** a user owns a collection, **When** they grant "edit" permission to another user, **Then** that user can add, edit, and remove items from the collection
2. **Given** a user has edit permission on a shared collection, **When** they add an item, **Then** the item is saved and visible to all users with access
3. **Given** a user has edit permission on a shared collection, **When** they edit an item, **Then** the changes are saved and visible to all users with access
4. **Given** a user has edit permission on a shared collection, **When** they delete an item, **Then** the item is soft-deleted (Feature 003) and moved to "Deleted Items"

---

### User Story 3 - Manage Collection Permissions

A collection owner wants to view all users who have access to their collection and modify or revoke permissions as needed.

**Why this priority**: Provides collection owners with control over who can access their collections and at what level.

**Independent Test**: Can be tested by sharing with multiple users, viewing the permissions list, changing permission levels, and revoking access.

**Acceptance Scenarios**:

1. **Given** a collection is shared with multiple users, **When** the owner views permissions, **Then** they see a list of all users with their respective permission levels
2. **Given** a user has view permission, **When** the owner changes it to edit permission, **Then** the user can now edit items in the collection
3. **Given** a user has edit permission, **When** the owner changes it to view permission, **Then** the user can no longer edit items
4. **Given** a user has access to a shared collection, **When** the owner revokes their access, **Then** the user can no longer view or access the collection
5. **Given** permissions are changed, **When** the shared user refreshes their view, **Then** the new permissions take effect within 5 seconds

---

### User Story 4 - Distinguish Owned vs Shared Collections

A user wants to easily see which collections they own versus which collections have been shared with them.

**Why this priority**: Helps users understand their relationship to collections and what actions they can perform.

**Independent Test**: Can be tested by having collections owned by user and collections shared with user, verifying they are clearly distinguished in the UI.

**Acceptance Scenarios**:

1. **Given** a user is viewing their collection list, **When** they see the list, **Then** owned collections and shared collections are clearly distinguished (e.g., with icons or labels)
2. **Given** a user is viewing a shared collection, **When** they see the collection details, **Then** the owner's name is displayed
3. **Given** a user has multiple collections, **When** they filter by "Shared with me", **Then** only collections others have shared with them appear
4. **Given** a user views a shared collection, **When** they see the permission level, **Then** it's clearly indicated whether they have view or edit access

---

### Edge Cases

- What happens when a user tries to share a collection with someone who doesn't have an account?
- What happens when a collection owner tries to remove their own ownership?
- What happens when a collection owner deletes a collection that has been shared with others?
- What happens when a user's access to a shared collection is revoked while they are actively viewing it?
- What happens when a user tries to share with the same user twice?
- What happens when sharing with a user who is already in a group with access (Feature 007)?
- What happens when owner shares a collection using a private custom item type (Feature 005)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-022**: System MUST support permission system allowing collection owners to share collections with specific users
- **FR-023**: System MUST support two permission levels for shared collections: "view" (read-only) and "edit" (full access)
- **FR-024**: System MUST allow collection owners to grant, modify, and revoke access permissions for shared users
- **FR-025**: System MUST enforce permission controls, preventing unauthorized access to private or restricted collections
- **FR-026**: System MUST distinguish between owned collections and collections shared with the user in the collection list
- **FR-027**: System MUST notify users when a collection is shared with them or when their access is revoked (notification infrastructure in Feature 008)
- **FR-029**: System MUST track and display who shared access and when for audit purposes
- **FR-080**: System MUST prevent collection owners from removing their own ownership without transferring or deleting collection
- **FR-081**: System MUST allow searching for users by email address when sharing collections
- **FR-082**: System MUST prevent duplicate permission entries for the same user on the same collection

### Key Entities

- **Collection Permission**: Defines access rights for a user to a specific collection. Contains the collection reference, user reference, permission level (view or edit), and grant metadata (who shared, when shared, last modified). Enforces access control for shared collections.
- **Collection** (from Feature 001): Extended with ownership and sharing metadata.
- **User**: The owner of collections and recipient of shared collections.
- **Permission Change Event**: Records changes to permissions for audit trail. Contains collection, user, old permission level, new permission level, changed by, and timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-009**: Shared collections with view permission prevent 100% of unauthorized edit attempts
- **SC-010**: Users can share a collection and grant access to another user in under 2 minutes
- **SC-011**: Permission changes (grant, revoke, modify) take effect within 5 seconds for all active sessions
- **SC-041**: Users can view all permissions for their collections in under 1 second
- **SC-042**: Owned vs shared collections are clearly distinguished with 100% visual clarity
- **SC-043**: Permission revocations are immediate (within 2 seconds) and prevent further access

## Assumptions *(mandatory)*

1. **User Authentication**: Assumes users are authenticated and user accounts exist before sharing can occur.

2. **User Discovery**: Assumes users can be found by email address. User management system provides user lookup capability.

3. **Invitation Workflow**: Assumes users can only share with existing registered users for MVP. Invitation workflow for non-users is future enhancement (requires email system).

4. **Ownership Transfer**: Assumes collection ownership cannot be transferred. If owner wants to leave, they must delete the collection or continue as owner.

5. **Permission Model**: Assumes two-tier permission model (view, edit) is sufficient for MVP. More granular permissions (e.g., "add items only", "delete items") are future enhancements.

6. **Permission Enforcement**: Assumes all item operations (add, edit, delete) check permissions at the API level before allowing changes.

7. **Collection Deletion**: Assumes when owner deletes a shared collection, all shared users immediately lose access. Deletion is handled by Feature 001.

8. **Access Revocation**: Assumes when user's access is revoked, any changes they have in progress are handled by Feature 009 (concurrent editing).

9. **Notification Delivery**: Assumes basic notification about sharing events is provided, but real-time notification infrastructure is in Feature 008.

10. **Audit Trail**: Assumes permission changes are logged for security and compliance. Logs retained according to standard retention policies.

11. **UI Indicators**: Assumes clear visual distinction between owned and shared collections in all views (list, detail, breadcrumbs).

12. **Custom Types**: Assumes collections using private custom item types (Feature 005) can be shared, and shared users can see custom attributes even though the type isn't globally available.

13. **Multiple Devices**: Assumes users may be logged in on multiple devices. Permission changes must propagate to all active sessions.

14. **Performance**: Assumes typical collections shared with 2-10 users. Permission list must load in under 1 second for up to 50 shared users.

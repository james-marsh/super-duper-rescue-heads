# Feature Specification: Soft Delete & Recovery

**Feature Branch**: `003-soft-delete-recovery`
**Created**: 2025-11-23
**Status**: Draft

## Overview

This feature implements a safety net for item deletions by introducing a "Deleted Items" area where removed items are retained for 30 days before permanent deletion. Users can restore accidentally deleted items within this retention window.

**Scope**: This feature handles the soft delete mechanism for items, restoration workflow, and automated purging after retention period.

**Dependencies**:
- Feature 001: Core Collection Management (collections must exist)
- Feature 002: Basic Item Management (items must exist and support removal)

**Related Features**:
- None (standalone safety feature)

## Clarifications

### Session 2025-11-23

- Q: Should users be able to undo item deletion? → A: Yes, soft delete with 30-day retention before permanent deletion
- Retention period: 30 days
- Users can manually permanently delete items from "Deleted Items" area if desired
- Deleted items count toward user's storage quota during retention period

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Restore Deleted Items (Priority: P8)

A user wants to recover items they accidentally deleted from their collection before the 30-day retention period expires.

**Why this priority**: Provides a safety net for accidental deletions without burdening the primary user flows. This is a recovery feature that supports but doesn't block core collection management.

**Independent Test**: Can be tested by deleting items, navigating to "Deleted Items" area, restoring items, and verifying they reappear in the original collection with all attributes intact.

**Acceptance Scenarios**:

1. **Given** a user has deleted items, **When** they access the "Deleted Items" area, **Then** all items deleted within the past 30 days are displayed with deletion date
2. **Given** a user selects a deleted item, **When** they choose to restore it, **Then** the item is returned to its original collection
3. **Given** a user wants to remove items permanently, **When** they select items in "Deleted Items" and confirm permanent deletion, **Then** the items are immediately and permanently removed
4. **Given** a deleted item has been in "Deleted Items" for 30 days, **When** the retention period expires, **Then** the item is automatically and permanently removed from the system
5. **Given** a user tries to restore an item to a collection that has been deleted, **When** they attempt restoration, **Then** the system shows an error and the item remains in "Deleted Items"

---

### User Story 2 - Automatic Purge of Expired Items

The system automatically removes items from "Deleted Items" after the 30-day retention period to free up storage.

**Why this priority**: Essential for system maintenance and preventing indefinite storage growth.

**Independent Test**: Can be tested by creating test items with backdated deletion timestamps and verifying the automated purge process removes them correctly.

**Acceptance Scenarios**:

1. **Given** items have been in "Deleted Items" for 30 days, **When** the automated purge process runs, **Then** those items are permanently deleted
2. **Given** the purge process is running, **When** it completes, **Then** users do not experience performance degradation
3. **Given** items are about to expire (3 days before 30-day limit), **When** the notification system checks, **Then** users receive a warning notification (handled by notification system in Feature 006)

---

### Edge Cases

- What happens when a user tries to restore an item to a collection that has been deleted?
- What happens when deleted items reach 30-day retention limit while user is viewing them?
- What happens when a user reaches the maximum total items limit and deleted items count toward that limit?
- What happens if the automated purge process fails or is interrupted?
- What happens when a user manually permanently deletes an item and then regrets it?
- What happens when a shared collection's items are deleted - can collaborators restore them?
- What happens to deleted items when a user's account is deleted?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-018**: System MUST implement soft delete for items, moving deleted items to a "Deleted Items" area with 30-day retention before permanent deletion
- **FR-019**: System MUST allow users to restore items from "Deleted Items" area within the 30-day retention period
- **FR-020**: System MUST permanently delete items after 30 days in "Deleted Items" area
- **FR-021**: System MUST allow users to manually and permanently delete items from "Deleted Items" area
- **FR-054**: System MUST run automated purge process at least daily to remove expired items
- **FR-055**: System MUST maintain deletion timestamp for all items in "Deleted Items" area
- **FR-056**: System MUST preserve all item attributes during soft delete for complete restoration
- **FR-057**: System MUST maintain reference to original collection for restoration purposes
- **FR-058**: System MUST count soft-deleted items toward user's total item limit during retention period
- **FR-059**: System MUST handle restoration failures gracefully (e.g., original collection deleted)

### Key Entities

- **Deleted Item**: Represents an item that has been removed from a collection but retained for 30 days. Contains all original item data plus deletion timestamp and original collection reference. Automatically purged after 30-day retention period.
- **Item**: Reference to the item data structure (from Feature 002) with added soft-delete state.
- **Collection**: Reference to original collection for restoration (from Feature 001).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-006**: 95% of accidental item deletions are recovered through the "Deleted Items" restore feature
- **SC-008**: Users can restore deleted items within 30-day retention period with 100% data integrity (all attributes preserved)
- **SC-025**: Deleted items appear in "Deleted Items" area within 1 second of deletion
- **SC-026**: Item restoration completes within 2 seconds and item reappears in original collection
- **SC-027**: Automated purge process runs successfully daily without impacting system performance
- **SC-028**: Users receive notification 3 days before items are permanently purged (notification system in Feature 006)

## Assumptions *(mandatory)*

1. **Item Deletion**: Assumes Feature 002 handles the initial deletion request and invokes the soft delete mechanism.

2. **Retention Period**: Assumes 30-day retention period is sufficient for recovery. This is a configurable value but 30 days is the default.

3. **Storage Quotas**: Assumes soft-deleted items stored in same data structure with "deleted" flag/timestamp. No separate storage system required. Deleted items count toward user's storage quota during retention period.

4. **Automated Background Job**: Assumes system has capability to run scheduled background jobs for automated purging. Purge process runs at least daily.

5. **Restoration Rules**: Assumes items can only be restored to their original collection. If original collection is deleted, restoration fails and item remains in "Deleted Items" until retention expires.

6. **Manual Permanent Delete**: Assumes users may want to immediately free up their item quota by manually permanently deleting items from "Deleted Items" area.

7. **Notifications**: Assumes notification system (Feature 006) handles alerts about approaching expiration (3 days before 30-day limit).

8. **Collection Deletion**: Assumes when a collection is deleted (Feature 001), its items are immediately permanently deleted, bypassing soft delete. Soft delete only applies to individual item removal.

9. **Concurrent Access**: Assumes multiple users cannot restore the same deleted item simultaneously (only item owner or collection owner with edit permission from Feature 006).

10. **Performance**: Assumes "Deleted Items" area may contain hundreds of items. Pagination required for viewing.

# Feature Specification: Custom Item Types

**Feature Branch**: `005-custom-item-types`
**Created**: 2025-11-23
**Status**: Draft

## Overview

This feature enables users to create custom item types with user-defined attributes to track collections that don't fit predefined types. Includes an admin approval workflow to make custom types globally available while allowing immediate private use by creators.

**Scope**: This feature handles custom item type creation, attribute definition, admin review workflow, and type management.

**Dependencies**:
- Feature 001: Core Collection Management (collections use item types)
- Feature 002: Basic Item Management (items are structured by types)

**Related Features**:
- Feature 004: Search Functionality (custom attributes must be searchable)

## Clarifications

### Session 2025-11-23

- Q: Who can create new item types? → A: Users can create custom item types with admin oversight (users define types, admins can review/approve or provide templates)
- Users can use their own custom types immediately without approval (private to user)
- Admin-approved types become available globally to all users
- Admins can provide template types that users can customize

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Custom Item Type (Priority: P5)

A user wants to create a custom item type with specific attributes to track items that don't fit predefined types (e.g., trading cards, board games, stamps).

**Why this priority**: Enables users to track any collection without waiting for admin support. Critical for user autonomy and system flexibility, but not blocking since predefined types cover common use cases initially.

**Independent Test**: Can be tested by creating a custom item type with custom attributes, creating a collection using that type, and adding items with the custom attributes.

**Acceptance Scenarios**:

1. **Given** a user wants to track items not covered by predefined types, **When** they create a custom item type named "Trading Cards" with attributes (card name, year, rarity, condition), **Then** the item type is created and available for use immediately
2. **Given** a user is creating a custom item type, **When** they specify required vs optional attributes, **Then** the system enforces those requirements when items are added to collections of that type
3. **Given** a user creates a custom item type, **When** the type is saved, **Then** it appears in the user's available types list with "private" indicator
4. **Given** a user has created a custom item type, **When** they create a collection using that type, **Then** the collection accepts items with the custom attributes defined
5. **Given** a user creates a custom item type with duplicate name, **When** they attempt to save it, **Then** the system prompts them to choose a unique name or use the existing type

---

### User Story 2 - Admin Review and Approval Workflow

An admin wants to review user-created custom item types and approve them for global use by all users.

**Why this priority**: Ensures quality and prevents duplicate types while allowing users immediate access to their own types.

**Independent Test**: Can be tested by creating custom types as a user, reviewing them as admin, approving/rejecting, and verifying global availability.

**Acceptance Scenarios**:

1. **Given** a user creates a custom item type, **When** an admin reviews pending item types, **Then** the admin can see the custom type with all its attributes and metadata
2. **Given** an admin is reviewing a custom type, **When** they approve it, **Then** the type becomes globally available to all users
3. **Given** an admin is reviewing a custom type, **When** they reject it with feedback, **Then** the creator receives notification with rejection reason
4. **Given** an admin is reviewing a custom type, **When** they suggest modifications, **Then** the creator can update the type and resubmit for review
5. **Given** a custom item type is approved by admin, **When** other users search for item types, **Then** the approved custom type appears in the available types list
6. **Given** an admin wants to provide templates, **When** they create a template item type, **Then** users can customize it for their needs

---

### User Story 3 - Manage Custom Attribute Definitions

A user wants to define detailed attribute specifications for their custom item type including data type, required/optional status, and default values.

**Why this priority**: Provides flexibility in defining how items are structured while maintaining data integrity.

**Independent Test**: Can be tested by creating attribute definitions with various properties and verifying they're enforced when items are created.

**Acceptance Scenarios**:

1. **Given** a user is defining a custom attribute, **When** they specify the data type (text, number, date, boolean), **Then** the system enforces that data type for items
2. **Given** a user is defining a custom attribute, **When** they mark it as required, **Then** the system prevents item creation without that attribute
3. **Given** a user is defining a custom attribute, **When** they provide a default value, **Then** new items are pre-filled with that value
4. **Given** a user has created a custom type, **When** they want to add new attributes later, **Then** existing items of that type remain valid (new attributes are optional or have defaults)

---

### Edge Cases

- What happens when a custom item type needs new attributes added in the future (extensibility through flexible schema)?
- What happens when admins approve a custom type that conflicts with an existing type?
- What happens when a user deletes a custom item type that's being used by collections?
- What happens when an admin rejects a custom type that's already in use by the creator?
- What happens to collections when a custom item type is modified?
- What happens when two users create custom types with identical attributes but different names?
- What happens when a user wants to share a collection using a private custom type (Feature 006)?
- What happens when an approved custom type is later found to have issues?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-042**: System MUST allow users to create custom item types with user-defined attributes
- **FR-043**: System MUST allow users to specify attribute properties for custom item types (name, data type, required/optional, default value)
- **FR-044**: System MUST support admin review and approval workflow for custom item types before they become globally available
- **FR-045**: System MUST allow users to use their own custom item types immediately without admin approval (private to user until approved)
- **FR-046**: System MUST allow admins to provide template item types that users can customize
- **FR-047**: System MUST prevent duplicate item type names across the system (case-insensitive uniqueness)
- **FR-048**: System MUST maintain backward compatibility when item types are modified (existing collections continue to work)
- **FR-065**: System MUST support attribute data types: text, number, date, boolean, and list (select from predefined options)
- **FR-066**: System MUST allow users to edit their private custom types before admin approval
- **FR-067**: System MUST prevent deletion of item types that are in use by existing collections
- **FR-068**: System MUST track custom item type creator, creation date, and approval status
- **FR-069**: System MUST distinguish between system-provided types, approved custom types, and private custom types in the UI

### Key Entities

- **Item Type**: Defines the category of items a collection contains. Can be system-provided (e.g., "Comic Book", "Puzzle", "Vinyl Record") or user-created custom types. Contains attribute definitions (name, data type, required/optional, default value), approval status (private/pending/approved for global use), creator reference, and template indicator. Acts as a template for item structure. Custom types are private to creator until admin-approved for global availability.
- **Attribute Definition**: Defines a single attribute for an item type. Contains attribute name, data type (text, number, date, boolean, list), required/optional flag, default value (optional), validation rules, and display order.
- **Item Type Template**: An admin-provided template that users can customize. Contains base attribute definitions and recommended structure.
- **Approval Request**: Represents a pending review for a custom item type. Contains item type reference, creator, submission date, status (pending/approved/rejected), admin reviewer, review date, and feedback/notes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-031**: Users can create a custom item type with 5 attributes in under 5 minutes
- **SC-032**: Custom item types are immediately available for private use upon creation
- **SC-033**: Admins can review and approve/reject custom types in under 2 minutes per type
- **SC-034**: Approved custom types appear in global type list within 5 seconds of approval
- **SC-035**: System prevents duplicate type names 100% of the time
- **SC-036**: Existing collections remain functional when item types are modified (100% backward compatibility)

## Assumptions *(mandatory)*

1. **Predefined Types**: Assumes system ships with common predefined types (Comic Book, Puzzle, Vinyl Record) to cover typical use cases.

2. **Admin Role**: Assumes system has admin user role with permissions to review and approve custom types. Admin management is outside scope of this feature.

3. **Attribute Data Types**: Assumes supported data types are: text (string), number (integer/decimal), date, boolean, and list (select from options). Complex types (nested objects, file uploads) are future enhancements.

4. **Type Modification**: Assumes users can only edit private (unapproved) custom types. Once approved globally, types become read-only to prevent breaking existing collections.

5. **Type Deletion**: Assumes custom types cannot be deleted if any collections are using them. System prevents deletion and shows warning with collection count.

6. **Template Customization**: Assumes users can customize admin-provided templates by copying them and modifying attributes. Original template remains unchanged.

7. **Validation**: Assumes basic validation rules per data type (e.g., numbers must be numeric, dates must be valid dates). Advanced validation (regex patterns, range constraints) is future enhancement.

8. **Attribute Limits**: Assumes custom types can have up to 20 attributes. This prevents overly complex schemas.

9. **Approval Workflow**: Assumes single-step approval (approve/reject). Multi-stage approval or approval hierarchies are future enhancements.

10. **Type Visibility**: Assumes private custom types are only visible to their creator. Approved types are visible to all users. Pending types are visible to admins and creator only.

11. **Sharing Impact**: Assumes collections using private custom types can be shared (Feature 006), and shared users see the custom attributes even though the type isn't globally available.

12. **Search Integration**: Assumes custom attributes are automatically indexed for search (Feature 004) when types are used.

# GitHub Issues for 001-collection-management

These issue templates can be copied to create GitHub issues for completing the 001-collection-management feature.

---

## Issue 1: Implement ItemType Value Object

**Title:** `[001] FR-002: Implement ItemType value object for collections`

**Labels:** `enhancement`, `feature-001`, `priority-1`

**Body:**
```markdown
## Description
Implement the ItemType value object to allow users to specify item types when creating collections.

## Requirements (FR-002)
- System MUST allow users to specify an item type when creating a collection
- Predefined item types: Vinyl Record (1), Comic Book (2), Puzzle (3)

## Acceptance Criteria
- [ ] Create `ItemType` value object in Domain layer with predefined types
- [ ] Add `FromId(int id)` factory method
- [ ] Add `itemTypeId` field to `CreateCollectionRequest`
- [ ] Add `itemType` object to `CollectionResponse`
- [ ] Update Collection entity to include ItemType property
- [ ] Add item type dropdown to CollectionForm.razor
- [ ] Add EF Core configuration for ItemType
- [ ] Add unit tests for ItemType value object

## Technical Notes
- ItemType should be immutable after creation
- Reference: `specs/001-collection-management/data-model.md`
- OpenAPI contract: `specs/001-collection-management/contracts/collections-api.yaml`
```

---

## Issue 2: Add Duplicate Collection Name Validation

**Title:** `[001] FR-015: Prevent duplicate collection names within user account`

**Labels:** `enhancement`, `feature-001`, `priority-1`

**Body:**
```markdown
## Description
Prevent users from creating collections with duplicate names within their account.

## Requirements (FR-015)
- System MUST prevent duplicate collection names within a single user's account
- Should return 409 Conflict for duplicate names

## Acceptance Criteria
- [ ] Add `ExistsByNameAndOwnerAsync(string name, Guid ownerId)` to ICollectionRepository
- [ ] Implement duplicate check in CollectionRepository
- [ ] Add validation in POST /collections endpoint (return 409 Conflict)
- [ ] Add validation in PUT /collections endpoint (return 409 Conflict)
- [ ] Display user-friendly error message in UI
- [ ] Add integration tests for duplicate name scenarios

## Technical Notes
- Check should be case-insensitive
- Soft-deleted collections should not count as duplicates
- Error response should follow RFC 7807 Problem Details format
```

---

## Issue 3: Implement Collection Limit Enforcement

**Title:** `[001] FR-049/051/052: Enforce configurable collection limits per user`

**Labels:** `enhancement`, `feature-001`, `priority-1`

**Body:**
```markdown
## Description
Enforce a configurable maximum number of collections per user (default: 100).

## Requirements
- **FR-049**: System MUST enforce configurable limits on collections per user (default: 100)
- **FR-051**: System MUST display clear error messages when users attempt to exceed limits
- **FR-052**: System MUST allow administrators to adjust limit configurations without code deployment

## Acceptance Criteria
- [ ] Add `CountByOwnerIdAsync(Guid ownerId)` to ICollectionRepository
- [ ] Add configuration setting for max collections (appsettings.json)
- [ ] Add validation in POST /collections endpoint (return 409 Conflict when limit exceeded)
- [ ] Return clear error message with current count and max allowed
- [ ] Display user-friendly error message in UI
- [ ] Add integration tests for limit enforcement

## Technical Notes
- Default limit: 100 collections per user
- Configuration should be in appsettings.json for runtime changes
- Error response should follow RFC 7807 Problem Details format
- Consider adding IOptions<CollectionLimitsOptions> for DI
```

---

## Issue 4: Add Pagination Support to GET /collections

**Title:** `[001] Add pagination support to GET /collections endpoint`

**Labels:** `enhancement`, `feature-001`, `priority-2`

**Body:**
```markdown
## Description
Add pagination support to the collections list endpoint per OpenAPI contract.

## Requirements
- API should support `skip` and `take` query parameters
- Response should include pagination metadata (total, hasMore)

## Acceptance Criteria
- [ ] Add `skip` query parameter (default: 0, min: 0)
- [ ] Add `take` query parameter (default: 20, min: 1, max: 100)
- [ ] Update `GetByOwnerIdAsync` to support pagination
- [ ] Add pagination object to response: `{ total, skip, take, hasMore }`
- [ ] Update UI to handle pagination (Load More button or pagination controls)
- [ ] Add integration tests for pagination

## Technical Notes
- Reference OpenAPI spec: `specs/001-collection-management/contracts/collections-api.yaml`
- Use consistent pagination pattern across all list endpoints
```

---

## Issue 5: Add Sorting Support to GET /collections

**Title:** `[001] Add sorting support to GET /collections endpoint`

**Labels:** `enhancement`, `feature-001`, `priority-2`

**Body:**
```markdown
## Description
Add sorting support to the collections list endpoint per OpenAPI contract.

## Requirements
- API should support `sort` query parameter
- Valid sort options: createdAt:desc, createdAt:asc, name:asc, name:desc

## Acceptance Criteria
- [ ] Add `sort` query parameter (default: createdAt:desc)
- [ ] Validate sort parameter against allowed values
- [ ] Update repository to apply sorting
- [ ] Add sorting dropdown to UI
- [ ] Add integration tests for sorting

## Technical Notes
- Reference OpenAPI spec: `specs/001-collection-management/contracts/collections-api.yaml`
- Default sort: newest first (createdAt:desc)
```

---

## Issue 6: Add Usage Indicators to UI

**Title:** `[001] FR-053: Display collection usage indicators in UI`

**Labels:** `enhancement`, `feature-001`, `priority-2`

**Body:**
```markdown
## Description
Show users how close they are to their collection limit.

## Requirements (FR-053)
- System MUST provide usage indicators showing users how close they are to limits
- Example: "87 of 100 collections"

## Acceptance Criteria
- [ ] Add endpoint or include in response: current count and max allowed
- [ ] Display usage indicator on Collections page header
- [ ] Show warning when approaching limit (e.g., 90%+)
- [ ] Update indicator after create/delete operations

## Technical Notes
- Could be included in GET /collections response metadata
- Or create separate GET /collections/usage endpoint
- Consider caching for performance
```

---

## Issue 7: Add Unit Tests for Collection Domain

**Title:** `[001] Add unit tests for Collection aggregate and CollectionName`

**Labels:** `testing`, `feature-001`, `priority-3`

**Body:**
```markdown
## Description
Add comprehensive unit tests for the Collection domain model.

## Acceptance Criteria
- [ ] Add tests for CollectionName value object
  - [ ] Create with valid name succeeds
  - [ ] Create with empty name throws
  - [ ] Create with name > 200 chars throws
  - [ ] Whitespace is trimmed
- [ ] Add tests for Collection entity
  - [ ] Create with valid data succeeds
  - [ ] UpdateName with valid name succeeds
  - [ ] UpdateName on deleted collection throws
  - [ ] UpdateDescription with valid description succeeds
  - [ ] MarkAsDeleted sets IsDeleted and DeletedAt
  - [ ] Restore clears IsDeleted and DeletedAt
  - [ ] IsOwnedBy returns correct result
  - [ ] Domain events are raised correctly

## Technical Notes
- Use TUnit framework (existing in project)
- Follow existing test patterns in `SuperDuperRescueHeads.Tests.Unit`
```

---

## Issue 8: Add Integration Tests for Collections API

**Title:** `[001] Add integration tests for Collections API endpoints`

**Labels:** `testing`, `feature-001`, `priority-3`

**Body:**
```markdown
## Description
Add integration tests for all Collections API endpoints.

## Acceptance Criteria
- [ ] Add tests for POST /collections
  - [ ] Create with valid data returns 201
  - [ ] Create with invalid name returns 400/422
  - [ ] Create with duplicate name returns 409
  - [ ] Create when limit exceeded returns 409
- [ ] Add tests for GET /collections
  - [ ] Returns user's collections only
  - [ ] Pagination works correctly
  - [ ] Sorting works correctly
- [ ] Add tests for GET /collections/{id}
  - [ ] Returns collection for owner
  - [ ] Returns 404 for non-existent
  - [ ] Returns 403 for non-owner
- [ ] Add tests for PUT /collections/{id}
  - [ ] Update succeeds for owner
  - [ ] Returns 409 for duplicate name
  - [ ] Returns 403 for non-owner
- [ ] Add tests for DELETE /collections/{id}
  - [ ] Delete succeeds for owner
  - [ ] Returns 403 for non-owner

## Technical Notes
- Use existing test infrastructure in `SuperDuperRescueHeads.Tests.Integration`
- Use in-memory database or test containers
```

---

## Issue 9: Add Integration Tests for CollectionRepository

**Title:** `[001] Add integration tests for CollectionRepository`

**Labels:** `testing`, `feature-001`, `priority-3`

**Body:**
```markdown
## Description
Add integration tests for CollectionRepository database operations.

## Acceptance Criteria
- [ ] Add tests for GetByIdAsync
- [ ] Add tests for GetByOwnerIdAsync (with pagination)
- [ ] Add tests for AddAsync
- [ ] Add tests for UpdateAsync
- [ ] Add tests for DeleteAsync (soft delete)
- [ ] Add tests for ExistsAsync
- [ ] Add tests for CountByOwnerIdAsync
- [ ] Add tests for ExistsByNameAndOwnerAsync
- [ ] Verify soft delete query filter works

## Technical Notes
- Follow existing patterns from ItemRepositoryTests
- Use in-memory SQLite for tests
```

---

## Summary

| # | Issue | Priority | Labels |
|---|-------|----------|--------|
| 1 | Implement ItemType value object | P1 | enhancement, feature-001 |
| 2 | Duplicate name validation | P1 | enhancement, feature-001 |
| 3 | Collection limit enforcement | P1 | enhancement, feature-001 |
| 4 | Pagination support | P2 | enhancement, feature-001 |
| 5 | Sorting support | P2 | enhancement, feature-001 |
| 6 | Usage indicators UI | P2 | enhancement, feature-001 |
| 7 | Unit tests for Collection domain | P3 | testing, feature-001 |
| 8 | Integration tests for API | P3 | testing, feature-001 |
| 9 | Integration tests for Repository | P3 | testing, feature-001 |

**Total: 9 issues** to complete the 001-collection-management feature.

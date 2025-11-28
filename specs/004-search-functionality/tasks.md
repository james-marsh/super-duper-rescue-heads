# Tasks: Search Functionality

**Input**: Design documents from `/specs/004-search-functionality/`
**Prerequisites**: Features 001 (Collection Management), 002 (Item Management), and 003 (Soft Delete & Recovery) must be complete
**Framework**: .NET 10 with .NET Aspire, Entity Framework Core 9.0, SQL Server Full-Text Search, Blazor Server

**Tests**: Following TDD approach - tests written FIRST, must FAIL before implementation

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

Based on Features 001-003 structure:
- **Domain**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Domain/Search/`
- **Infrastructure**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Infrastructure/Search/`
- **API**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Api/Endpoints/`
- **Web**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Web/Components/Pages/Search/`
- **Tests**: `SuperDuperRescueHeads/SuperDuperRescueHeads.Tests.{Type}/Search/`

---

## Phase 1: Setup (Infrastructure Dependencies)

**Purpose**: Install dependencies and create database migration for SQL Server Full-Text Search

- [ ] T001 Create Search directory in SuperDuperRescueHeads.Domain/
- [ ] T002 Create Search directory in SuperDuperRescueHeads.Infrastructure/
- [ ] T003 [P] Create Search directory in SuperDuperRescueHeads.Api/Endpoints/ (if not exists)
- [ ] T004 [P] Create Search directory in SuperDuperRescueHeads.Web/Components/Pages/
- [ ] T005 [P] Create Search directory in SuperDuperRescueHeads.Tests.Unit/
- [ ] T006 [P] Create Search directory in SuperDuperRescueHeads.Tests.Integration/
- [ ] T007 [P] Create Search directory in SuperDuperRescueHeads.Tests.UI/
- [ ] T008 [P] Create Search directory in SuperDuperRescueHeads.Tests.E2E/

---

## Phase 2: Foundational (Full-Text Search Infrastructure)

**Purpose**: Create SQL Server Full-Text index and base domain models

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T009 Create SearchScope enum in SuperDuperRescueHeads.Domain/Search/SearchScope.cs
- [ ] T010 Create FilterType enum in SuperDuperRescueHeads.Domain/Search/FilterType.cs
- [ ] T011 Create migration 004_AddFullTextSearch in SuperDuperRescueHeads.Infrastructure/Migrations/
- [ ] T012 Apply migration to create full-text catalog and index using dotnet ef database update

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Basic Search Across All Items (Priority: P1) 🎯 MVP

**Goal**: Users can search across all their items by keywords in name, notes, and attributes, with results ordered by relevance and paginated

**Independent Test**: Create 100 items across 5 collections. Search for "vintage". Verify results include all matching items regardless of collection, sorted by relevance, paginated with 20 items/page

### Tests for User Story 1 (TDD - Write Tests FIRST)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T013 [P] [US1] Unit test: SearchQuery.Validate() enforces search term 1-200 chars in SuperDuperRescueHeads.Tests.Unit/Search/SearchQueryTests.cs
- [ ] T014 [P] [US1] Unit test: SearchQuery.Validate() throws if Skip < 0 or Take not 1-100 in SuperDuperRescueHeads.Tests.Unit/Search/SearchQueryTests.cs
- [ ] T015 [P] [US1] Unit test: SearchResultPage.HasNextPage calculated correctly in SuperDuperRescueHeads.Tests.Unit/Search/SearchResultPageTests.cs
- [ ] T016 [P] [US1] Unit test: SearchService.SearchAsync validates query before executing in SuperDuperRescueHeads.Tests.Unit/Search/SearchServiceTests.cs
- [ ] T017 [P] [US1] Integration test: SearchRepository returns items matching search term via full-text index in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T018 [P] [US1] Integration test: Search results ordered by relevance (RANK) in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T019 [P] [US1] Integration test: Search excludes soft-deleted items (IsDeleted=1) in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T020 [P] [US1] Integration test: Pagination works correctly (Skip/Take) in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T021 [P] [US1] Integration test: GET /api/v1/search returns 200 with results in SuperDuperRescueHeads.Tests.Integration/Search/SearchEndpointsTests.cs
- [ ] T022 [P] [US1] Integration test: GET /api/v1/search returns 400 for empty search term in SuperDuperRescueHeads.Tests.Integration/Search/SearchEndpointsTests.cs
- [ ] T023 [P] [US1] E2E test: User can search items and see paginated results in SuperDuperRescueHeads.Tests.E2E/Search/SearchJourneyTests.cs

### Implementation for User Story 1

- [ ] T024 [P] [US1] Create SearchQuery record in SuperDuperRescueHeads.Domain/Search/SearchQuery.cs
- [ ] T025 [P] [US1] Create SearchResult record in SuperDuperRescueHeads.Domain/Search/SearchResult.cs
- [ ] T026 [P] [US1] Create SearchResultPage record in SuperDuperRescueHeads.Domain/Search/SearchResultPage.cs
- [ ] T027 [P] [US1] Create PaginationParameters record in SuperDuperRescueHeads.Domain/Search/PaginationParameters.cs
- [ ] T028 [US1] Create ISearchRepository interface in SuperDuperRescueHeads.Domain/Search/ISearchRepository.cs
- [ ] T029 [US1] Create ISearchService interface in SuperDuperRescueHeads.Domain/Search/ISearchService.cs
- [ ] T030 [US1] Implement SearchRepository.SearchItemsAsync using FREETEXTTABLE in SuperDuperRescueHeads.Infrastructure/Search/SearchRepository.cs
- [ ] T031 [US1] Implement SearchService.SearchAsync with validation and result mapping in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T032 [US1] Create SearchRequest DTO in SuperDuperRescueHeads.Api/Models/SearchRequest.cs
- [ ] T033 [US1] Create SearchResponse DTO in SuperDuperRescueHeads.Api/Models/SearchResponse.cs
- [ ] T034 [US1] Create SearchResultItemDto in SuperDuperRescueHeads.Api/Models/SearchResultItemDto.cs
- [ ] T035 [US1] Create SearchEndpoints.cs with GET /api/v1/search endpoint in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T036 [US1] Register ISearchRepository and ISearchService in SuperDuperRescueHeads.Api/Program.cs
- [ ] T037 [US1] Map SearchEndpoints in SuperDuperRescueHeads.Api/Program.cs
- [ ] T038 [US1] Create Search/Index.razor page with search input and results list in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T039 [US1] Add "Search" navigation link to NavMenu.razor in SuperDuperRescueHeads.Web/Components/Layout/NavMenu.razor

**Checkpoint**: Users can now search across all items with pagination - MVP complete!

---

## Phase 4: User Story 2 - Collection-Specific Search (Priority: P2)

**Goal**: Users can search within a specific collection only, with toggle to switch to global search

**Independent Test**: Create "Coins" collection with 100 items and "Stamps" with 100 items. Search for "gold" within Coins collection. Verify results include only Coins items.

### Tests for User Story 2 (TDD - Write Tests FIRST)

- [ ] T040 [P] [US2] Integration test: SearchRepository filters by CollectionId when specified in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T041 [P] [US2] Integration test: GET /api/v1/search?collectionId={id} returns only items from that collection in SuperDuperRescueHeads.Tests.Integration/Search/SearchEndpointsTests.cs
- [ ] T042 [P] [US2] Integration test: SearchQuery.Scope returns CollectionSpecific when CollectionId provided in SuperDuperRescueHeads.Tests.Unit/Search/SearchQueryTests.cs
- [ ] T043 [P] [US2] E2E test: User can toggle between global and collection-specific search in SuperDuperRescueHeads.Tests.E2E/Search/SearchJourneyTests.cs

### Implementation for User Story 2

- [ ] T044 [US2] Update SearchQuery to support CollectionId filtering in SuperDuperRescueHeads.Domain/Search/SearchQuery.cs
- [ ] T045 [US2] Update SearchRepository.SearchItemsAsync to filter by CollectionId in WHERE clause in SuperDuperRescueHeads.Infrastructure/Search/SearchRepository.cs
- [ ] T046 [US2] Update SearchEndpoints GET /search to accept collectionId query parameter in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T047 [US2] Add collection filter dropdown to Search/Index.razor in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T048 [US2] Add "Search All Collections" toggle button to Search/Index.razor in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor

**Checkpoint**: Users can now search within specific collections or globally

---

## Phase 5: User Story 3 - Filter Search Results (Priority: P3)

**Goal**: Users can filter search results by acquisition date range and custom attributes

**Independent Test**: Search for "coin" returning 200 results. Apply "Acquisition Date: 2020-2022" filter. Verify results reduce to only coins acquired in that date range.

### Tests for User Story 3 (TDD - Write Tests FIRST)

- [ ] T049 [P] [US3] Unit test: SearchFilter validates StartDate <= EndDate in SuperDuperRescueHeads.Tests.Unit/Search/SearchFilterTests.cs
- [ ] T050 [P] [US3] Integration test: SearchRepository filters by acquisition date range in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T051 [P] [US3] Integration test: Multiple filters applied correctly (AND logic) in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T052 [P] [US3] E2E test: User can apply date filter and see reduced results in SuperDuperRescueHeads.Tests.E2E/Search/SearchJourneyTests.cs

### Implementation for User Story 3

- [ ] T053 [P] [US3] Create SearchFilter record in SuperDuperRescueHeads.Domain/Search/SearchFilter.cs
- [ ] T054 [US3] Update SearchQuery to support Filters collection in SuperDuperRescueHeads.Domain/Search/SearchQuery.cs
- [ ] T055 [US3] Update SearchRepository.SearchItemsAsync to apply date range filters in WHERE clause in SuperDuperRescueHeads.Infrastructure/Search/SearchRepository.cs
- [ ] T056 [US3] Update SearchEndpoints GET /search to accept startDate and endDate query parameters in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T057 [US3] Create SearchFilters.razor component with date range pickers in SuperDuperRescueHeads.Web/Components/Pages/Search/SearchFilters.razor
- [ ] T058 [US3] Add SearchFilters component to Search/Index.razor in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T059 [US3] Add "Clear Filters" button to reset all filters in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor

**Checkpoint**: Users can now filter search results by date range

---

## Phase 6: User Story 4 - Search Suggestions and Auto-Complete (Priority: P4)

**Goal**: Users see auto-complete suggestions after typing 3+ characters

**Independent Test**: Start typing "vinta" in search box. Verify dropdown shows suggestions like "vintage camera", "vintage watch" based on existing item names.

### Tests for User Story 4 (TDD - Write Tests FIRST)

- [ ] T060 [P] [US4] Unit test: SearchService.GetSuggestionsAsync returns empty for prefix < 3 chars in SuperDuperRescueHeads.Tests.Unit/Search/SearchServiceTests.cs
- [ ] T061 [P] [US4] Integration test: SearchRepository.GetSuggestionsAsync returns up to 5 matching item names in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T062 [P] [US4] Integration test: GET /api/v1/search/suggestions returns suggestions array in SuperDuperRescueHeads.Tests.Integration/Search/SearchEndpointsTests.cs
- [ ] T063 [P] [US4] UI test: Suggestions dropdown appears after 3+ characters typed in SuperDuperRescueHeads.Tests.UI/Search/SearchBoxTests.cs

### Implementation for User Story 4

- [ ] T064 [US4] Add GetSuggestionsAsync to ISearchRepository in SuperDuperRescueHeads.Domain/Search/ISearchRepository.cs
- [ ] T065 [US4] Implement SearchRepository.GetSuggestionsAsync using LIKE prefix matching in SuperDuperRescueHeads.Infrastructure/Search/SearchRepository.cs
- [ ] T066 [US4] Implement SearchService.GetSuggestionsAsync with 3-char minimum in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T067 [US4] Add GET /api/v1/search/suggestions endpoint to SearchEndpoints.cs in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T068 [US4] Create SearchBox.razor component with auto-complete dropdown in SuperDuperRescueHeads.Web/Components/Pages/Search/SearchBox.razor
- [ ] T069 [US4] Add 300ms debounce to SearchBox suggestions API call in SuperDuperRescueHeads.Web/Components/Pages/Search/SearchBox.razor
- [ ] T070 [US4] Replace search input with SearchBox component in Search/Index.razor in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor

**Checkpoint**: Users now see auto-complete suggestions as they type

---

## Phase 7: User Story 5 - Recent Searches (Priority: P5)

**Goal**: Users can quickly access their last 5 search terms

**Independent Test**: Perform searches for "coin", "stamp", "camera". Clear search field. Click search box. Verify recent searches appear as quick options.

### Tests for User Story 5 (TDD - Write Tests FIRST)

- [ ] T071 [P] [US5] Unit test: UserSearchHistory stores max 5 searches per user in SuperDuperRescueHeads.Tests.Unit/Search/UserSearchHistoryTests.cs
- [ ] T072 [P] [US5] Integration test: SaveRecentSearchAsync adds search to history table in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T073 [P] [US5] Integration test: GetRecentSearchesAsync returns last 5 searches descending by date in SuperDuperRescueHeads.Tests.Integration/Search/SearchRepositoryTests.cs
- [ ] T074 [P] [US5] Integration test: GET /api/v1/search/recent returns user's recent searches in SuperDuperRescueHeads.Tests.Integration/Search/SearchEndpointsTests.cs

### Implementation for User Story 5

- [ ] T075 [US5] Create UserSearchHistory entity in SuperDuperRescueHeads.Domain/Search/UserSearchHistory.cs
- [ ] T076 [US5] Create UserSearchHistoryConfiguration in SuperDuperRescueHeads.Infrastructure/Data/Configurations/UserSearchHistoryConfiguration.cs
- [ ] T077 [US5] Add DbSet<UserSearchHistory> to ApplicationDbContext in SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T078 [US5] Add UserSearchHistory table to migration 004_AddFullTextSearch in SuperDuperRescueHeads.Infrastructure/Migrations/004_AddFullTextSearch.cs
- [ ] T079 [US5] Apply updated migration to database using dotnet ef database update
- [ ] T080 [US5] Add SaveRecentSearchAsync to ISearchService in SuperDuperRescueHeads.Domain/Search/ISearchService.cs
- [ ] T081 [US5] Add GetRecentSearchesAsync to ISearchService in SuperDuperRescueHeads.Domain/Search/ISearchService.cs
- [ ] T082 [US5] Implement SaveRecentSearchAsync in SearchService (keep only last 5) in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T083 [US5] Implement GetRecentSearchesAsync in SearchService in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T084 [US5] Add GET /api/v1/search/recent endpoint to SearchEndpoints.cs in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T085 [US5] Call SaveRecentSearchAsync after each search in SearchService.SearchAsync in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T086 [US5] Add recent searches dropdown to SearchBox.razor (shown when field focused, empty) in SuperDuperRescueHeads.Web/Components/Pages/Search/SearchBox.razor
- [ ] T087 [US5] Add "Clear History" button to recent searches dropdown in SuperDuperRescueHeads.Web/Components/Pages/Search/SearchBox.razor

**Checkpoint**: Users can now access recent searches for quick re-execution

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T088 [P] Add keyword highlighting to search results (bold matched terms) in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T089 [P] Add loading spinner during search execution in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T090 [P] Add empty state message "No results found" with suggestions in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T091 [P] Add error handling for search API failures in SuperDuperRescueHeads.Web/Components/Pages/Search/Index.razor
- [ ] T092 [P] Add structured logging (Serilog) for search queries in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T093 [P] Add Application Insights telemetry for search latency and result counts in SuperDuperRescueHeads.Infrastructure/Search/SearchService.cs
- [ ] T094 [P] Add OpenTelemetry trace for search request flow in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T095 [P] Style Search pages with Tailwind CSS (responsive, mobile-friendly) in SuperDuperRescueHeads.Web/Components/Pages/Search/
- [ ] T096 [P] Add XML comments to search domain models and services in SuperDuperRescueHeads.Domain/Search/
- [ ] T097 [P] Add OpenAPI documentation for search endpoints in SuperDuperRescueHeads.Api/Endpoints/SearchEndpoints.cs
- [ ] T098 [P] Create composite index (CollectionId, IsDeleted, CreatedAt) for search performance in migration
- [ ] T099 [P] Add full-text index maintenance script (weekly REORGANIZE) in SuperDuperRescueHeads.Infrastructure/Search/
- [ ] T100 Run all tests and verify 80%+ code coverage
- [ ] T101 Performance test: Verify search completes <2s for 5,000 items
- [ ] T102 Performance test: Verify suggestions return <200ms
- [ ] T103 Security review: Verify search queries are parameterized (no SQL injection)
- [ ] T104 Accessibility audit: Verify search UI keyboard navigable

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - create directories
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (Basic Search) is foundational for US2-5
  - US2 (Collection-Specific) can proceed in parallel with US3-5 after US1
  - US3 (Filters) can proceed in parallel with US2, US4-5 after US1
  - US4 (Suggestions) can proceed in parallel with US2-3, US5 after US1
  - US5 (Recent Searches) can proceed in parallel with US2-4 after US1
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundational - BLOCKS US2-5
- **User Story 2 (P2)**: Depends on US1 - Can run parallel with US3-5
- **User Story 3 (P3)**: Depends on US1 - Can run parallel with US2, US4-5
- **User Story 4 (P4)**: Depends on US1 - Can run parallel with US2-3, US5
- **User Story 5 (P5)**: Depends on US1 - Can run parallel with US2-4

### Parallel Opportunities

- After US1 completes, US2-5 can all run in parallel
- All tests within a user story marked [P] can run in parallel
- All Polish tasks marked [P] can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (directory structure)
2. Complete Phase 2: Foundational (SQL Full-Text index, base models)
3. Complete Phase 3: User Story 1 (Basic Search)
4. **STOP and VALIDATE**: Test basic search → view results → pagination
5. Deploy/demo if ready

### Incremental Delivery

1. Add US1 (Basic Search) → Test → Deploy (users can now search across all items)
2. Add US2 (Collection-Specific) → Test → Deploy (users can narrow to specific collection)
3. Add US3 (Filters) → Test → Deploy (power users can filter by date/attributes - MVP complete!)
4. Add US4 (Suggestions) → Test → Deploy (improved UX with auto-complete)
5. Add US5 (Recent Searches) → Test → Deploy (convenience feature for repeat searches)
6. Add Polish → Test → Deploy (production-ready)

---

## Summary

- **Total Tasks**: 104 tasks
- **Test Tasks**: 26 tasks (25% - following TDD approach)
- **Implementation Tasks**: 78 tasks
- **Parallel Opportunities**: 68 tasks marked [P] can run concurrently
- **User Stories**: 5 independent stories + Polish
- **MVP Scope**: Phase 1 + Phase 2 + Phase 3 (US1) = 39 tasks

**Task Distribution by User Story**:
- US1 (Basic Search): 27 tasks (11 tests + 16 implementation)
- US2 (Collection-Specific Search): 9 tasks (4 tests + 5 implementation)
- US3 (Filter Results): 11 tasks (4 tests + 7 implementation)
- US4 (Suggestions): 11 tasks (4 tests + 7 implementation)
- US5 (Recent Searches): 17 tasks (4 tests + 13 implementation)
- Polish: 17 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- All tests must FAIL before writing implementation (TDD Red-Green-Refactor)
- This feature extends existing Item aggregate from Feature 002
- SQL Server Full-Text Search with CHANGE_TRACKING = AUTO for automatic index updates
- Performance targets: <2s for 5,000 items, <500ms index updates, <200ms suggestions
- Follow quickstart.md for detailed implementation guidance

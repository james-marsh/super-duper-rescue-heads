# Implementation Plan: Search Functionality

**Branch**: `004-search-functionality` | **Date**: 2025-11-27 | **Spec**: [spec.md](./spec.md)

## Summary

Adds full-text search across items with collection-specific and global search modes, filtering by date range and attributes, relevance ranking, and search suggestions. Users can search across all their items or within specific collections, with results paginated and ordered by relevance. Search index updates automatically via domain events when items are created, updated, or restored.

## Technical Context

**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Entity Framework Core 9.0, SQL Server Full-Text Search (initial) or Azure Cognitive Search (future), Blazor Server
**Storage**: Azure SQL Database (production), SQL Server LocalDB (development) - existing from Features 001-003
**Testing**: TUnit, bUnit (Blazor components), Playwright (E2E), FluentAssertions
**Target Platform**: Azure Container Apps via .NET Aspire orchestration
**Project Type**: Web application - extends Features 001-003 (Collection Management, Item Management, Soft Delete & Recovery)
**Performance Goals**: P95 search latency <2s for 5,000 items, <500ms index updates, <200ms auto-complete suggestions
**Constraints**: 50,000 items per user max (Feature 001 limit), exclude soft-deleted items, case-insensitive search, partial word matching
**Scale/Scope**: Typical 500-1,000 items per user, max 50,000 items per user, pagination 20 items/page

## Constitution Check

### Test-Driven Development
✅ **PASS** - TDD Red-Green-Refactor cycle enforced

**Approach**:
- Unit tests FIRST for search service methods (search query parsing, relevance scoring)
- Integration tests FIRST for search repository with SQL Full-Text indexes
- Integration tests FIRST for search index updates via domain event handlers
- Component tests (bUnit) FIRST for search UI components
- E2E tests (Playwright) FIRST for complete search user journeys

**Test Coverage Requirements**:
- Search across all collections returns correct results ordered by relevance
- Collection-specific search filters correctly
- Filtering by date range and attributes works
- Search excludes soft-deleted items
- Empty search queries handled gracefully
- Special characters and Unicode handled correctly
- Pagination works with large result sets
- Search suggestions appear after 3 characters
- Recent searches tracked per user

### Code Quality & Standards
✅ **PASS** - Follows .NET 10, C# 14, DDD, SOLID, EF Core, Blazor, Tailwind CSS standards

**DDD Alignment**:
- **Bounded Context**: Search belongs to "Item Search & Discovery" bounded context, consumes events from "Item Management" context (Feature 002)
- **Aggregates**: SearchQuery (search term, filters, pagination), SearchResult (item data, relevance score, highlights)
- **Value Objects**: SearchTerm, SearchFilter, PaginationParameters, RelevanceScore
- **Domain Services**: ISearchService (search execution, relevance ranking)
- **Repositories**: ISearchRepository (search index queries), extends IItemRepository from Feature 002
- **Domain Events**: Subscribe to ItemCreatedEvent, ItemUpdatedEvent, ItemRestoredEvent (Feature 002/003) to update search index
- **Ubiquitous Language**: "Search query", "relevance", "filter", "suggestion", "index update"

**EF Core Strategy**:
- Leverage SQL Server Full-Text Search via raw SQL queries or views
- Use `AsNoTracking()` for read-only search queries (performance)
- Project only needed columns for search results (name, collection, date, thumbnail URL)
- Implement `Skip()` and `Take()` for pagination
- Add full-text indexes on Items.Name, Items.Notes, Items.Attributes columns
- Connection resiliency for transient failures

**Architecture**:
- No new projects - extends existing SuperDuperRescueHeads.Domain, .Infrastructure, .Api, .Web
- Search service in Domain layer (ISearchService interface)
- Search repository in Infrastructure layer (implements search index queries)
- Search endpoints in Api layer (GET /api/v1/search, GET /api/v1/search/suggest

ions)
- Search UI pages in Web layer (Blazor Server - Search/Index.razor)

### Comprehensive Testing
✅ **PASS** - Unit, integration, component, E2E, contract tests planned

**Test Strategy**:
- **Unit Tests** (TUnit): SearchService logic, query parsing, filter composition, relevance scoring algorithms
- **Integration Tests** (TUnit): SearchRepository queries against actual SQL Server Full-Text indexes, domain event handlers updating index
- **Component Tests** (bUnit): Search box component, filter components, results list component, suggestions dropdown
- **E2E Tests** (Playwright): Complete search user journey (global search, collection search, filtering, pagination)
- **Contract Tests**: Verify search API endpoint contracts match OpenAPI spec
- **Performance Tests**: Search 5,000 items completes in <2s, index updates <500ms, suggestions <200ms

**Coverage Targets**: 80%+ code coverage, all user stories (US1-US5) independently testable

### User Experience Consistency
✅ **PASS** - Consistent with existing UI patterns from Features 001-002, responsive design

**UI Patterns**:
- Search box in navbar (global search) follows existing navigation patterns
- Search results page layout similar to item list pages (Feature 002)
- Filter sidebar follows collection filter patterns (Feature 001)
- Blazor Server components reuse existing Tailwind CSS utility classes
- Mobile-responsive search UI (Tailwind responsive utilities)
- Loading states with spinners (consistent with existing pages)
- Error messages follow existing error display patterns

### Performance Requirements
✅ **PASS** - Performance goals defined, SQL Full-Text indexes, pagination, async operations

**Performance Strategy**:
- SQL Server Full-Text indexes on searchable columns (Name, Notes, Attributes)
- Filtered indexes for common search patterns
- Asynchronous search queries (EF Core async methods)
- Pagination to limit result set size (20 items/page)
- Search result projection (only needed columns, not entire Item aggregate)
- Domain event handlers update index asynchronously (background processing)
- Auto-complete debounce (300ms) to reduce query frequency
- Consider Azure Cognitive Search for future if SQL Full-Text performance inadequate

**Benchmarks**:
- Typical search (100-500 items): <500ms
- Large search (5,000 items): <1.5s
- Maximum search (50,000 items): <2s
- Index update: <500ms
- Suggestions: <200ms

### Simplicity & Maintainability
✅ **PASS** - Simple SQL Full-Text approach initially, can upgrade to Azure Cognitive Search if needed

**Complexity Justification**:
- Start with SQL Server Full-Text Search (built-in, no extra infrastructure)
- Upgrade to Azure Cognitive Search only if performance or feature requirements demand it (suggestions, relevance tuning)
- No new projects or infrastructure initially
- Reuse existing domain events from Features 002-003
- Search index updates via standard domain event handler pattern

### Security, Resilience & Observability
✅ **PASS** - Authorization via existing auth, retry policies, structured logging, telemetry

**Security**:
- Search restricted to authenticated user's items only (existing auth from Feature 001)
- Search queries parameterized to prevent SQL injection
- No search across other users' items (enforced via UserId filter)

**Resilience**:
- EF Core connection resiliency (retry policies for transient failures)
- Graceful degradation if search service unavailable (show error, allow retry)
- Domain event retries if index update fails (via Polly policies)

**Observability**:
- Structured logging (Serilog) for search queries, index updates, errors
- Application Insights telemetry (search latency, result counts, failure rates)
- OpenTelemetry traces for search request flow (API → Service → Repository)
- Hangfire dashboard (if using background jobs for index updates)

### Cost Optimization
✅ **PASS** - SQL Server Full-Text included with Azure SQL, no additional cost initially

**Cost Strategy**:
- SQL Server Full-Text Search included with Azure SQL Database (no extra cost)
- Azure Cognitive Search deferred until needed (free tier: 10,000 docs, paid tier if exceed)
- Search queries use existing database connection pool
- No additional infrastructure costs for MVP
- Monitor search query costs via Application Insights (query frequency, data transfer)
- Consider Azure Cognitive Search Standard tier (~$250/month) only if SQL Full-Text insufficient

**Future Considerations**:
- If Azure Cognitive Search needed, use free tier initially (10,000 docs = ~10 users at 1,000 items each)
- Upgrade to Standard tier only if free tier capacity exceeded
- Monitor search costs via Azure Cost Management

**Gate Status: ✅ ALL GATES PASSED**

## Project Structure

### Documentation (this feature)
```text
specs/004-search-functionality/
├── plan.md              # This file
├── research.md          # Phase 0 output (search technology evaluation)
├── data-model.md        # Phase 1 output (search entities, index schema)
├── quickstart.md        # Phase 1 output (implementation guide)
├── contracts/           # Phase 1 output (OpenAPI specs for search endpoints)
│   └── search-api.yaml
├── checklists/
│   └── requirements.md  # Specification quality checklist (completed)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created yet)
```

### Source Code (extends existing solution)
```text
SuperDuperRescueHeads/
├── SuperDuperRescueHeads.Domain/
│   ├── Search/                          # NEW - Search domain
│   │   ├── ISearchService.cs           # Search service interface
│   │   ├── ISearchRepository.cs        # Search repository interface
│   │   ├── SearchQuery.cs              # Aggregate: search query
│   │   ├── SearchResult.cs             # Aggregate: search result
│   │   ├── SearchTerm.cs               # Value object
│   │   ├── SearchFilter.cs             # Value object
│   │   └── PaginationParameters.cs     # Value object
│   └── Items/                          # EXISTING - from Feature 002
│       └── (existing Item aggregate, events)
│
├── SuperDuperRescueHeads.Infrastructure/
│   ├── Search/                          # NEW - Search infrastructure
│   │   ├── SearchService.cs            # Search service implementation
│   │   ├── SearchRepository.cs         # Search repository with SQL Full-Text
│   │   └── ItemSearchIndexHandler.cs   # Domain event handler for index updates
│   ├── Data/
│   │   └── Configurations/
│   │       └── ItemSearchConfiguration.cs  # EF Core full-text index config
│   └── Migrations/
│       └── 004_AddFullTextSearch.cs    # NEW migration
│
├── SuperDuperRescueHeads.Api/
│   ├── Endpoints/
│   │   └── SearchEndpoints.cs          # NEW - Search API endpoints
│   └── Models/
│       ├── SearchRequest.cs            # NEW - Search request DTO
│       ├── SearchResponse.cs           # NEW - Search response DTO
│       └── SearchResultItemDto.cs      # NEW - Search result item DTO
│
└── SuperDuperRescueHeads.Web/
    └── Components/Pages/
        └── Search/
            ├── Index.razor             # NEW - Search results page
            ├── SearchBox.razor         # NEW - Search input component
            ├── SearchFilters.razor     # NEW - Filter sidebar component
            └── SearchResults.razor     # NEW - Results list component

tests/
├── SuperDuperRescueHeads.Tests.Unit/
│   └── Search/
│       ├── SearchServiceTests.cs       # NEW - Search service unit tests
│       └── SearchQueryTests.cs         # NEW - Search query parsing tests
│
├── SuperDuperRescueHeads.Tests.Integration/
│   └── Search/
│       ├── SearchRepositoryTests.cs    # NEW - Search repository integration tests
│       └── SearchIndexUpdateTests.cs   # NEW - Index update via events tests
│
├── SuperDuperRescueHeads.Tests.UI/
│   └── Search/
│       ├── SearchBoxTests.cs           # NEW - Search box component tests
│       └── SearchResultsTests.cs       # NEW - Search results component tests
│
└── SuperDuperRescueHeads.Tests.E2E/
    └── Search/
        └── SearchJourneyTests.cs       # NEW - End-to-end search journey tests
```

**Structure Decision**: Extends existing .NET Aspire solution structure from Features 001-003. Search functionality integrated into existing Domain, Infrastructure, Api, and Web projects. No new projects required - follows established patterns.

## Complexity Tracking

No constitution violations. All complexity is justified:

- **SQL Server Full-Text Search**: Built-in capability of Azure SQL Database, no extra infrastructure. Justified by performance requirements (<2s for 5,000 items). Simpler alternative (LIKE queries) would be too slow for large item counts.
- **Domain Event Handlers**: Reuses existing pattern from Features 002-003. Justified by need for automatic search index updates when items change. Simpler alternative (manual index updates) would create data consistency issues.
- **Search UI Components**: Reuses Blazor Server and Tailwind CSS from existing features. Justified by need for interactive search experience with filtering and pagination. Simpler alternative (static search page) would not meet user experience requirements.

**Status: No unjustified complexity**

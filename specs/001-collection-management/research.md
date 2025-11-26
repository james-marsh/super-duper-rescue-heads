# Research: Core Collection Management

**Feature**: 001-collection-management | **Date**: 2025-11-24

## Overview

This document consolidates research findings for technical decisions made during the implementation planning of the Core Collection Management feature. All decisions align with the project constitution and leverage .NET 10 ecosystem best practices.

## Technology Decisions

### 1. Application Framework: .NET Aspire

**Decision**: Use .NET Aspire for distributed application orchestration

**Rationale**:
- Constitution mandates Aspire for cloud-ready, distributed applications
- Provides built-in service discovery, telemetry, and orchestration
- Standardizes configuration across microservices
- Includes OpenTelemetry integration out-of-the-box
- Simplifies local development with Aspire dashboard
- Azure Container Apps deployment model reduces operational complexity

**Alternatives Considered**:
- **Plain ASP.NET Core**: Lacks orchestration and service discovery; manual configuration required
- **Custom Docker Compose**: More configuration overhead; no built-in telemetry; less aligned with Azure deployment

**References**:
- [.NET Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- Constitution Principle II: Distributed Applications

### 2. UI Framework: Blazor Server

**Decision**: Blazor Server for web UI with interactive components

**Rationale**:
- Constitution mandates Blazor as standard UI framework
- Blazor Server chosen for low-latency interactive requirements (<200ms response target)
- Eliminates need for separate JavaScript SPA framework
- C# throughout the stack improves maintainability
- SignalR handles real-time communication automatically
- Smaller initial payload vs Blazor WebAssembly
- Server-side rendering improves SEO for public pages

**Alternatives Considered**:
- **Blazor WebAssembly**: Larger initial payload; would exceed 2s page load target; better for offline scenarios (not required for this feature)
- **Blazor United (Auto)**: Unnecessary complexity for simple CRUD operations; hybrid mode reserved for features requiring both client and server benefits

**Best Practices**:
- Component-based architecture with single responsibility
- Cascading parameters for shared state
- Two-way binding with `@bind` directive
- Proper lifecycle management (`OnInitializedAsync`, `OnParametersSetAsync`)
- Avoid excessive JavaScript interop

**References**:
- [Blazor Server documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models#blazor-server)
- Constitution Principle II: UI Framework

### 3. Styling: Tailwind CSS

**Decision**: Tailwind CSS utility-first framework for styling

**Rationale**:
- Constitution mandates Tailwind CSS as standard
- Utility-first approach enables rapid development
- Prevents CSS bloat with PurgeCSS removing unused classes
- Consistent design system through configuration
- Mobile-first responsive design built-in
- Dark mode support with utilities
- Reduces custom CSS and component-specific styles

**Best Practices**:
- Use utility classes directly in Razor components
- Configure design tokens in `tailwind.config.js` (colors, spacing, typography)
- Use `@apply` only for patterns appearing 3+ times
- Leverage Tailwind breakpoints (`sm:`, `md:`, `lg:`, `xl:`, `2xl:`)
- Avoid inline styles

**References**:
- [Tailwind CSS documentation](https://tailwindcss.com/docs)
- Constitution Principle IV: Styling Standards

### 4. Data Access: Entity Framework Core with Repository Pattern

**Decision**: EF Core for data access with repository pattern for aggregate roots

**Rationale**:
- Constitution requires EF Core for data access
- Repository pattern mandated by DDD for aggregate persistence
- Provides abstraction over EF Core, enabling easier testing
- DbContext acts as Unit of Work for transaction management
- Code-first migrations enable version-controlled schema evolution
- Connection resiliency with retry logic for transient failures
- Compiled queries for frequently executed operations

**Best Practices**:
- One repository per aggregate root (Collection)
- Use `AsNoTracking()` for read-only queries
- Projection with `Select()` to load only needed columns
- Eager loading with `Include()` to avoid N+1 queries
- Pagination with `Skip()` and `Take()`
- Index foreign keys and frequently queried columns
- Scoped DbContext lifetime (one per request)

**Alternatives Considered**:
- **Dapper**: More performant for read-heavy scenarios but lacks change tracking and migrations; EF Core performance sufficient for requirements (P95 <200ms achievable)
- **Direct ADO.NET**: Too low-level; violates DDD abstraction; harder to test

**References**:
- [EF Core best practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- Constitution Principle II: Data Access & Entity Framework Core

### 5. Domain-Driven Design Architecture

**Decision**: DDD layered architecture with clear bounded context for Collection Management

**Rationale**:
- Constitution mandates DDD for business domain alignment
- Bounded context: "Collection Management" (manages collections lifecycle)
- Collection as aggregate root with CollectionName and ItemType as value objects
- Ubiquitous language: Collection, Item Type, User (Owner)
- Domain layer isolated from infrastructure dependencies
- Rich domain model with business logic in domain objects

**Bounded Context Map**:
```
[Collection Management Context]
  - Aggregates: Collection (root)
  - Value Objects: CollectionName, ItemType, Description
  - Repositories: ICollectionRepository
  - Domain Services: None (simple CRUD)
  - Domain Events: CollectionCreated, CollectionUpdated, CollectionDeleted

Future Context Relationships (not in this feature):
  - [Item Management Context]: Customer-Supplier (collections provide context for items)
  - [Sharing Context]: Customer-Supplier (collections are shared entities)
  - [Search Context]: Anti-Corruption Layer (search has own model)
```

**Aggregate Design**:
- **Collection (Aggregate Root)**:
  - Enforces invariants: unique collection name per user, max 100 collections per user
  - Encapsulates validation logic
  - Publishes domain events for lifecycle changes
- **Value Objects**:
  - CollectionName: Immutable, validated (max length, no empty strings)
  - ItemType: Immutable, predefined types (extensible in Feature 005)
  - Description: Optional, immutable

**Alternatives Considered**:
- **Anemic Domain Model**: Violates constitution; places business logic in services rather than domain objects
- **Transaction Script**: Too procedural; doesn't scale for complex business rules

**References**:
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- Constitution Principle II: Domain-Driven Design (DDD)

### 6. Testing Strategy: Multi-Layered with TUnit, bUnit, Playwright

**Decision**: Comprehensive testing with TUnit (unit/integration), bUnit (UI components), Playwright (E2E)

**Rationale**:
- Constitution mandates multi-layered testing
- **TUnit**: Modern, high-performance test framework; faster than xUnit/NUnit; better async support
- **bUnit**: Specialized for Blazor component testing; supports component rendering, parameter binding, lifecycle events
- **Playwright**: Cross-browser E2E testing (Chromium, Firefox, WebKit); stable selectors; screenshot/video capture
- **FluentAssertions**: Readable, expressive assertions
- **Coverlet**: Code coverage with 80% minimum threshold

**Test Coverage**:
- Unit tests: Domain logic (Collection aggregate, value objects), services
- Integration tests: API endpoints, database repositories (with test containers or in-memory provider)
- UI component tests: Blazor components (rendering, interactions, parameter binding)
- E2E tests: Critical user journeys (create collection, view collections list)
- Contract tests: API request/response validation

**Alternatives Considered**:
- **xUnit/NUnit**: Widely used but slower; TUnit offers better performance and modern features
- **Selenium**: Older E2E framework; Playwright provides better developer experience and cross-browser support

**References**:
- [TUnit documentation](https://github.com/thomhurst/TUnit)
- [bUnit documentation](https://bunit.dev/)
- [Playwright .NET documentation](https://playwright.dev/dotnet/)
- Constitution Principle III: Comprehensive Testing Standards

### 7. API Design: ASP.NET Core Minimal APIs with OpenAPI

**Decision**: Minimal APIs for RESTful endpoints with OpenAPI/Swagger documentation

**Rationale**:
- Constitution requires RESTful API design
- Minimal APIs reduce boilerplate vs MVC controllers
- OpenAPI auto-generation for API documentation
- Follows REST conventions: `GET /api/collections`, `POST /api/collections`, `PUT /api/collections/{id}`, `DELETE /api/collections/{id}`
- Problem Details (RFC 7807) for standardized error responses
- Versioning via URL (`/api/v1/collections`)

**Endpoint Design**:
- `GET /api/collections`: List all collections for authenticated user (paginated)
- `GET /api/collections/{id}`: Get single collection by ID
- `POST /api/collections`: Create new collection
- `PUT /api/collections/{id}`: Update collection (full update)
- `PATCH /api/collections/{id}`: Partial update (optional, not in MVP)
- `DELETE /api/collections/{id}`: Delete collection (hard delete, soft delete in Feature 003)

**Alternatives Considered**:
- **MVC Controllers**: More boilerplate; Minimal APIs sufficient for simple CRUD operations
- **GraphQL**: Over-engineering for simple CRUD; adds complexity without clear benefit

**References**:
- [Minimal APIs documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Problem Details (RFC 7807)](https://www.rfc-editor.org/rfc/rfc7807)
- Constitution Principle IV: API Design Standards

### 8. Authentication & Authorization: ASP.NET Core Identity

**Decision**: ASP.NET Core Identity for user management with policy-based authorization

**Rationale**:
- Constitution mandates Identity for authentication
- Built-in user management, password hashing, token generation
- Integrates with Entity Framework Core
- Policy-based authorization more flexible than role-based
- Supports OAuth2/OpenID Connect for external providers (future)

**Authorization Policies**:
- `CollectionOwnerPolicy`: Users can only access their own collections
- Implemented via custom authorization handler checking `UserId` claim matches collection owner

**Alternatives Considered**:
- **IdentityServer/Duende**: Over-engineering for single application; reserved for multi-tenant or OAuth provider scenarios
- **JWT only**: Lacks user management features; Identity provides complete solution

**References**:
- [ASP.NET Core Identity documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- Constitution Principle VII: Authentication & Authorization

### 9. Observability: Serilog + OpenTelemetry + Application Insights

**Decision**: Structured logging with Serilog, distributed tracing with OpenTelemetry, monitoring with Application Insights

**Rationale**:
- Constitution mandates comprehensive observability
- **Serilog**: Structured logging with properties (not string interpolation); sinks to console (dev) and Application Insights (prod)
- **OpenTelemetry**: Distributed tracing across services; W3C Trace Context propagation; Aspire dashboard integration
- **Application Insights**: Azure-native monitoring, telemetry, performance tracking, alerts

**Logging Best Practices**:
- Log levels: Information+ in production, Debug in development
- Structured properties: `_logger.Information("User {UserId} created collection {CollectionId}", userId, collectionId)`
- Correlation IDs for request tracking
- Enrichers: Environment, machine name, thread ID

**Metrics**:
- Built-in ASP.NET Core meters (request rate, duration, errors)
- Custom metrics: Collections created/updated/deleted per second, user collection count

**References**:
- [Serilog documentation](https://serilog.net/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- Constitution Principle VII: Observability

### 10. Resilience: Polly for Retry and Circuit Breaker

**Decision**: Polly resilience policies for database operations and external dependencies

**Rationale**:
- Constitution mandates resilience policies
- Retry with exponential backoff for transient database failures
- Circuit breaker to prevent cascading failures
- Timeout policy for long-running operations
- Integrates with `IHttpClientFactory`

**Policies**:
- **Database Retry**: 3 retries with exponential backoff (2^attempt seconds) for transient SQL errors
- **Circuit Breaker**: Open after 5 consecutive failures; stays open for 1 minute
- **Timeout**: 30-second timeout for database operations

**References**:
- [Polly documentation](https://www.thepollyproject.org/)
- Constitution Principle VII: Resilience

## Security Considerations

### Input Validation
- Server-side validation using FluentValidation
- Data annotations on DTOs: `[Required]`, `[StringLength]`, `[RegularExpression]`
- Sanitize user input to prevent XSS
- Validate collection name length, allowed characters

### OWASP Top 10 Mitigations
- **Injection**: Parameterized queries (EF Core), input validation
- **Broken Authentication**: ASP.NET Core Identity, strong password policies
- **Sensitive Data Exposure**: HTTPS everywhere, no PII in logs
- **XSS**: Blazor auto-escapes by default; never use `@((MarkupString)userInput)`
- **Broken Access Control**: Policy-based authorization on every request
- **Security Misconfiguration**: Remove debug info in production, disable directory browsing
- **Insecure Deserialization**: Validate input before deserialization
- **Using Components with Known Vulnerabilities**: Dependabot for automated dependency updates
- **Insufficient Logging**: Log security events (failed auth, unauthorized access)

### Secrets Management
- **Development**: User Secrets (`dotnet user-secrets`)
- **Production**: Azure Key Vault for connection strings and secrets
- Never commit secrets to source control

## Performance Optimizations

### Database Performance
- Indexes on foreign keys: `UserId` (collection owner)
- Index on collection name for duplicate detection
- Use `AsNoTracking()` for read-only list queries
- Pagination for collection lists (default 20 per page)
- Compiled queries for frequently executed operations

### Caching Strategy
- Response caching headers for static assets
- Consider Redis distributed cache for user collection counts (implement if performance testing shows need)
- Blazor component state caching in browser storage

### API Performance
- Minimal data transfer: Project only needed fields in list queries
- Compression enabled for responses (gzip)
- Async/await for all I/O operations

## Cost Optimization

### Development Environment
- Azure SQL serverless tier (auto-pause after 1 hour idle)
- Auto-shutdown dev environments: 6 PM-8 AM weekdays, off weekends
- Aspire local orchestration to minimize cloud resource usage during development

### Production Environment
- Azure Container Apps with consumption plan (pay-per-request)
- Application Insights adaptive sampling (5 items/sec) to control telemetry costs
- Azure SQL provisioned tier with right-sized DTUs based on load testing
- Cost allocation tags: `Environment=Production`, `Owner=TeamName`, `Project=SuperDuperRescueHeads`

### Monitoring
- Weekly Azure Cost Management review
- Budget alerts at 50%, 75%, 90% thresholds
- Review Azure Advisor cost recommendations

## Deployment Strategy

### Infrastructure as Code
- Bicep templates for Azure resources (SQL Database, Container Apps, Key Vault, Application Insights)
- Aspire AppHost models local and cloud orchestration

### CI/CD Pipeline
- GitHub Actions workflow:
  1. Build (restore, compile)
  2. Unit tests (TUnit)
  3. Integration tests (TUnit + WebApplicationFactory)
  4. UI component tests (bUnit)
  5. Contract tests
  6. Coverage report (Coverlet, 80% threshold)
  7. E2E tests on pre-merge (Playwright, critical paths)
  8. Deploy to staging (Azure Container Apps)
  9. Deploy to production (manual approval required)

### Branching Strategy
- GitHub Flow: Feature branches from `main`, PR for review, merge after approval
- Branch protection: Require PR approval, status checks, linear history
- Short-lived branches: Merge within 3 days

## Documentation Requirements

### Architecture Diagrams
- **C4 Context Diagram**: Show system boundaries, users (collection owners), external systems (authentication provider)
- **C4 Container Diagram**: Blazor Web UI, API, SQL Database, Aspire AppHost
- **Bounded Context Map**: Collection Management context relationships with future contexts (Item Management, Sharing, Search)
- **Aggregate Diagram**: Collection aggregate root, value objects (CollectionName, ItemType), invariants

### ADR (Architecture Decision Records)
- ADR-001: Use .NET Aspire for distributed application orchestration
- ADR-002: Use Blazor Server for low-latency interactive UI
- ADR-003: Use DDD layered architecture with bounded contexts
- ADR-004: Use Repository pattern for Collection aggregate

Diagrams and ADRs to be created in `/docs/architecture/` directory.

## Summary

All technical decisions align with the project constitution and leverage modern .NET 10 ecosystem capabilities. The chosen stack provides a solid foundation for scalable, maintainable, and performant collection management functionality. No technology choices require further clarification—all decisions are documented with clear rationale and alternatives considered.

**Next Steps**: Proceed to Phase 1 (Design & Contracts) to create data models, API contracts, and quickstart guide.

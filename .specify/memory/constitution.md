<!--
  SYNC IMPACT REPORT
  Version Change: 1.9.1 → 1.10.0 (Added Azure Well-Architected Framework alignment: Cost Optimization & enhanced Disaster Recovery)

  NEW Principle:
  - Principle VIII: Cost Optimization & Financial Management (MAJOR ADDITION - addresses critical gap in Azure Well-Architected Framework)

  Enhanced Principles:
  - Principle VII (Security, Resilience & Observability): Added comprehensive Disaster Recovery & Business Continuity section

  New Sections in Principle VIII - Cost Optimization:
  - Azure Cost Management & Monitoring: Budgets, alerts, cost allocation tags, anomaly detection, showback/chargeback, analysis cadence (weekly/monthly/quarterly/yearly)
  - Resource Right-Sizing: App Service plans, Azure Container Apps (CPU/memory limits), Azure Functions, database sizing (Azure SQL serverless, Cosmos DB autoscale), container resource limits in Aspire
  - Auto-Scaling Strategy: Cost-aware horizontal scaling, scaling rules (scale out/in thresholds), scheduled scaling for dev/test, Aspire replica configuration
  - Reserved Capacity & Commitments: When to use RIs (1-year/3-year), cost savings (20-60%), utilization tracking (>90% target), services eligible (SQL, Cosmos DB, App Service, Redis)
  - Development & Non-Production Cost Controls: Auto-shutdown policies (6 PM-8 AM weekdays, off weekends), lower-tier services, shared resources, ephemeral PR environments, Azure DevTest Labs
  - Storage Optimization: Blob storage tiers (hot/cool/archive), lifecycle management automation, compression (70-90% savings), deduplication, database storage monitoring
  - Serverless vs Always-On Decision Matrix: When to use Azure Functions (event-driven, <100 req/min, <5 min execution) vs App Service/Container Apps (consistent traffic, low latency, stateful)
  - Data Transfer & Network Costs: Cross-region transfer minimization, CDN usage for static assets, API Gateway/Front Door cost-benefit evaluation, inbound free/outbound charged
  - Observability Cost Management: Application Insights adaptive sampling (5 items/sec, 80-95% cost savings), log levels (Information+ in production), Serilog sink strategy, OpenTelemetry trace sampling (10-20%)
  - Cost Optimization Checklist: Monthly review checklist (10 items), cost gates in CI/CD (estimate required for PRs adding Azure resources, cost impact documentation, tech lead approval)

  New Sections in Principle VII - Disaster Recovery & Business Continuity:
  - Recovery Objectives: RPO ≤15 minutes (production databases), RTO ≤1 hour (critical services), MTTR ≤30 minutes targets
  - Backup Strategy:
    - Database backups (automated daily, PITR 7-35 days, long-term retention: weekly 3 months/monthly 1 year/annual 7 years, encrypted, quarterly restore testing)
    - Application state (IaC in source control, Key Vault metadata backup, Container Registry geo-replication)
    - Blob storage (soft delete 14-day, versioning, GRS/GZRS/RA-GRS)
  - Multi-Region Deployment: Active-passive (manual/automatic failover), active-active (Azure Front Door/Traffic Manager, multi-region writes for Cosmos DB), database geo-replication, quarterly failover testing, data consistency strategies
  - Business Continuity Plan: Incident response team roles (incident commander, technical lead, communications), runbooks for common failures (database, region outage, security breach), communication plan, quarterly DR drills, blameless postmortems
  - Data Retention & Compliance: GDPR/CCPA compliance (right to erasure, data portability, consent management), retention policies per data type, audit logs (1 year minimum, immutable storage), legal hold procedures

  Rationale Updates:
  - Principle VII: Added disaster recovery rationale (business continuity, data protection, comprehensive backup strategies protect against data loss and extended downtime)
  - Principle VIII: Cloud costs can spiral without active management; cost optimization maximizes business value through right-sizing, waste elimination, reserved capacity, monitoring; Aspire's container architecture enables granular resource allocation and scale-to-zero; uncontrolled costs threaten project viability

  Azure Well-Architected Framework Alignment:
  - ✅ Security: Already comprehensive (Principle VII)
  - ✅ Reliability: Already comprehensive (Principle VII - Resilience & DR)
  - ✅ Performance Efficiency: Already comprehensive (Principle V)
  - ✅ Operational Excellence: Already comprehensive (Principle VII - Observability, CI/CD)
  - ✅ Cost Optimization: NOW COMPREHENSIVE (NEW Principle VIII)
  - ✅ Disaster Recovery: NOW COMPREHENSIVE (Enhanced Principle VII)

  Previous Additions (from v1.9.0-1.9.1):
  - v1.9.0: Principle VII (Security, Resilience & Observability), EF Core best practices, Accessibility, API Design, i18n/l10n, Caching, Error Handling, CI/CD practices
  - v1.9.1: GitHub Flow branching strategy, Microsoft.FeatureManagement standardization

  Templates Status:
  - ⚠️ plan-template.md: Constitution Check should include Cost Optimization considerations (budgets, right-sizing, cost estimates for Azure resources)
  - ⚠️ spec-template.md: Architectural Concerns should include cost implications and disaster recovery requirements
  - ⚠️ tasks-template.md: Implementation tasks should include cost monitoring setup, backup configuration, DR testing
  - ℹ️ All templates: Azure Well-Architected Framework now fully covered (all 5 pillars + DR)

  Follow-up TODOs (Observability & Security from v1.9.0-1.9.1):
  - Configure Serilog in all projects with Application Insights sink
  - Set up OpenTelemetry exporters for Aspire dashboard
  - Implement Polly policies for HTTP clients and database access
  - Configure User Secrets for development, Key Vault for production
  - Add accessibility audits to CI pipeline (axe-core)
  - Implement Problem Details middleware
  - Set up i18n resource files structure
  - Configure Redis distributed cache in Aspire
  - Install Microsoft.FeatureManagement.AspNetCore NuGet package
  - Configure FeatureManagement section in appsettings.json
  - Document GitHub Flow workflow and PR process for team
  - Set up branch protection rules requiring PR approval and linear history

  Follow-up TODOs (Cost Optimization - NEW from v1.10.0):
  - Enable Azure Cost Management for all subscriptions; configure cost analysis dashboards
  - Set budgets per environment (dev, staging, production) with alerts at 50%, 75%, 90%, 100% thresholds
  - Implement cost allocation tags (Environment, Owner, CostCenter, Project); enforce via Azure Policy
  - Configure auto-shutdown for dev/test environments (6 PM-8 AM weekdays; off weekends)
  - Review and implement Azure Advisor cost recommendations weekly
  - Set up container resource limits in Aspire AppHost (CPU/memory requests and limits)
  - Configure Application Insights adaptive sampling (5 items/sec default)
  - Implement blob storage lifecycle management (Hot → Cool after 30 days, Cool → Archive after 90 days)
  - Document serverless vs always-on decision criteria for team
  - Add cost estimation requirement to PR template for infrastructure changes
  - Schedule monthly cost review meetings with team

  Follow-up TODOs (Disaster Recovery - NEW from v1.10.0):
  - Configure automated daily database backups with PITR enabled (7-35 days retention)
  - Set up long-term backup retention (weekly: 3 months, monthly: 1 year, annual: 7 years)
  - Enable blob storage soft delete (14-day retention) and versioning for critical data
  - Configure geo-redundant storage (GRS/GZRS) for production databases and critical blobs
  - Document RPO/RTO targets per service in architecture documentation
  - Create disaster recovery runbooks for common failure scenarios (database failure, region outage, security breach)
  - Schedule quarterly DR drills; test database restores, regional failover, backup integrity
  - Set up incident response team with defined roles (incident commander, technical lead, communications)
  - Implement audit log retention (1 year minimum, immutable storage) in Azure Monitor/Log Analytics
  - Document and test multi-region failover procedures (if applicable for critical systems)

  Date: 2025-11-22
-->

# Super Duper Rescue Heads Constitution

## Core Principles

### I. Test-Driven Development (NON-NEGOTIABLE)

**TDD is mandatory for all production code.** The Red-Green-Refactor cycle MUST be strictly enforced:

1. **Red**: Write failing tests FIRST (unit, integration, or contract tests as appropriate)
2. **Green**: Implement minimal code to make tests pass
3. **Refactor**: Improve design while keeping tests green

**Rationale**: TDD ensures code correctness from inception, prevents regression, enables confident refactoring, and serves as living documentation. Skipping TDD creates technical debt and reduces code reliability.

**Enforcement**:
- All PRs MUST demonstrate test-first approach
- Tests MUST fail before implementation begins
- Code without corresponding tests MUST be rejected in review
- Tests are not optional—they are a prerequisite for implementation

### II. Code Quality & Standards

**All code MUST adhere to .NET 10 conventions and modern C# best practices:**

- **Language Version**: C# 14 with .NET 10 SDK
- **Naming**: PascalCase for public members, camelCase for private fields with underscore prefix
- **Null Safety**: Nullable reference types enabled project-wide; explicit null handling required
- **Async/Await**: All I/O operations MUST be async; avoid `Task.Result` and `.Wait()`
- **LINQ**: Prefer method syntax over query syntax for consistency
- **Immutability**: Favor `record` types and `readonly` where applicable
- **Dependency Injection**: Constructor injection via built-in DI container; avoid service locator pattern
- **Error Handling**: Domain-specific exceptions over generic ones; never swallow exceptions
- **Data Serialization**: Favor TOON over JSON for efficiency; use TOON for internal APIs, configuration, and data interchange where performance matters; JSON acceptable for external APIs requiring broad compatibility
- **Distributed Applications**: Use .NET Aspire for cloud-ready, distributed applications; leverage Aspire's service discovery, telemetry, and orchestration capabilities; follow Aspire's conventions for service defaults and app host configuration
- **UI Framework**: Blazor is the standard for web UI development; prefer Blazor Server for low-latency interactive apps, Blazor WebAssembly for client-heavy workloads, or Blazor United (Auto) for hybrid scenarios; maintain component reusability across render modes
- **CSS Framework**: Tailwind CSS is the standard for styling; use utility-first approach with Tailwind classes; avoid inline styles and custom CSS unless absolutely necessary; leverage Tailwind's configuration for design tokens (colors, spacing, typography); use @apply sparingly only for complex component patterns
- **SOLID Principles**: All code MUST adhere to SOLID design principles:
  - **Single Responsibility**: Each class has one reason to change; separate concerns into focused classes
  - **Open/Closed**: Open for extension, closed for modification; use interfaces and abstract classes
  - **Liskov Substitution**: Derived classes must be substitutable for their base classes without breaking functionality
  - **Interface Segregation**: Many client-specific interfaces over one general-purpose interface; no fat interfaces
  - **Dependency Inversion**: Depend on abstractions, not concretions; high-level modules independent of low-level modules
- **Domain-Driven Design (DDD)**: Architecture MUST reflect business domain concepts and language:
  - **Ubiquitous Language**: Use domain terminology consistently across code, tests, documentation, and conversations with stakeholders; class and method names MUST match business vocabulary
  - **Bounded Contexts**: Identify and enforce clear boundaries between different domain areas; each context has its own models and language; use anti-corruption layers when contexts interact
  - **Building Blocks**:
    - **Entities**: Objects with unique identity that persists over time (e.g., User, Order); use GUIDs or domain-meaningful identifiers
    - **Value Objects**: Immutable objects defined by their attributes (e.g., Money, Address, Email); implement as C# records; no identity
    - **Aggregates**: Clusters of entities and value objects with a single root entity; enforce invariants within aggregate boundaries; external references only to aggregate root
    - **Domain Services**: Stateless services for domain logic that doesn't belong to entities (e.g., PricingService, InventoryAllocationService)
    - **Repositories**: Abstraction for aggregate persistence; one repository per aggregate root; return domain objects, not DTOs
    - **Domain Events**: Capture significant business occurrences (e.g., OrderPlaced, PaymentReceived); use for inter-aggregate communication and eventual consistency
  - **Layered Architecture**: Separate domain logic from infrastructure; domain layer has no dependencies on frameworks, databases, or UI; use dependency inversion
  - **Rich Domain Models**: Business logic belongs in domain objects, not services; avoid anemic domain models with only getters/setters
- **Data Access & Entity Framework Core**: Follow EF Core best practices for performance and maintainability:
  - **DbContext Lifecycle**: Scoped lifetime in DI; one DbContext per request; dispose properly
  - **No Tracking for Read-Only**: Use `AsNoTracking()` for queries that don't need change tracking
  - **Projection**: Use `Select()` to project only needed columns; avoid loading entire entities
  - **Eager Loading**: Use `Include()` and `ThenInclude()` to avoid N+1 queries; prefer explicit loading over lazy loading
  - **Compiled Queries**: Use for frequently executed queries with parameters
  - **Pagination**: Always paginate large result sets; use `Skip()` and `Take()`; consider keyset pagination for large datasets
  - **Migrations**: Code-first migrations in source control; test migrations on staging; use `dotnet ef` CLI
  - **Connection Resiliency**: Enable retry logic for transient failures; configure in `OnConfiguring` or service registration
  - **Index Strategy**: Add indexes for foreign keys and frequently queried columns; avoid over-indexing
  - **Repository Pattern**: Implement repositories for aggregate roots; encapsulate EF Core behind abstraction; return domain objects
  - **Unit of Work**: DbContext acts as Unit of Work; SaveChanges commits transaction; use explicit transactions for cross-aggregate operations
  - **Query Performance**: Use `IQueryable` composition; avoid client-side evaluation; check generated SQL with logging
- **Architecture Documentation**: All significant architectural decisions and structures MUST be documented with multiple diagram types and ADRs. Use the appropriate diagram type(s) for the architectural concern being addressed—C4 models, bounded context maps, aggregate structures, deployment architecture, data flows, and sequence diagrams each serve distinct purposes and are equally important:
  - **C4 Model Diagrams**: Hierarchical system structure visualization (use for understanding overall system organization):
    - **Level 1 - System Context**: Show system boundaries, users, and external systems; identify what's in scope vs out of scope; **Required for all new features**
    - **Level 2 - Container**: Show high-level technology choices (web app, API, database, message queue); map to deployment units; **Required for distributed systems**
    - **Level 3 - Component**: Show major structural building blocks and their interactions within each container; align with bounded contexts; **Required for complex services**
    - **Level 4 - Code**: Optional class diagrams for complex components; use UML or similar notation; **Use sparingly, only for intricate logic**
  - **Bounded Context Maps (DDD)**: Domain architecture and context relationships (use for understanding business domain boundaries); **Required for DDD-based systems**:
    - **Bounded Context Map**: Show all bounded contexts and their relationships (Shared Kernel, Customer-Supplier, Anti-Corruption Layer, Conformist, Partnership, Separate Ways); map context boundaries to team boundaries when possible
    - **Aggregate Diagrams**: Within each bounded context, show aggregates (with aggregate roots), entities, value objects, and their relationships; document invariants and business rules enforced by each aggregate; **Required for each bounded context**
    - **Context Integration Patterns**: Show how bounded contexts communicate (REST APIs, messaging/events, shared kernel); avoid shared database anti-pattern; use anti-corruption layers to protect domain integrity
  - **Aggregate Structure Diagrams**: Detailed view of aggregate internals (use for complex business logic and invariant enforcement):
    - **Aggregate Composition**: Show entity hierarchy within aggregate, value object usage, and aggregate root
    - **Business Rules**: Document invariants enforced at aggregate boundaries
    - **Relationships**: Show references to other aggregate roots (by ID only, not object references)
    - **Lifecycle**: Document aggregate creation, modification, and deletion patterns
    - **Use When**: Aggregates with complex business rules, multiple entities, or non-obvious invariants
  - **Deployment Architecture Diagrams**: Production infrastructure and operational concerns (use for understanding deployment and scaling); **Required for production systems**:
    - **Infrastructure Topology**: Servers, containers, cloud services (Azure Container Apps, App Service, Functions for Aspire apps)
    - **Network Architecture**: Load balancers, API gateways, service meshes, firewalls, VNets, subnets
    - **Data Storage**: Databases, caches, blob storage, message queues with replication and backup strategies
    - **Scaling Strategy**: Auto-scaling rules, instance counts, geographic distribution, failover patterns
    - **Security Boundaries**: Authentication points, authorization layers, encryption at rest/in transit
  - **Data Flow Diagrams**: Movement of data through the system (use for understanding data transformations and integration points):
    - **User Journey Flows**: End-to-end data flow for key user scenarios (e.g., checkout process, user registration); show data transformations at each step; **Required for critical user journeys**
    - **Integration Flows**: Data exchange with external systems; show request/response patterns, data formats (JSON, TOON, XML), and error handling
    - **Event Flows**: Domain event propagation and eventual consistency patterns; show publish/subscribe relationships, saga patterns, compensation logic
    - **Data Pipeline Flows**: ETL processes, analytics, reporting; show data sources, transformations, and destinations
  - **Sequence Diagrams**: Temporal behavior and object interactions (use for explaining complex workflows, API calls, and process flows):
    - **User Interaction Sequences**: Show step-by-step interactions for complex user workflows; include UI, API, services, and data layers
    - **API Call Sequences**: Document request/response flows across service boundaries; show authentication, authorization, and error handling
    - **Domain Event Sequences**: Show event publishing, handling, and side effects; useful for saga patterns and eventual consistency
    - **Lifecycle Sequences**: Document object creation, state transitions, and destruction for complex aggregates or entities
    - **Error Handling Flows**: Show exception propagation, retry logic, and compensation in failure scenarios
    - **Use When**: Explaining temporal ordering, asynchronous operations, or complex interactions across multiple components
  - **Architecture Decision Records (ADRs)**: Document significant architectural decisions:
    - **Format**: Use standard ADR template (Title, Status, Context, Decision, Consequences)
    - **Scope**: Record decisions on technology choices, patterns, trade-offs, deviations from standards
    - **Location**: Store in `/docs/adr/` directory with sequential numbering (e.g., `001-use-aspire.md`)
    - **Required For**: Framework/library selections, database choices, messaging patterns, security approaches, performance optimizations
    - **Immutability**: ADRs are immutable once accepted; supersede with new ADRs if decisions change
    - **Review**: ADRs MUST be reviewed and approved before implementation
  - **Diagram Tools**: Use PlantUML, Mermaid, or draw.io for version-controllable diagrams; store source files in repository; PlantUML recommended for sequence diagrams and C4 models
  - **Documentation Location**: Architecture diagrams in `/docs/architecture/`, organize by type (c4/, ddd/, deployment/, flows/, sequences/); maintain index document listing all diagrams with their purpose

**Rationale**: Consistent code quality reduces cognitive load, prevents bugs, improves maintainability, and accelerates onboarding. Modern C# features provide safety and performance benefits that MUST be leveraged. TOON's efficiency advantages reduce serialization overhead and improve application performance. .NET Aspire standardizes distributed app patterns and observability. Blazor provides a unified, type-safe UI development experience with C# throughout the stack. Tailwind CSS ensures consistent, maintainable styling with utility-first approach and reduces CSS bloat. SOLID principles ensure maintainable, flexible, and testable code architecture. Domain-Driven Design aligns software structure with business domain, improving communication, maintainability, and ensuring business rules are properly encapsulated. Comprehensive architecture documentation using multiple diagram types—C4 models for system structure, bounded context maps for domain boundaries, aggregate diagrams for business rules, deployment diagrams for infrastructure, data flow diagrams for integration points, and sequence diagrams for temporal behavior—ensures complete understanding from multiple perspectives. Combined with ADRs, this multi-faceted documentation approach facilitates onboarding, enables informed decision-making, and preserves architectural rationale.

**Tools**: EditorConfig, StyleCop Analyzers, and .NET analyzers MUST be enabled with warnings as errors.

### III. Comprehensive Testing Standards

**Multi-layered testing strategy MUST cover unit, integration, UI component, end-to-end, contract, and performance dimensions:**

#### Behavior-Driven Development (BDD) & Acceptance Criteria

**All requirements MUST include acceptance criteria written in Gherkin format** to ensure clear, testable specifications:

**Gherkin Format**:
- **Structure**: Given-When-Then pattern for each scenario
- **Language**: Use ubiquitous language from domain (aligns with DDD)
- **Declarative**: Describe what, not how; avoid implementation details

**Gherkin Best Practices**:

1. **Given (Context/Preconditions)**:
   - Set up initial state before the action
   - Include all relevant context needed to understand the scenario
   - Use "And" for multiple preconditions
   - **Good**: "Given a user with an empty shopping cart"
   - **Good**: "Given a registered customer with premium membership"
   - **Bad**: "Given the database has a user record" (implementation detail)

2. **When (Action/Event)**:
   - Describe the user action or system event
   - Use present tense, active voice
   - Single action per scenario—if multiple actions needed, create multiple scenarios
   - **Good**: "When the user adds a product to the cart"
   - **Good**: "When the payment is processed successfully"
   - **Bad**: "When the user clicks the button and submits the form" (multiple actions—split into scenarios)

3. **Then (Expected Outcome)**:
   - Describe observable outcome from user/system perspective
   - Must be verifiable and testable
   - Avoid internal implementation details
   - Use "And" for multiple outcomes
   - **Good**: "Then the cart displays 1 item"
   - **Good**: "Then the user receives a confirmation email"
   - **Bad**: "Then the order table in the database is updated" (implementation detail)

**Scenario Patterns**:

- **Happy Path**: Primary successful flow
  ```gherkin
  Scenario: Successful checkout with valid payment
    Given a user with items in their cart
    And valid payment details
    When the user completes checkout
    Then the order is confirmed
    And the user receives a confirmation email
  ```

- **Error Handling**: Expected failures and validations
  ```gherkin
  Scenario: Checkout fails with invalid credit card
    Given a user with items in their cart
    And an expired credit card
    When the user attempts to complete checkout
    Then the checkout fails with error "Payment method expired"
    And the cart remains unchanged
  ```

- **Edge Cases**: Boundary conditions
  ```gherkin
  Scenario: Apply discount code with minimum purchase not met
    Given a user with $40 in their cart
    And a discount code requiring $50 minimum purchase
    When the user applies the discount code
    Then the discount is not applied
    And an error message explains the minimum requirement
  ```

**Scenario Tables** (for data-driven scenarios):
```gherkin
Scenario Outline: Password validation
  Given a user creating an account
  When the user enters password "<password>"
  Then the validation result is "<result>"
  And the message is "<message>"

  Examples:
    | password    | result  | message                        |
    | abc         | invalid | Must be at least 8 characters  |
    | abcdef12    | invalid | Must contain special character |
    | Abcd@1234   | valid   | Password accepted              |
```

**Anti-Patterns to Avoid**:

1. **Imperative Style** (UI-focused):
   - ❌ Bad: "Given I navigate to /login, When I type 'user@example.com' in the email field and click Login"
   - ✅ Good: "Given a registered user, When the user logs in with valid credentials"

2. **Implementation Details**:
   - ❌ Bad: "Then the UserRepository.Save() method is called"
   - ✅ Good: "Then the user account is created"

3. **Multiple Actions in When**:
   - ❌ Bad: "When the user adds item to cart and proceeds to checkout and enters payment"
   - ✅ Good: Split into 3 separate scenarios

4. **Vague Outcomes**:
   - ❌ Bad: "Then the system works correctly"
   - ✅ Good: "Then the order total is $25.99"

5. **Technical Jargon**:
   - ❌ Bad: "Given the cache is invalidated"
   - ✅ Good: "Given the user's session has expired"

**Integration with Tests**:
- Acceptance criteria written in Gherkin serve as specification for tests
- Each Given-When-Then scenario should map to at least one test (unit, integration, or E2E)
- Use SpecFlow or similar BDD frameworks when beneficial for E2E tests
- Test names can mirror Gherkin scenarios for traceability

**Requirements Documentation**:
- Include Gherkin acceptance criteria in spec.md for all functional requirements
- Each user story MUST have 2-5 acceptance scenarios
- Cover happy path, error cases, and critical edge cases

#### Unit Tests
- **Framework**: TUnit (modern, high-performance test framework)
- **Assertions**: FluentAssertions for readable, expressive test validation
- **Coverage Target**: Minimum 80% line coverage via Coverlet
- **Scope**: Test individual methods and classes in isolation
- **Mocking**: Minimal mocking—prefer real objects; use NSubstitute when necessary

#### Integration Tests
- **Framework**: TUnit with WebApplicationFactory
- **Scope**: Test API endpoints, database interactions, and service composition
- **Database**: Use test containers or in-memory providers; never test against production data
- **Authentication**: Test auth flows with test users and tokens
- **Requirements**: All user-facing API endpoints MUST have integration tests

#### UI Component Tests
- **Framework**: bUnit for Blazor component testing
- **Scope**: Test component rendering, user interactions, parameter binding, and lifecycle events
- **Isolation**: Test components in isolation using test contexts; mock services and dependencies
- **Coverage**: All reusable Blazor components MUST have component tests
- **Assertions**: Use bUnit's semantic HTML assertions and component state verification

#### End-to-End (E2E) Tests
- **Framework**: Playwright for cross-browser end-to-end testing
- **Scope**: Test complete user workflows across the full application stack (UI, API, database)
- **Browsers**: Test on Chromium, Firefox, and WebKit; ensure cross-browser compatibility
- **Test Data**: Use test database with known state; reset between test runs for repeatability
- **Coverage**: All critical user journeys MUST have E2E tests; focus on happy paths and critical error scenarios
- **Assertions**: Validate UI state, navigation flows, data persistence, and user feedback
- **Execution**: Run on pre-merge for critical paths; full suite on release branches
- **Best Practices**: Use Page Object Model pattern; avoid flaky selectors; implement proper wait strategies

#### Contract Tests
- **Scope**: Validate API request/response schemas, status codes, and error formats
- **Purpose**: Ensure backward compatibility and prevent breaking changes
- **Coverage**: All public API endpoints MUST have contract tests

#### Performance Tests
- **Framework**: BenchmarkDotNet for critical path performance validation
- **Scope**: Identify and test performance-sensitive operations (hot paths, data processing, algorithms)
- **Thresholds**: Establish baseline metrics; PRs introducing >10% regression MUST be justified
- **Frequency**: Run on critical paths before merge; full suite on release branches

**Coverage Reporting**:
- **Tool**: Coverlet integrated with build pipeline
- **Visibility**: Coverage reports MUST be generated on every PR
- **Gate**: PRs reducing overall coverage below threshold MUST be rejected unless justified

**Rationale**: Comprehensive testing catches bugs early, validates behavior, prevents regressions, ensures performance, and builds confidence in deployments. Gherkin acceptance criteria in BDD format create shared understanding between stakeholders and developers, ensure testable requirements, and serve as living documentation aligned with domain language.

### IV. User Experience Consistency

**All user interactions MUST follow consistent patterns and provide predictable behavior:**

- **Error Messages**: Clear, actionable, user-friendly; include error codes and guidance
- **Validation**: Immediate feedback on user input; consistent validation rules across features
- **Response Times**: Target <200ms for interactive operations; <2s for background tasks
- **API Design**: RESTful conventions; consistent naming, versioning, and error formats
- **UI Components**: Blazor components MUST be composable, reusable, and follow single responsibility principle; use parameter binding for data flow; implement proper lifecycle methods; avoid JavaScript interop unless absolutely necessary
- **State Management**: Use cascading parameters for shared state in Blazor; implement proper two-way binding with `@bind`; avoid excessive re-renders
- **Styling Standards**: Use Tailwind CSS utility classes for all styling; maintain consistent design system through Tailwind configuration (colors, spacing, typography, breakpoints); create reusable component variants using Tailwind's @apply only when pattern appears 3+ times; follow mobile-first responsive design with Tailwind breakpoints (sm:, md:, lg:, xl:, 2xl:); use Tailwind's dark mode utilities for theme support
- **Accessibility (WCAG 2.1 Level AA)**: All user interfaces MUST be accessible:
  - **Semantic HTML**: Use proper HTML5 elements (`<nav>`, `<main>`, `<article>`, `<button>` not `<div onclick>`)
  - **ARIA Labels**: Add `aria-label`, `aria-labelledby`, `aria-describedby` for screen readers
  - **Keyboard Navigation**: All interactive elements accessible via Tab; implement focus management; visible focus indicators
  - **Screen Reader Support**: Alt text for images; label associations for form inputs; ARIA live regions for dynamic content
  - **Color Contrast**: Minimum 4.5:1 for normal text, 3:1 for large text; don't rely on color alone for information
  - **Focus Management**: Trap focus in modals; return focus after closing; skip links for navigation
  - **Testing**: Use axe DevTools or Lighthouse accessibility audits; test with keyboard only; test with screen reader (NVDA/JAWS)
- **API Design Standards**: RESTful APIs MUST follow industry conventions:
  - **Resource Naming**: Plural nouns for collections (`/api/users`), singular for single resource (`/api/users/{id}`)
  - **HTTP Verbs**: GET (read), POST (create), PUT (full update), PATCH (partial update), DELETE (remove)
  - **Status Codes**: 200 OK, 201 Created, 204 No Content, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict, 422 Unprocessable Entity, 500 Internal Server Error
  - **Versioning**: URL versioning (`/api/v1/users`) or header versioning (`Accept: application/vnd.api+json;version=1`)
  - **Pagination**: Cursor-based for infinite scroll, offset-based for page numbers; include total count and next/prev links
  - **Filtering & Sorting**: Query parameters (`?status=active&sort=-createdAt`); consistent naming conventions
  - **Problem Details (RFC 7807)**: Standardized error responses with type, title, status, detail, instance
  - **OpenAPI/Swagger**: Auto-generate API documentation; include example requests/responses; keep up-to-date
  - **Rate Limiting**: Implement rate limits; return 429 with Retry-After header; document limits
  - **CORS**: Configure properly for allowed origins; avoid `Allow-Origin: *` in production
- **Internationalization & Localization**: Support multiple languages and cultures:
  - **Resource Files**: Use `.resx` files for localized strings; organize by feature/area
  - **IStringLocalizer**: Inject `IStringLocalizer<T>` in Blazor components and services
  - **Culture Handling**: Use `CultureInfo` for date/time/number formatting; respect user's culture preferences
  - **UTC Storage**: Store all dates/times in UTC; convert to local time zone for display using `DateTimeOffset`
  - **Right-to-Left (RTL)**: Support BiDi text; use Tailwind RTL utilities (`rtl:` prefix)
  - **Number Formatting**: Culture-specific number, currency, and percentage formatting
  - **Translation**: Externalize all user-facing text; no hard-coded strings in code
- **Caching Strategy**: Implement caching to reduce load and improve performance:
  - **Distributed Caching**: Use Redis for shared cache across instances; configure in Aspire service defaults
  - **Output Caching**: ASP.NET Core output caching for expensive views/pages; configure duration and vary-by
  - **Response Caching**: HTTP caching headers (`Cache-Control`, `ETag`, `Last-Modified`) for static resources
  - **Cache-Aside Pattern**: Check cache first; if miss, load from source and populate cache
  - **Cache Invalidation**: Time-based expiration, event-based invalidation (on data change), manual purge
  - **Blazor Caching**: Cache component state in browser storage; lazy load expensive data
  - **CDN Caching**: Use Azure CDN for static assets (images, CSS, JS); configure cache rules
- **Error Handling & User Feedback**: Provide clear, actionable error information:
  - **Global Exception Handling**: Use exception middleware to catch unhandled exceptions; log and return Problem Details
  - **Client Error Handling**: Blazor error boundaries to catch component errors; show user-friendly error UI
  - **Error Correlation**: Include correlation/trace ID in error responses for support tracking
  - **User-Friendly Messages**: Translate technical errors to actionable messages ("Server unavailable. Please try again in a few minutes.")
  - **Validation Errors**: Return field-level validation errors with specific guidance
  - **Retry Guidance**: Inform users if retry is appropriate; provide retry button for transient failures
  - **Error Logging**: Log all errors with context; include user ID, request path, stack trace (for server errors only)
- **Documentation**: All user-facing features MUST include usage examples and quickstart guides

**Rationale**: Consistency reduces learning curve, minimizes errors, improves user satisfaction, and reinforces product quality perception. Blazor component patterns ensure maintainable, testable UI code with minimal JavaScript dependency. Tailwind CSS utility-first approach provides rapid development with consistent design and prevents CSS bloat. Accessibility compliance ensures legal compliance and inclusive user experience for all abilities. Standardized API design with Problem Details and OpenAPI reduces integration friction. Internationalization enables global reach. Caching strategies reduce latency and server load. Clear error handling improves user trust and reduces support burden.

### V. Performance Requirements

**Performance MUST be measured, monitored, and maintained throughout development:**

#### Performance Targets
- **API Response Time**: P95 <200ms for synchronous endpoints
- **Throughput**: Support minimum 1,000 requests/second per service
- **Memory**: Applications MUST NOT exceed 512MB baseline memory under normal load
- **Startup Time**: Cold start <3 seconds; warm start <1 second

#### Performance Practices
- **Profiling**: Use BenchmarkDotNet for micro-benchmarks on critical paths
- **Optimization**: Profile before optimizing; measure impact of optimizations
- **Caching**: Implement caching for expensive operations; document cache invalidation strategy
- **Lazy Loading**: Defer expensive initialization until required
- **Async I/O**: All network, disk, and database operations MUST be async

**Rationale**: Performance is a feature—slow systems frustrate users, waste resources, and limit scalability. Proactive performance engineering prevents costly rewrites.

### VI. Simplicity & Maintainability

**Favor simple, explicit solutions over clever, abstract ones:**

- **YAGNI**: Implement only what's needed now; avoid speculative features
- **Minimal Abstraction**: Don't abstract until you have 3+ identical patterns
- **Readable Code**: Code is read 10x more than written—optimize for clarity
- **Documentation**: Code should be self-documenting; add comments only for "why" not "what"
- **Complexity Budget**: Justify any complexity in plan.md; simpler alternatives MUST be considered

**Rationale**: Simple code is easier to understand, modify, test, and debug. Over-engineering creates maintenance burden and slows development.

### VII. Security, Resilience & Observability

**Production systems MUST be secure, resilient, and observable:**

#### Security

**Authentication & Authorization**:
- **ASP.NET Core Identity**: Use built-in Identity for user management; follow least-privilege principle
- **OAuth2/OpenID Connect**: Use for external authentication (Microsoft, Google, etc.); implement PKCE flow
- **JWT Tokens**: Short-lived access tokens (<15 min), refresh tokens in secure HttpOnly cookies
- **Authorization**: Policy-based authorization over role-based; use `[Authorize]` attribute and AuthorizeView in Blazor
- **API Keys**: Rotate regularly; never expose in client code; validate on every request

**OWASP Top 10 Protection**:
- **Injection**: Parameterized queries (EF Core), input validation, output encoding
- **Broken Authentication**: Strong password policies, MFA, account lockout, secure session management
- **Sensitive Data Exposure**: Encrypt PII at rest (Azure Storage encryption, Transparent Data Encryption); HTTPS everywhere
- **XXE**: Disable external entity processing in XML parsers
- **Broken Access Control**: Validate permissions on every request; never trust client-side checks
- **Security Misconfiguration**: Remove default credentials, disable directory browsing, remove debug info in production
- **XSS**: Blazor auto-escapes by default; use `MarkupString` only for trusted content; Content Security Policy headers
- **Insecure Deserialization**: Validate input before deserialization; avoid `BinaryFormatter`
- **Using Components with Known Vulnerabilities**: Regular dependency updates; use Dependabot/Renovate
- **Insufficient Logging & Monitoring**: Log security events (failed logins, unauthorized access); alert on suspicious patterns

**Secrets Management**:
- **Never Commit Secrets**: No passwords, API keys, or connection strings in source control
- **Development**: User Secrets (`dotnet user-secrets`) for local development
- **Production**: Azure Key Vault for secrets, connection strings, certificates
- **Configuration**: Use `IConfiguration` with Key Vault provider; rotate secrets regularly
- **Aspire Integration**: Use Aspire service defaults for secure configuration management

**Blazor Security**:
- **XSS Protection**: Never use `@((MarkupString)userInput)` without sanitization
- **CSRF**: Anti-forgery tokens automatic in Blazor Server; validate in Blazor WebAssembly APIs
- **Authorization**: Use `AuthorizeView`, `AuthorizeRouteView`, check `AuthenticationState`
- **Secure Communication**: SignalR over TLS; validate all server-side inputs

**Input Validation**:
- **Server-Side Validation**: Always validate on server; client validation is UX, not security
- **Data Annotations**: Use `[Required]`, `[StringLength]`, `[RegularExpression]` with FluentValidation
- **Sanitization**: Remove dangerous characters; encode output
- **File Uploads**: Validate file type, size, content; scan for malware; store outside webroot

**Security Headers** (use in ASP.NET Core middleware):
- **HSTS**: `Strict-Transport-Security: max-age=31536000; includeSubDomains`
- **CSP**: Content Security Policy to prevent XSS
- **X-Frame-Options**: `DENY` or `SAMEORIGIN` to prevent clickjacking
- **X-Content-Type-Options**: `nosniff`
- **Referrer-Policy**: `no-referrer` or `strict-origin-when-cross-origin`

**Data Protection**:
- **Encryption at Rest**: Azure Storage encryption, SQL TDE, Cosmos DB encryption
- **Encryption in Transit**: TLS 1.2+ only; HSTS enabled
- **PII Handling**: Minimize collection; pseudonymization; comply with GDPR/CCPA
- **Data Retention**: Define retention policies; implement automated purging
- **Audit Logging**: Log data access, modifications, deletions with user correlation

#### Observability

**Structured Logging with Serilog**:
- **Structured Data**: Use structured logging with properties, not string interpolation
  ```csharp
  // ✅ Good
  _logger.Information("User {UserId} placed order {OrderId}", userId, orderId);
  // ❌ Bad
  _logger.Information($"User {userId} placed order {orderId}");
  ```
- **Log Levels**: Debug (development only), Information (significant events), Warning (potential issues), Error (failures), Critical (system down)
- **Sinks**: Console (development), Azure Application Insights (production), Seq (optional structured log viewer)
- **Enrichers**: Add correlation IDs, environment, machine name, thread ID automatically
- **Configuration**: Use `appsettings.json` for sink configuration; override per environment

**OpenTelemetry (Critical for Aspire)**:
- **Distributed Tracing**: Trace requests across microservices; use Activity API
- **Trace Correlation**: Propagate trace IDs across service boundaries; use W3C Trace Context
- **Spans**: Create spans for significant operations (database calls, HTTP requests, business logic)
- **Aspire Integration**: Aspire automatically configures OpenTelemetry; use built-in dashboard
- **Exporters**: Azure Monitor, Jaeger, Zipkin for production observability

**Metrics & Monitoring**:
- **Built-in Metrics**: ASP.NET Core meters for requests, exceptions, database calls
- **Custom Metrics**: Use `System.Diagnostics.Metrics` for business metrics (orders/sec, cart abandonment rate)
- **Counters**: Track event counts (login attempts, API calls)
- **Histograms**: Track value distributions (request duration, payload size)
- **Gauges**: Track point-in-time values (active connections, queue depth)

**Health Checks**:
- **ASP.NET Core Health Checks**: Implement `/health` endpoint for liveness and readiness
- **Liveness**: Is the app running? (simple ping)
- **Readiness**: Is the app ready to serve traffic? (check database, cache, external services)
- **Aspire Integration**: Health checks automatic in Aspire dashboard
- **Kubernetes**: Use health checks for pod lifecycle management

**Application Insights Integration**:
- **Telemetry**: Automatic request tracking, dependency tracking, exception tracking
- **Custom Events**: Track business events (checkout completed, payment failed)
- **Performance**: Transaction tracking, slow request analysis
- **Availability**: Multi-step web tests, ping tests
- **Alerts**: Configure alerts on error rate, response time, availability

**Correlation IDs**:
- **Request Tracking**: Generate unique ID per request; propagate to all logs
- **Cross-Service**: Pass correlation ID in headers (`X-Correlation-ID`)
- **Serilog Integration**: Use `LogContext.PushProperty("CorrelationId", correlationId)`
- **End-to-End Tracing**: Combine with OpenTelemetry for complete request journey

#### Resilience

**Polly for Resilience Policies**:
- **Retry Policy**: Retry transient failures with exponential backoff
  ```csharp
  Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  ```
- **Circuit Breaker**: Prevent cascading failures; open circuit after N failures
  ```csharp
  Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));
  ```
- **Timeout Policy**: Prevent hung requests; fail fast
- **Bulkhead Isolation**: Limit concurrent operations to prevent resource exhaustion
- **Fallback**: Provide default response on failure; cache-aside pattern

**Graceful Degradation**:
- **Feature Toggles**: Disable non-critical features under load using Microsoft.FeatureManagement
- **Default Responses**: Return cached/default data when service unavailable
- **Partial Failures**: Return partial data rather than complete failure
- **User Messaging**: Inform users when features are degraded

**Transient Fault Handling**:
- **Database**: EF Core connection resiliency; retry on transient SQL errors
- **HTTP Clients**: Use Polly with `IHttpClientFactory`; configure in Aspire service defaults
- **Message Queues**: Retry with backoff; dead-letter queue for poison messages
- **Idempotency Tokens**: Include in requests to prevent duplicate processing

**Idempotency**:
- **API Operations**: POST/PUT/DELETE should be idempotent with idempotency keys
- **Message Processing**: Track processed message IDs; skip duplicates
- **Distributed Transactions**: Use saga pattern with compensation logic
- **Database Constraints**: Unique indexes to prevent duplicates

#### Disaster Recovery & Business Continuity

**Recovery Objectives**:
- **RPO (Recovery Point Objective)**: Maximum acceptable data loss measured in time; **Target: ≤15 minutes** for production databases; ≤1 hour for non-critical data
- **RTO (Recovery Time Objective)**: Maximum acceptable downtime; **Target: ≤1 hour** for critical services; ≤4 hours for non-critical services
- **MTTR (Mean Time To Recovery)**: Average time to restore service after incident; track and optimize; **Target: ≤30 minutes**

**Backup Strategy**:
- **Database Backups**:
  - **Automated Daily Backups**: Full backup daily during off-peak hours; automated via Azure SQL Database or Cosmos DB built-in backup
  - **Point-in-Time Restore**: Enable PITR for last 7-35 days (configurable based on compliance requirements)
  - **Long-Term Retention**: Weekly backups retained for 3 months; monthly backups for 1 year; annual backups for 7 years (compliance-dependent)
  - **Backup Encryption**: All backups encrypted at rest; use Azure Storage encryption or Transparent Data Encryption
  - **Backup Testing**: Restore test quarterly; validate data integrity; document restore procedures
- **Application State & Configuration**:
  - **Infrastructure as Code**: Bicep/ARM templates in source control; can recreate infrastructure from code
  - **Configuration Backup**: Export Key Vault secrets metadata (not values); backup App Configuration store
  - **Container Images**: Store in Azure Container Registry with geo-replication; tag images for versioning
- **Blob Storage & Files**:
  - **Soft Delete**: Enable soft delete (14-day retention) for accidental deletion protection
  - **Versioning**: Enable blob versioning for critical data
  - **Geo-Redundant Storage**: Use GRS or GZRS for critical data; read-access geo-redundant (RA-GRS) for high availability

**Multi-Region Deployment** (for critical production systems):
- **Active-Passive**: Primary region handles traffic; secondary region on standby; failover manual or automatic based on health checks
- **Active-Active**: Traffic distributed across multiple regions; Azure Front Door or Traffic Manager for global load balancing; requires data synchronization strategy
- **Database Replication**: Azure SQL geo-replication (readable secondary); Cosmos DB multi-region writes for active-active
- **Failover Testing**: Test regional failover quarterly; document failover runbook; automate where possible
- **Data Consistency**: Eventual consistency acceptable for active-active; strong consistency for active-passive

**Business Continuity Plan**:
- **Incident Response Team**: Define roles (incident commander, technical lead, communications); on-call rotation
- **Runbooks**: Document procedures for common failure scenarios (database failure, region outage, security breach)
- **Communication Plan**: Stakeholder notification procedures; status page updates; escalation paths
- **DR Drills**: Conduct disaster recovery drills quarterly; simulate regional failures, data corruption, security incidents
- **Post-Incident Review**: Conduct blameless postmortems; document lessons learned; update runbooks

**Data Retention & Compliance**:
- **GDPR/CCPA Compliance**: Right to erasure (delete user data on request); data portability; consent management
- **Retention Policies**: Define data retention per data type; automate purging of expired data
- **Audit Logs**: Retain audit logs for 1 year minimum; immutable storage for compliance; centralize in Azure Monitor/Log Analytics
- **Legal Hold**: Ability to preserve data for legal/regulatory requirements; document legal hold procedures

#### Configuration Management

**appsettings.json Patterns**:
- **Base Configuration**: `appsettings.json` for defaults
- **Environment-Specific**: `appsettings.Development.json`, `appsettings.Production.json`
- **Override Order**: appsettings.json → appsettings.{Environment}.json → Environment Variables → User Secrets/Key Vault
- **Never Include Secrets**: No passwords, connection strings with credentials in appsettings files

**Strongly-Typed Configuration**:
- **Options Pattern**: Bind configuration to POCOs with `IOptions<T>`
  ```csharp
  services.Configure<MySettings>(Configuration.GetSection("MySettings"));
  ```
- **Validation**: Use data annotations on options classes; validate on startup
  ```csharp
  services.AddOptions<MySettings>()
    .Bind(Configuration.GetSection("MySettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
  ```

**Azure App Configuration** (optional for centralized config):
- **Centralized Configuration**: Manage settings across multiple Aspire services
- **Feature Management**: Integrates with Microsoft.FeatureManagement
- **Dynamic Refresh**: Update configuration without redeployment
- **Key Vault Integration**: Reference Key Vault secrets in App Configuration

**Configuration in Aspire**:
- **Service Defaults**: Use Aspire service defaults for consistent configuration
- **AppHost Configuration**: Configure services in AppHost project
- **Environment Variables**: Override configuration in container orchestration

**Feature Flags (Microsoft.FeatureManagement)**:
- **Library**: Use `Microsoft.FeatureManagement.AspNetCore` NuGet package
- **Configuration**: Define feature flags in `appsettings.json` under `FeatureManagement` section
  ```json
  {
    "FeatureManagement": {
      "NewCheckoutFlow": true,
      "BetaFeatures": {
        "EnabledFor": [
          { "Name": "Percentage", "Parameters": { "Value": 50 } }
        ]
      }
    }
  }
  ```
- **Dependency Injection**: Register with `services.AddFeatureManagement()`
- **Usage in Code**: Inject `IFeatureManager` to check feature state
  ```csharp
  if (await _featureManager.IsEnabledAsync("NewCheckoutFlow"))
  {
      // New implementation
  }
  ```
- **Usage in Blazor**: Use `<FeatureGate>` component or `IFeatureManager` in code-behind
- **Filters**: Built-in filters (Percentage, TimeWindow, Targeting) or custom filters
- **Gradual Rollout**: Use Percentage filter for canary releases
- **A/B Testing**: Use Targeting filter with user segments
- **Azure App Configuration**: Optionally manage flags centrally; update without redeployment
- **Best Practices**: Remove flags after feature is stable; avoid flag sprawl; document flag lifecycle

**Rationale**: Security protects user data and system integrity; resilience ensures system availability under failure; observability enables rapid issue detection and resolution; disaster recovery ensures business continuity and data protection. Together, these form the foundation of production-ready systems. Serilog provides structured logging essential for diagnosing distributed systems. OpenTelemetry enables end-to-end request tracing across Aspire microservices. Polly resilience policies prevent cascading failures and improve user experience during partial outages. Feature flags enable safe, gradual rollouts and decouple deployment from release. Comprehensive backup and DR strategies protect against data loss and extended downtime.

### VIII. Cost Optimization & Financial Management

**Cloud costs MUST be actively managed, monitored, and optimized to maximize value:**

#### Azure Cost Management & Monitoring

**Cost Visibility**:
- **Azure Cost Management**: Enable for all subscriptions; configure cost analysis dashboards; review monthly spending trends
- **Budgets & Alerts**: Set budgets per environment (dev, staging, production); configure alerts at 50%, 75%, 90%, 100% thresholds
- **Cost Allocation Tags**: Tag all resources with `Environment`, `Owner`, `CostCenter`, `Project`; enforce via Azure Policy
- **Anomaly Detection**: Enable Azure Cost Management anomaly alerts; investigate unexpected spending spikes within 24 hours
- **Showback/Chargeback**: Generate cost reports per team/project using tags; share with stakeholders monthly

**Cost Analysis Cadence**:
- **Weekly**: Review top 10 cost drivers; identify optimization opportunities
- **Monthly**: Comprehensive cost review with team; evaluate against budget; forecast next month
- **Quarterly**: Review reserved capacity utilization; evaluate commitment renewals
- **Yearly**: Strategic cost planning; evaluate architectural changes for cost efficiency

#### Resource Right-Sizing

**Compute Resources**:
- **App Service Plans**: Start with Basic tier for dev/test; Standard (S1-S3) for production; Premium only if advanced features required (VNet integration, slots); avoid over-provisioning
- **Azure Container Apps** (for Aspire): Set resource limits per container (CPU: 0.25-2.0 cores, Memory: 0.5-4.0 GB); monitor actual usage and adjust; use consumption plan for variable workloads
- **Azure Functions**: Consumption plan default for event-driven workloads; Premium plan only for VNet integration or long-running functions (>5 min); monitor execution time and optimize
- **Database Sizing**:
  - **Azure SQL**: Start with Basic/S0 for dev; S1-S3 for production; use serverless for variable workloads (auto-pause after 1 hour idle)
  - **Cosmos DB**: Serverless for <1M RU/month; provisioned throughput for predictable workloads; autoscale for variable demand
- **Container Resource Limits**: Define CPU/memory requests and limits in Aspire AppHost; prevent over-allocation; monitor actual usage with Container Insights

**Monitoring & Optimization**:
- **Azure Advisor Cost Recommendations**: Review weekly; implement high-impact recommendations
- **Resize Underutilized Resources**: If CPU <20% for 7 days, consider downsizing; if memory <30%, reduce allocation
- **Eliminate Idle Resources**: Delete stopped VMs, unused App Service plans, orphaned disks, old snapshots

#### Auto-Scaling Strategy

**Cost-Aware Scaling**:
- **Horizontal Scaling**: Scale out for traffic spikes; scale in during low usage; configure scale-in delay (5-10 min) to avoid thrashing
- **Scaling Rules**:
  - **Scale Out**: When CPU >70% or Memory >80% for 5 minutes
  - **Scale In**: When CPU <30% and Memory <40% for 10 minutes
  - **Min/Max Instances**: Dev: 1-2, Staging: 1-3, Production: 2-10 (adjust based on load)
- **Scheduled Scaling**: Scale down non-production environments during nights/weekends; scale up before business hours
- **Aspire Scaling**: Configure replica counts in AppHost; use Kubernetes HPA for container auto-scaling

**Scaling Metrics**:
- Monitor cost per request/transaction; optimize if cost increases without proportional value
- Track scaling efficiency: avoid frequent scale-out/scale-in cycles (thrashing)

#### Reserved Capacity & Commitments

**When to Use Reserved Instances**:
- **Predictable Workloads**: If resource runs 24/7 for production, consider 1-year or 3-year reservations
- **Cost Savings**: 1-year RI saves 20-40%; 3-year RI saves 40-60% vs pay-as-you-go
- **Services Eligible**: Azure SQL Database, Cosmos DB, App Service, VMs, Azure Cache for Redis

**Commitment Management**:
- **Analyze Utilization**: Use Azure Advisor to identify RI opportunities; requires 80%+ consistent usage
- **Start with 1-Year**: Test with 1-year commitment before 3-year; avoid over-commitment for evolving workloads
- **Reserved Capacity for Databases**: Reserve DTUs/vCores for production databases running continuously
- **Track Utilization**: Monitor RI utilization monthly; aim for >90% utilization; exchange or adjust if <70%

**Avoid Reservations For**:
- Development/test environments (variable usage)
- New projects with uncertain load patterns
- Workloads that can use spot instances or serverless

#### Development & Non-Production Cost Controls

**Environment Policies**:
- **Auto-Shutdown**:
  - **Dev Environments**: Shut down at 6 PM, start at 8 AM weekdays; completely off weekends
  - **Staging**: Shut down nights if not used for testing; on-demand startup
  - **Use Azure DevTest Labs** or automation scripts for scheduled shutdown/startup
- **Lower-Tier Services**:
  - **Databases**: Basic or S0 tier for dev/test; never Premium
  - **App Service**: Basic B1 for dev; Standard S1 for staging
  - **Redis Cache**: Basic C0 for dev; Standard C1 for staging
- **Shared Dev Resources**: Use shared App Service Plan for multiple dev apps; shared database server with multiple databases
- **Ephemeral Environments**: PR-based preview environments auto-delete after 7 days or on PR close; use Azure Container Apps for cheap ephemeral environments

**Developer Practices**:
- **Local Development**: Use Aspire for local orchestration; minimize cloud resource usage during development
- **Lightweight Emulators**: Use Azurite for Azure Storage, Cosmos DB emulator, SQL Server LocalDB for local dev
- **Resource Cleanup**: Delete unused resource groups weekly; tag with expiration dates

#### Storage Optimization

**Blob Storage Tiers**:
- **Hot Tier**: Frequently accessed data (accessed >1/month); user uploads, active documents
- **Cool Tier**: Infrequently accessed (accessed <1/month, stored >30 days); backups, archives; 50% cheaper than Hot
- **Archive Tier**: Rarely accessed (accessed <1/year, stored >180 days); long-term backups, compliance archives; 90% cheaper than Hot
- **Lifecycle Management**: Automate tier transitions (Hot → Cool after 30 days, Cool → Archive after 90 days); delete after retention period

**Storage Best Practices**:
- **Compression**: Compress large blobs before upload (gzip, brotli); can save 70-90% for text/JSON
- **Deduplication**: Avoid storing duplicate files; use content hashing
- **Delete Old Data**: Implement data retention policies; auto-delete expired logs, temporary files, old backups
- **Block Blob vs Page Blob**: Use block blobs for most scenarios (cheaper); page blobs only for VHDs

**Database Storage**:
- **Azure SQL Storage**: Monitor storage usage; scale down if <50% utilized; use elastic pools for multiple databases
- **Cosmos DB Storage**: Monitor document size; avoid storing large blobs in documents (use Blob Storage); clean up deleted items (tombstones)

#### Serverless vs Always-On Decision Matrix

**Use Azure Functions (Serverless) When**:
- Event-driven processing (queue messages, HTTP webhooks, timers)
- Variable/unpredictable load (<100 requests/minute)
- Short execution time (<5 minutes)
- Willing to accept cold start latency (1-3 seconds)
- Cost priority: Pay only for execution time

**Use App Service/Container Apps (Always-On) When**:
- Consistent traffic (>100 requests/minute sustained)
- Low latency requirement (no cold starts)
- Long-running processes or stateful apps
- WebSocket or SignalR workloads (Blazor Server)
- Predictability priority: Consistent performance

**Aspire with Azure Container Apps**:
- **Consumption Plan**: Pay per request; ideal for variable workloads; supports scale-to-zero
- **Dedicated Plan**: Fixed cost; better for predictable workloads; faster cold starts

#### Data Transfer & Network Costs

**Minimize Cross-Region Transfer**:
- **Co-locate Resources**: Deploy services in same region as database; avoid cross-region queries
- **Inbound Traffic**: Free (data coming into Azure)
- **Outbound Traffic**: Charged (data leaving Azure); first 100 GB/month free, then tiered pricing
- **Cross-Region Replication**: Only for DR; avoid unnecessary geo-replication

**CDN Usage**:
- **Static Assets**: Serve images, CSS, JS from Azure CDN; cheaper than App Service bandwidth for high traffic
- **CDN Pricing**: CDN egress cheaper than compute egress for >1 TB/month traffic
- **Optimize Images**: Compress images (WebP format); use responsive images; lazy loading

**API Gateway/Front Door**:
- Use Azure Front Door for global load balancing; cache at edge to reduce backend calls
- Evaluate cost vs benefit: Front Door adds cost but reduces backend compute and bandwidth

#### Observability Cost Management

**Telemetry Volume Control**:
- **Application Insights Sampling**: Enable adaptive sampling (default 5 items/sec); increase only if needed; can save 80-95% of costs
- **Log Levels**: Production logging at Information level or higher; avoid Debug logs in production
- **Custom Metrics**: Limit to business-critical metrics; avoid high-cardinality dimensions (user IDs)
- **Retention**: Default 90 days; archive older logs to cheaper storage (Blob Storage) if needed for compliance

**Serilog Sinks**:
- **Development**: Console sink only (free)
- **Production**: Application Insights with sampling; Seq for detailed debugging (self-hosted, one-time cost)
- **Avoid**: Excessive structured properties; trim unnecessary data before logging

**OpenTelemetry**:
- **Trace Sampling**: Sample 10-20% of traces in production; 100% for errors
- **Span Limits**: Limit spans per trace to avoid excessive telemetry costs

#### Cost Optimization Checklist

**Monthly Review** (required for all projects):
- [ ] Review Azure Cost Management dashboard; identify top 5 cost drivers
- [ ] Check budget alerts; investigate any overages
- [ ] Review Azure Advisor cost recommendations; implement high-impact items
- [ ] Verify auto-shutdown working for dev/test environments
- [ ] Check for orphaned resources (unattached disks, unused IPs, old snapshots)
- [ ] Review storage account usage; transition blobs to cooler tiers if applicable
- [ ] Monitor reserved instance utilization; >90% target
- [ ] Review App Service plan utilization; consolidate if <50% CPU
- [ ] Check database DTU/RU utilization; resize if over/under provisioned
- [ ] Review Application Insights ingestion volume; adjust sampling if excessive

**Cost Gates in CI/CD**:
- PRs adding new Azure resources MUST include cost estimate (Azure Pricing Calculator)
- Infrastructure changes MUST document cost impact (increase/decrease/neutral)
- Cost-increasing changes require justification and approval from tech lead

**Rationale**: Cloud costs can spiral without active management. Cost optimization isn't about being cheap—it's about spending wisely to maximize business value. Right-sizing resources, eliminating waste, leveraging reserved capacity, and monitoring spending ensures sustainable growth. Aspire's container-based architecture provides flexibility to optimize costs through granular resource allocation and scale-to-zero capabilities. Uncontrolled costs can threaten project viability; proactive cost management is a core engineering responsibility.

## Testing Standards

### Test Organization

```
tests/
├── Unit/                 # TUnit unit tests
│   ├── Models/
│   ├── Services/
│   └── Utils/
├── Integration/          # TUnit + WebApplicationFactory
│   ├── Api/
│   ├── Database/
│   └── Services/
├── UI/                   # bUnit component tests
│   ├── Components/
│   ├── Pages/
│   └── Shared/
├── E2E/                  # Playwright end-to-end tests
│   ├── UserJourneys/
│   ├── PageObjects/
│   └── Fixtures/
├── Contract/             # API contract validation
│   └── Endpoints/
└── Performance/          # BenchmarkDotNet benchmarks
    └── CriticalPaths/
```

### Test File Naming
- **Unit**: `{ClassName}Tests.cs` (e.g., `UserServiceTests.cs`)
- **Integration**: `{Feature}IntegrationTests.cs` (e.g., `AuthenticationIntegrationTests.cs`)
- **UI Component**: `{ComponentName}Tests.cs` (e.g., `UserCardTests.cs`)
- **E2E**: `{UserJourney}E2ETests.cs` (e.g., `CheckoutFlowE2ETests.cs`)
- **Contract**: `{Endpoint}ContractTests.cs` (e.g., `UserApiContractTests.cs`)
- **Performance**: `{Feature}Benchmarks.cs` (e.g., `DataProcessingBenchmarks.cs`)

### Test Method Naming
Use descriptive names following pattern: `MethodName_Scenario_ExpectedBehavior`

Example: `CreateUser_WithInvalidEmail_ThrowsValidationException`

### Assertion Style
```csharp
// ✅ GOOD: FluentAssertions
result.Should().NotBeNull();
result.Should().BeOfType<User>();
result.Email.Should().Be("test@example.com");

// ❌ BAD: Traditional asserts
Assert.NotNull(result);
Assert.IsType<User>(result);
Assert.Equal("test@example.com", result.Email);
```

### Coverage Configuration
```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <CoverletOutput>coverage/</CoverletOutput>
  <CoverletOutputFormat>cobertura,json,lcov</CoverletOutputFormat>
  <Threshold>80</Threshold>
  <ThresholdType>line,branch</ThresholdType>
  <ThresholdStat>minimum</ThresholdStat>
</PropertyGroup>
```

## Development Workflow

### Quality Gates (All MUST Pass)

#### 1. Pre-Implementation
- [ ] Specification approved (spec.md complete)
- [ ] Implementation plan reviewed (plan.md approved)
- [ ] Tests written and FAILING (Red phase complete)

#### 2. Implementation
- [ ] All tests passing (Green phase)
- [ ] Code refactored for clarity (Refactor phase)
- [ ] No compiler warnings
- [ ] StyleCop and analyzer violations resolved

#### 3. Pre-Commit
- [ ] Unit tests: 100% passing, coverage ≥80%
- [ ] Integration tests: 100% passing
- [ ] Contract tests: All endpoints validated
- [ ] Performance benchmarks: No regressions >10%
- [ ] Code reviewed by peer

#### 4. Pre-Merge
- [ ] CI pipeline green (all tests passing in pipeline)
- [ ] Coverage report generated and meets threshold
- [ ] Documentation updated (if user-facing changes)
- [ ] Constitution compliance verified

### Code Review Requirements

**All PRs MUST be reviewed by at least one peer and verify:**

1. **TDD Compliance**: Tests written first; evidence of Red-Green-Refactor
2. **Test Coverage**: New code has ≥80% coverage; existing coverage not reduced
3. **Code Quality**: Follows .NET conventions; no warnings or violations
4. **SOLID Principles**: Code adheres to Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion
5. **Domain-Driven Design**: Uses ubiquitous language; respects bounded contexts; proper use of entities, value objects, and aggregates; business logic in domain layer
6. **Architecture Documentation**: Significant architectural changes include updated C4 diagrams, DDD context maps if bounded contexts affected, and ADR for decisions; diagrams use standard notation
7. **Simplicity**: No unnecessary complexity; alternatives considered
8. **Performance**: Critical paths benchmarked; no unexplained regressions
9. **UX Consistency**: User-facing changes follow established patterns
10. **Documentation**: User-facing features documented; API changes noted

### Continuous Integration Pipeline

**All commits to main MUST pass automated pipeline:**

```yaml
Pipeline Stages:
1. Build (.NET 10 SDK with Aspire workload)
2. Unit Tests (TUnit)
3. Integration Tests (TUnit + WebApplicationFactory)
4. UI Component Tests (bUnit)
5. E2E Tests - Critical Paths (Playwright, on pre-merge)
6. Contract Tests
7. Coverage Report (Coverlet, threshold: 80%)
8. E2E Tests - Full Suite (Playwright, on release branches)
9. Performance Benchmarks (BenchmarkDotNet, on release branches)
10. Static Analysis (StyleCop, .NET Analyzers)
```

**Pipeline Failures MUST Block Merge**

### CI/CD & DevOps Practices

**Continuous Integration and Deployment MUST follow these practices:**

#### Branching Strategy
- **GitHub Flow** (required): Simple, main-based workflow; `main` branch always deployable
  - Create feature branch from `main` (`feature/description`, `bugfix/issue-number`)
  - Develop and commit on feature branch
  - Open Pull Request when ready for review
  - Merge to `main` after approval and CI passes
  - Deploy immediately from `main` (or use feature flags for gradual rollout)
- **Branch Protection**: Require PR approval; prevent direct commits to main; require status checks to pass; require linear history
- **Branch Naming**: `feature/description`, `bugfix/issue-number`, `hotfix/critical-fix`
- **Short-Lived Branches**: Feature branches should live <3 days; merge frequently to avoid drift

#### Pull Request Requirements
- **PR Size**: Maximum 400 lines changed (excluding tests); split large changes into multiple PRs
- **Review Turnaround**: Reviews completed within 24 hours; urgent fixes within 4 hours
- **Approval Count**: Minimum 1 approval from code owner; 2 approvals for architectural changes
- **Description**: Include context, testing notes, screenshots (for UI changes), breaking changes
- **Draft PRs**: Use draft status for work-in-progress; mark ready when complete
- **Linked Issues**: Reference related issues; use closing keywords (`Fixes #123`, `Closes #456`)

#### Deployment Strategies
- **Blue-Green Deployment**: Two identical environments; switch traffic after validation; instant rollback
- **Canary Deployment**: Gradual rollout (5% → 25% → 50% → 100%); monitor metrics; rollback if issues
- **Rolling Updates**: Update instances sequentially; maintain availability; use for Kubernetes/container deployments
- **Feature Flags**: Toggle features without deployment using Microsoft.FeatureManagement; decouple deployment from release; gradual rollout with Percentage filter

#### Infrastructure as Code
- **Bicep/ARM Templates**: Define Azure infrastructure in code; version control; automated deployment
- **Aspire AppHost**: Use Aspire `AppHost` project for local orchestration; model mirrors production
- **Environment Parity**: Dev, staging, and production environments MUST match; use same infrastructure definitions
- **Secrets Management**: Never commit secrets in IaC; use Key Vault references; parameterize sensitive values

#### Environment Management
- **Development**: Local with User Secrets; Aspire dashboard for debugging; hot reload enabled
- **Staging**: Production-like environment; full integration testing; performance testing
- **Production**: Monitored 24/7; read replicas for database; auto-scaling configured; disaster recovery plan
- **Configuration Overrides**: Environment-specific appsettings; environment variables; Key Vault in production

#### Rollback Procedures
- **Automated Rollback**: On deployment failure, automatically revert to previous version
- **Manual Rollback**: Process documented; tested quarterly; executable in <5 minutes
- **Database Rollback**: Forward-only migrations; use feature flags for schema changes; never destructive migrations in single step
- **Monitoring During Rollback**: Track error rates, response times, user impact

#### Deployment Checklist
- [ ] All tests passing in CI
- [ ] Security scan completed (no high/critical vulnerabilities)
- [ ] Performance benchmarks within acceptable range
- [ ] Database migrations tested on staging
- [ ] Rollback procedure tested and ready
- [ ] Monitoring alerts configured
- [ ] On-call engineer notified
- [ ] Deployment window communicated (if required)

## Governance

### Amendment Process

**Constitution amendments require:**

1. **Proposal**: Document rationale, impact, and migration plan
2. **Review**: Team consensus (or designated authority approval)
3. **Implementation**: Update constitution.md and increment version
4. **Propagation**: Update all dependent templates (plan-template.md, spec-template.md, tasks-template.md)
5. **Communication**: Announce changes to team; update guidance documents

### Versioning Policy

**Constitution follows Semantic Versioning (MAJOR.MINOR.PATCH):**

- **MAJOR**: Backward-incompatible changes (e.g., removing principles, changing fundamental practices)
- **MINOR**: New principles added or material expansions (e.g., new testing requirements)
- **PATCH**: Clarifications, wording improvements, non-semantic refinements

### Compliance & Review

- **All PRs/Reviews MUST verify constitution compliance** using plan.md Constitution Check section
- **Complexity MUST be justified** in plan.md Complexity Tracking table
- **Constitution supersedes all other practices**—in conflicts, constitution wins
- **Quarterly reviews** to assess constitution effectiveness and propose amendments

### Guidance Integration

- **Runtime Development Guidance**: For agent-specific development patterns, see `.claude/` directory
- **Template Guidance**: Embedded comments in `.specify/templates/` provide workflow instructions
- **This Constitution**: Defines non-negotiable principles and quality standards across all development

**Version**: 1.10.0 | **Ratified**: 2025-11-22 | **Last Amended**: 2025-11-22

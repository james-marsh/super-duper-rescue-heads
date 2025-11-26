# Implementation Plan: Soft Delete & Recovery

**Branch**: `003-soft-delete-recovery` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary

This feature implements a safety net by introducing soft delete for items with 30-day retention, allowing users to recover accidentally deleted items. Includes automated purging after retention period and manual permanent deletion option. Extends Features 001-002 with `IsDeleted` and `DeletedAt` columns, global query filters, and recovery endpoints.

## Technical Context

**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Blazor Server, EF Core, Hangfire (background jobs for purging)
**Storage**: Azure SQL Database (production), SQL Server LocalDB (development)
**Testing**: TUnit, bUnit, Playwright, FluentAssertions
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 001-002
**Performance Goals**:
- Recovery endpoint: P95 <200ms
- Purge job: Process 10,000 items in <5 minutes
**Constraints**:
- 30-day retention period (configurable)
- Deleted items count toward 50,000 user item limit
- Purge job runs daily
**Scale/Scope**:
- Typical: 50-200 deleted items per user
- Edge case: 5,000 deleted items (10% of limit)

## Constitution Check

### Test-Driven Development
✅ **PASS** - TDD Red-Green-Refactor cycle

### Code Quality & Standards
✅ **PASS** - Extends Features 001-002 architecture; EF Core global query filter pattern

### Comprehensive Testing
✅ **PASS** - Unit tests for soft delete logic, integration tests for purge job, E2E tests for recovery workflow

### User Experience Consistency
✅ **PASS** - Consistent with existing patterns; "Deleted Items" UI follows Feature 001 collection list pattern

### Performance Requirements
✅ **PASS** - Purge job runs as background process; indexed queries on `IsDeleted` and `DeletedAt`

### Simplicity & Maintainability
✅ **PASS** - Simple boolean flag + timestamp; EF Core query filter handles filtering automatically

### Security, Resilience & Observability
✅ **PASS** - Hangfire for reliable background jobs; logs purge operations; alerts on purge failures

### Cost Optimization
✅ **PASS** - Purge frees storage; minimal cost for retention (30 days storage)

**Gate Status: ✅ ALL GATES PASSED**

## Project Structure

### Documentation
```text
specs/003-soft-delete-recovery/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── deleted-items-api.yaml
```

### Source Code (Modifications to Features 001-002)
```text
src/SuperDuperRescueHeads.Domain/Items/
├── Item.cs                               # MODIFIED - Add IsDeleted, DeletedAt, Restore()

src/SuperDuperRescueHeads.Infrastructure/
├── Data/Configurations/ItemConfiguration.cs  # MODIFIED - Add query filter
├── BackgroundJobs/
│   └── PurgeDeletedItemsJob.cs          # NEW - Hangfire job
└── Migrations/
    └── 003_AddSoftDelete.cs             # NEW - Add IsDeleted, DeletedAt columns

src/SuperDuperRescueHeads.Api/Endpoints/
└── DeletedItemsEndpoints.cs             # NEW - Recovery/purge endpoints

src/SuperDuperRescueHeads.Web/Components/Pages/
└── DeletedItems/
    ├── Index.razor                       # NEW - Deleted items list
    └── Restore.razor                     # NEW - Restore confirmation

tests/
├── Unit/Domain/ItemSoftDeleteTests.cs    # NEW
├── Integration/BackgroundJobs/PurgeJobTests.cs  # NEW
└── E2E/UserJourneys/RecoverItemE2ETests.cs      # NEW
```

## Complexity Tracking

No constitution violations. Soft delete is standard practice, not premature complexity:
- **Global query filter**: EF Core built-in feature, prevents accidental queries of deleted items
- **Background job (Hangfire)**: Required for reliable automated purging
- **30-day retention**: User requirement, not premature optimization

**Status: No unjustified complexity**

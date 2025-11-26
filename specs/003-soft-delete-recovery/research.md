# Research: Soft Delete & Recovery

**Feature**: 003-soft-delete-recovery | **Date**: 2025-11-24

## Overview

This feature adds soft delete to Items (Feature 002) with 30-day retention and recovery capability. All decisions extend Features 001-002.

## Key Technology Decisions

### 1. EF Core Global Query Filter for Soft Delete

**Decision**: Use EF Core's `HasQueryFilter()` to automatically exclude deleted items from queries

**Rationale**:
- Prevents accidental querying of deleted items
- Single point of configuration
- Can be explicitly disabled with `IgnoreQueryFilters()` for deleted items list
- Constitution-aligned: simple, maintainable

**Implementation**:
```csharp
builder.HasQueryFilter(i => !i.IsDeleted);
```

**Alternatives Rejected**:
- Manual `WHERE IsDeleted = 0` in every query: Error-prone, violates DRY
- Separate `DeletedItems` table: Duplication, complex joins for recovery

**References**: [EF Core Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)

### 2. Hangfire for Automated Purging

**Decision**: Use Hangfire for reliable background job to purge expired items daily

**Rationale**:
- Constitution requires resilience
- Persistent job queue (survives app restarts)
- Retry logic built-in
- Dashboard for monitoring
- Azure-compatible

**Implementation**:
```csharp
RecurringJob.AddOrUpdate<PurgeDeletedItemsJob>(
    "purge-deleted-items",
    job => job.ExecuteAsync(),
    Cron.Daily(2)); // 2 AM daily
```

**Alternatives Rejected**:
- Azure Functions Timer: Requires separate deployment, more complex than needed
- Hosted Service with Timer: No persistence, no retry logic, lost on restart

**References**: [Hangfire Documentation](https://www.hangfire.io/)

### 3. Restore Validation Strategy

**Decision**: Validate collection existence before restore; handle gracefully if collection deleted

**Rationale**:
- User may have deleted collection after deleting item
- Show friendly error: "Cannot restore - original collection was deleted"
- Offer option to restore to different collection (future enhancement)

## Database Schema Changes

**Migration 003_AddSoftDelete**:
```sql
ALTER TABLE Items ADD IsDeleted BIT NOT NULL DEFAULT 0;
ALTER TABLE Items ADD DeletedAt DATETIMEOFFSET NULL;

CREATE INDEX IX_Items_DeletedAt ON Items(DeletedAt)
WHERE IsDeleted = 1; -- Filtered index for purge queries
```

## Performance Considerations

- **Query Performance**: Filtered index on `IsDeleted` prevents index scans
- **Purge Job**: Batch delete 1,000 items at a time to avoid long transactions
- **Deleted Items List**: Separate index for deleted items queries

## Summary

Soft delete pattern is simple, well-established, and extends existing architecture cleanly. EF Core query filter ensures safety, Hangfire provides reliability.

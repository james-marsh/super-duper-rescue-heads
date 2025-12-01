# super-duper-rescue-heads Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-27

## Active Technologies
- C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, EF Core (007-group-sharing)
- Azure SQL Database (007-group-sharing)
- C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, SignalR (real-time), SendGrid (email), Hangfire (scheduled) (008-notifications)
- C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, EF Core (RowVersion), SignalR (presence/locking) (009-concurrent-editing)

- C# 14 with .NET 10 SDK + .NET Aspire, Entity Framework Core 9.0, SQL Server Full-Text Search (initial) or Azure Cognitive Search (future), Blazor Server (004-search-functionality)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

# Add commands for C# 14 with .NET 10 SDK

## Code Style

C# 14 with .NET 10 SDK: Follow standard conventions

## Recent Changes
- 009-concurrent-editing: Added C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, EF Core (RowVersion), SignalR (presence/locking)
- 008-notifications: Added C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, SignalR (real-time), SendGrid (email), Hangfire (scheduled)
- 007-group-sharing: Added C# 14 with .NET 10 SDK + .NET Aspire, Blazor Server, EF Core


<!-- MANUAL ADDITIONS START -->

## Feature 009: Concurrent Editing Implementation Notes

### Architecture
- **Optimistic Concurrency Control**: Uses EF Core RowVersion (byte[]) for automatic version tracking
- **Conflict Resolution Strategy**: Last-write-wins with notification to losing user
- **Conflict Tracking**: ConflictEvent entity for monitoring and audit purposes

### Key Components
**Domain Layer**:
- `Item.RowVersion` - EF Core concurrency token (byte[], automatically managed)
- `ConflictEvent` - Entity recording conflict occurrences
- `ConflictResolutionResult` - Value object for conflict resolution outcomes
- `ConcurrencyException` - Domain exception for concurrency conflicts
- `IConflictResolutionService` - Service interface for conflict handling
- `IConflictEventRepository` - Repository interface for conflict event persistence

**Infrastructure Layer**:
- `ItemConfiguration` - Updated with `.IsRowVersion()` for RowVersion property
- `ConflictEventConfiguration` - EF Core configuration with 4 indexes (ItemId, WinningUserId, LosingUserId, OccurredAt)
- `ConflictEventRepository` - Full CRUD with conflict querying
- `ConflictResolutionService` - Handles conflicts, sends notifications via Feature 008

**Migrations**:
- `20251201000007_AddRowVersionForConcurrency` - Adds RowVersion column to Items table
- `20251201000008_AddConflictEvents` - Creates ConflictEvents table with indexes

### User Stories Implemented
- ✅ **US4**: Optimistic Locking with Version Control - RowVersion tracking
- ✅ **US1**: Concurrent Item Editing with Conflict Resolution - Notification integration
- ✅ **US2**: Concurrent Item Addition - Works by default (new GUIDs, no conflicts)
- ⏳ **US3**: Real-Time Collaboration Awareness - Deferred (requires SignalR PresenceHub, EditSession entity)

### Deferred Features
- EditSession entity for tracking active users viewing/editing items
- SignalR PresenceHub for real-time presence indicators
- UI components for showing "currently editing" badges
- Session timeout management (5 minute inactivity)
- Multi-device presence sync

### Integration Points
- **Feature 008 (Notifications)**: ConflictDetected notification type sent to losing user
- **DbUpdateConcurrencyException**: Caught in ItemRepository.UpdateAsync (handled in service layer)

### Testing Notes
- Concurrent item additions: Multiple users can add items simultaneously (tested by design - unique GUIDs)
- Conflict detection: Triggered when RowVersion mismatch occurs on save
- Last-write-wins: First save succeeds, second save rejected with notification

<!-- MANUAL ADDITIONS END -->

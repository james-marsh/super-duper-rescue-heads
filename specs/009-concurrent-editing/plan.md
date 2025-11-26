# Implementation Plan: Concurrent Editing

**Branch**: `009-concurrent-editing` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary
Enables multiple users to edit shared collections/items simultaneously with conflict resolution. Uses optimistic concurrency control with EF Core RowVersion, real-time presence indicators via SignalR, and "last write wins" with merge options for conflicts.

## Technical Context
**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Blazor Server, EF Core (RowVersion), SignalR (presence/locking)
**Storage**: Azure SQL Database
**Testing**: TUnit, bUnit, Playwright
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 002 & 006
**Performance Goals**: Conflict detection <50ms, presence updates <200ms
**Constraints**: Max 5 simultaneous editors per item
**Scale/Scope**: Rare conflicts (0.1% of edits), typical 1-2 concurrent editors

## Constitution Check
✅ ALL GATES PASSED - EF Core RowVersion for optimistic concurrency (built-in), SignalR for presence, TDD for conflict resolution logic

## Project Structure
- Modifies: Add RowVersion to Item/Collection entities, concurrency exception handling
- New: Presence service (SignalR), conflict resolution UI, merge logic

## Complexity Tracking
No violations - Concurrent editing is user requirement for shared collections; optimistic concurrency is standard pattern

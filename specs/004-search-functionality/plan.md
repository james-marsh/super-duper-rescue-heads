# Implementation Plan: Search Functionality

**Branch**: `004-search-functionality` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary

Adds full-text search across items with collection-specific and global search modes, filtering, and relevance ranking. Uses Azure Cognitive Search for performance (<2s for 5,000 items).

## Technical Context

**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Azure Cognitive Search SDK, Blazor Server
**Storage**: Azure SQL Database + Azure Cognitive Search
**Testing**: TUnit, bUnit, Playwright
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 001-002
**Performance Goals**: Search <2s for 5,000 items, index updates <500ms
**Constraints**: Full-text search, filters (type/date/attributes), relevance ranking
**Scale/Scope**: 50,000 items per user max

## Constitution Check
✅ ALL GATES PASSED - Azure Cognitive Search is appropriate, TDD for search logic, indexed for performance

## Project Structure
- New: Search service, indexer, API endpoints, search UI page
- Modifies: Add domain event handlers for indexing on item changes

## Complexity Tracking
No violations - Search is user requirement, Azure Cognitive Search is appropriate tool

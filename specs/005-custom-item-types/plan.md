# Implementation Plan: Custom Item Types

**Branch**: `005-custom-item-types` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary
Allows users to define custom item types beyond the built-in types (Vinyl, Comic, Puzzle). Users can create custom schemas defining required/optional attributes with data types and validation rules. Extends Feature 002's JSON attribute system.

## Technical Context
**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Blazor Server, EF Core, FluentValidation
**Storage**: Azure SQL Database
**Testing**: TUnit, bUnit, Playwright
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 001-002
**Performance Goals**: Schema validation <50ms, CRUD <200ms
**Constraints**: Max 50 custom types per user, max 30 attributes per type
**Scale/Scope**: Typical 3-10 custom types per user

## Constitution Check
✅ ALL GATES PASSED - JSON schema storage for flexibility, TDD for schema validation logic, simple UI for schema builder

## Project Structure
- New: ItemTypeSchema aggregate, schema validation service, schema builder UI
- Modifies: ItemType from value object to entity with custom schemas

## Complexity Tracking
No violations - Custom types are user requirement; JSON schema is appropriate for dynamic attribute definitions

# Implementation Plan: Basic Sharing

**Branch**: `006-basic-sharing` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary
Enables users to share collections with other users via email/username with read-only or edit permissions. Includes invitation system, permission management, and shared collection views. Single-user sharing only (groups in Feature 007).

## Technical Context
**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Blazor Server, EF Core, SendGrid (email)
**Storage**: Azure SQL Database
**Testing**: TUnit, bUnit, Playwright
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends Features 001-002
**Performance Goals**: Share invite <500ms, permission check <50ms
**Constraints**: Max 10 collaborators per collection
**Scale/Scope**: Typical 1-3 collaborators per shared collection

## Constitution Check
✅ ALL GATES PASSED - Standard sharing pattern, policy-based authorization for permissions, TDD for permission logic

## Project Structure
- New: CollectionShare aggregate, invitation service, permission authorization handlers, sharing UI
- Modifies: Authorization policies to check CollectionShare permissions

## Complexity Tracking
No violations - Sharing is user requirement; permission checks are standard authorization pattern

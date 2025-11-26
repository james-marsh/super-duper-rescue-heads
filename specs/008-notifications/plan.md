# Implementation Plan: Notifications

**Branch**: `008-notifications` | **Date**: 2025-11-24 | **Spec**: [spec.md](./spec.md)

## Summary
Provides in-app and email notifications for key events: sharing invitations, item deletion warnings (3 days before 30-day expiry), shared collection updates. User-configurable notification preferences.

## Technical Context
**Language/Version**: C# 14 with .NET 10 SDK
**Primary Dependencies**: .NET Aspire, Blazor Server, SignalR (real-time), SendGrid (email), Hangfire (scheduled)
**Storage**: Azure SQL Database
**Testing**: TUnit, bUnit, Playwright
**Target Platform**: Azure Container Apps
**Project Type**: Web application - extends all previous features
**Performance Goals**: Notification delivery <1s (in-app), <5s (email)
**Constraints**: Max 100 unread notifications per user
**Scale/Scope**: Average 5-20 notifications per day per active user

## Constitution Check
✅ ALL GATES PASSED - SignalR for real-time push, Hangfire for scheduled notifications, TDD for notification logic

## Project Structure
- New: Notification aggregate, notification service, SignalR hub, email templates, notification preferences UI
- Modifies: Domain event handlers to create notifications

## Complexity Tracking
No violations - Notifications are user requirement; SignalR and Hangfire are appropriate tools

# Copilot Instructions for Super Duper Rescue Heads

## Project Overview

Super Duper Rescue Heads is a collaborative collection management application for tracking vinyl records, comic books, puzzles, and other collectibles. Built with C# 14, .NET 10, and .NET Aspire.

## Technology Stack

- **Framework**: .NET 10 SDK with C# 14
- **Architecture**: .NET Aspire for cloud-native development
- **Frontend**: Blazor Server
- **Database**: Entity Framework Core 9.0 with Azure SQL Database
- **Real-time**: SignalR for notifications and presence
- **Testing**: TUnit framework

## Project Structure

```
SuperDuperRescueHeads/
├── SuperDuperRescueHeads.AppHost/       # .NET Aspire host project
├── SuperDuperRescueHeads.ServiceDefaults/ # Shared service configuration
├── SuperDuperRescueHeads.Domain/        # Domain entities, value objects, interfaces
├── SuperDuperRescueHeads.Infrastructure/ # EF Core, repositories, external services
├── SuperDuperRescueHeads.Api/           # API endpoints and SignalR hubs
├── SuperDuperRescueHeads.Web/           # Blazor Server frontend
├── SuperDuperRescueHeads.Tests.Unit/    # Unit tests (TUnit)
├── SuperDuperRescueHeads.Tests.Integration/ # Integration tests
├── SuperDuperRescueHeads.Tests.Contract/ # Contract tests
├── SuperDuperRescueHeads.Tests.E2E/     # End-to-end tests
└── SuperDuperRescueHeads.Tests.UI/      # UI tests
```

## Build and Test Commands

```bash
# Restore dependencies
dotnet restore SuperDuperRescueHeads/SuperDuperRescueHeads.sln

# Build the solution
dotnet build SuperDuperRescueHeads/SuperDuperRescueHeads.sln

# Run all tests
dotnet test SuperDuperRescueHeads/SuperDuperRescueHeads.sln

# Run specific test project
dotnet test SuperDuperRescueHeads/SuperDuperRescueHeads.Tests.Unit/

# Run the application (via Aspire AppHost)
dotnet run --project SuperDuperRescueHeads/SuperDuperRescueHeads.AppHost/
```

## Code Style Guidelines

### C# Conventions

- Use C# 14 features where appropriate (e.g., primary constructors, collection expressions)
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use implicit usings
- Follow standard .NET naming conventions (PascalCase for public members, camelCase for private fields with underscore prefix)
- Prefer file-scoped namespaces

### Domain Layer Patterns

- **Entities**: Use factory methods (`Create()`) instead of public constructors
- **Value Objects**: Create immutable types with validation in factory methods
- **Domain Events**: Use for cross-aggregate communication
- **Repository Interfaces**: Define in Domain layer, implement in Infrastructure

Example entity pattern:
```csharp
public class Item
{
    private Item() { } // EF Core constructor
    
    public static Item Create(Guid collectionId, ItemName name, Dictionary<string, object> attributes, string? notes = null)
    {
        // Validation and initialization
    }
}
```

### Testing Patterns

This project uses TUnit for testing. Follow these patterns:

```csharp
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class MyTests
{
    [Test]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var sut = CreateTestSubject();

        // Act
        var result = sut.DoSomething();

        // Assert
        await Assert.That(result).IsEqualTo(expectedValue);
    }
}
```

### EF Core Patterns

- Configure entities in separate `*Configuration` classes implementing `IEntityTypeConfiguration<T>`
- Use RowVersion for optimistic concurrency control
- Use migrations with descriptive names: `YYYYMMDDHHMMSS_DescriptiveName`

## Feature Specifications

Feature specifications are located in the `specs/` directory. When implementing features:

1. Review the relevant spec in `specs/XXX-feature-name/`
2. Follow the user scenarios and acceptance criteria
3. Implement according to the success criteria

## Key Domain Concepts

- **Collection**: A container for items (max 100 per user)
- **Item**: An individual collectible with attributes (max 50,000 per user)
- **ItemType**: Predefined (vinyl, comics, puzzles) or custom schemas
- **Sharing**: Collections can be shared with view or edit permissions
- **Groups**: Optional grouping for batch sharing permissions

## Concurrency Handling

The project uses optimistic concurrency control with EF Core RowVersion:

- `Item.RowVersion` is automatically managed by EF Core
- Conflicts are detected via `DbUpdateConcurrencyException`
- Resolution strategy: Last-write-wins with notification to the losing user
- Conflict events are logged for audit purposes

## Real-time Features

SignalR is used for:
- In-app notifications (< 1 second delivery)
- Collaborative editing presence
- Conflict notifications

## Important Notes

- All times should be stored as `DateTimeOffset` in UTC
- Use GUIDs for entity identifiers
- Soft delete with 30-day recovery period for items
- Maximum attribute value length: varies by type (check validation)
- Maximum notes length: 1000 characters

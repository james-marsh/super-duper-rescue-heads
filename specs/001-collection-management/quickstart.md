# Quickstart: Implementing Core Collection Management

**Feature**: 001-collection-management | **Date**: 2025-11-24

## Overview

This quickstart guide provides a step-by-step implementation path for the Core Collection Management feature following Test-Driven Development (TDD) and Domain-Driven Design (DDD) principles. The implementation is organized into phases, with each phase building on the previous one.

## Prerequisites

- .NET 10 SDK installed
- Visual Studio 2025 or Rider 2025
- Azure SQL Database or SQL Server LocalDB for development
- Basic understanding of DDD, TDD, and Blazor

## Project Setup

### 1. Create Solution and Projects

```bash
# Create solution
dotnet new sln -n SuperDuperRescueHeads

# Create Aspire AppHost (orchestration)
dotnet new aspire-apphost -n SuperDuperRescueHeads.AppHost
dotnet sln add SuperDuperRescueHeads.AppHost

# Create Service Defaults (shared configuration)
dotnet new aspire-servicedefaults -n SuperDuperRescueHeads.ServiceDefaults
dotnet sln add SuperDuperRescueHeads.ServiceDefaults

# Create Domain layer (class library)
dotnet new classlib -n SuperDuperRescueHeads.Domain -f net10.0
dotnet sln add SuperDuperRescueHeads.Domain

# Create Infrastructure layer (class library)
dotnet new classlib -n SuperDuperRescueHeads.Infrastructure -f net10.0
dotnet sln add SuperDuperRescueHeads.Infrastructure

# Create API project (web API)
dotnet new webapi -n SuperDuperRescueHeads.Api -f net10.0
dotnet sln add SuperDuperRescueHeads.Api

# Create Web UI project (Blazor Server)
dotnet new blazor -n SuperDuperRescueHeads.Web -f net10.0 --interactivity Server
dotnet sln add SuperDuperRescueHeads.Web

# Create test projects
dotnet new classlib -n SuperDuperRescueHeads.Tests.Unit -f net10.0
dotnet new classlib -n SuperDuperRescueHeads.Tests.Integration -f net10.0
dotnet new classlib -n SuperDuperRescueHeads.Tests.UI -f net10.0
dotnet new classlib -n SuperDuperRescueHeads.Tests.E2E -f net10.0
dotnet new classlib -n SuperDuperRescueHeads.Tests.Contract -f net10.0

dotnet sln add SuperDuperRescueHeads.Tests.Unit
dotnet sln add SuperDuperRescueHeads.Tests.Integration
dotnet sln add SuperDuperRescueHeads.Tests.UI
dotnet sln add SuperDuperRescueHeads.Tests.E2E
dotnet sln add SuperDuperRescueHeads.Tests.Contract
```

### 2. Add Project References

```bash
# Infrastructure depends on Domain
dotnet add SuperDuperRescueHeads.Infrastructure reference SuperDuperRescueHeads.Domain

# API depends on Infrastructure and ServiceDefaults
dotnet add SuperDuperRescueHeads.Api reference SuperDuperRescueHeads.Infrastructure
dotnet add SuperDuperRescueHeads.Api reference SuperDuperRescueHeads.ServiceDefaults

# Web depends on Domain and ServiceDefaults
dotnet add SuperDuperRescueHeads.Web reference SuperDuperRescueHeads.Domain
dotnet add SuperDuperRescueHeads.Web reference SuperDuperRescueHeads.ServiceDefaults

# AppHost depends on ServiceDefaults
dotnet add SuperDuperRescueHeads.AppHost reference SuperDuperRescueHeads.ServiceDefaults
dotnet add SuperDuperRescueHeads.AppHost reference SuperDuperRescueHeads.Api
dotnet add SuperDuperRescueHeads.AppHost reference SuperDuperRescueHeads.Web
```

### 3. Install NuGet Packages

```bash
# Domain layer (no external dependencies except maybe MediatR for domain events)
cd SuperDuperRescueHeads.Domain
dotnet add package MediatR

# Infrastructure layer
cd ../SuperDuperRescueHeads.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# API project
cd ../SuperDuperRescueHeads.Api
dotnet add package FluentValidation.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.ApplicationInsights
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore

# Web project
cd ../SuperDuperRescueHeads.Web
dotnet add package Serilog.AspNetCore

# Unit tests
cd ../SuperDuperRescueHeads.Tests.Unit
dotnet add package TUnit
dotnet add package FluentAssertions
dotnet add package NSubstitute
dotnet add package coverlet.collector

# Integration tests
cd ../SuperDuperRescueHeads.Tests.Integration
dotnet add package TUnit
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers
dotnet add package coverlet.collector

# UI tests
cd ../SuperDuperRescueHeads.Tests.UI
dotnet add package TUnit
dotnet add package FluentAssertions
dotnet add package bUnit
dotnet add package coverlet.collector

# E2E tests
cd ../SuperDuperRescueHeads.Tests.E2E
dotnet add package TUnit
dotnet add package FluentAssertions
dotnet add package Microsoft.Playwright

# Contract tests
cd ../SuperDuperRescueHeads.Tests.Contract
dotnet add package TUnit
dotnet add package FluentAssertions
dotnet add package Newtonsoft.Json.Schema
```

### 4. Configure Aspire AppHost

Edit `SuperDuperRescueHeads.AppHost/Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("collections-db");

// Add API with database dependency
var api = builder.AddProject<Projects.SuperDuperRescueHeads_Api>("api")
    .WithReference(sqlServer)
    .WithExternalHttpEndpoints();

// Add Web UI with API dependency
builder.AddProject<Projects.SuperDuperRescueHeads_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

## Phase 1: Domain Layer (TDD - Red Phase)

### Step 1: Write Failing Tests for CollectionName Value Object

Create `SuperDuperRescueHeads.Tests.Unit/Domain/CollectionNameTests.cs`:

```csharp
using FluentAssertions;
using SuperDuperRescueHeads.Domain.Collections;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain;

public class CollectionNameTests
{
    [Test]
    public async Task Create_WithValidName_ShouldSucceed()
    {
        // Arrange
        var validName = "Classic Rock Vinyl";

        // Act
        var collectionName = CollectionName.Create(validName);

        // Assert
        collectionName.Value.Should().Be(validName);
    }

    [Test]
    public async Task Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyName = "";

        // Act
        Action act = () => CollectionName.Create(emptyName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Collection name cannot be empty*");
    }

    [Test]
    public async Task Create_WithNameExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        Action act = () => CollectionName.Create(longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Collection name cannot exceed 100 characters*");
    }

    [Test]
    public async Task Create_WithWhitespaceName_ShouldTrim()
    {
        // Arrange
        var nameWithWhitespace = "  Classic Rock  ";

        // Act
        var collectionName = CollectionName.Create(nameWithWhitespace);

        // Assert
        collectionName.Value.Should().Be("Classic Rock");
    }

    [Test]
    public async Task Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var name1 = CollectionName.Create("Jazz Collection");
        var name2 = CollectionName.Create("Jazz Collection");

        // Assert
        name1.Should().Be(name2);
    }
}
```

**Run tests**: They should all fail (RED PHASE) because CollectionName doesn't exist yet.

```bash
dotnet test SuperDuperRescueHeads.Tests.Unit
```

### Step 2: Implement CollectionName Value Object (Green Phase)

Create `SuperDuperRescueHeads.Domain/Common/ValueObject.cs` (base class):

```csharp
namespace SuperDuperRescueHeads.Domain.Common;

public abstract record ValueObject;
```

Create `SuperDuperRescueHeads.Domain/Collections/CollectionName.cs`:

```csharp
using SuperDuperRescueHeads.Domain.Common;

namespace SuperDuperRescueHeads.Domain.Collections;

public record CollectionName : ValueObject
{
    public string Value { get; }

    private CollectionName(string value)
    {
        Value = value;
    }

    public static CollectionName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Collection name cannot be empty", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > 100)
            throw new ArgumentException("Collection name cannot exceed 100 characters", nameof(value));

        return new CollectionName(trimmed);
    }
}
```

**Run tests**: All tests should now pass (GREEN PHASE).

### Step 3: Write Tests for ItemType Value Object

Create `SuperDuperRescueHeads.Tests.Unit/Domain/ItemTypeTests.cs`:

```csharp
using FluentAssertions;
using SuperDuperRescueHeads.Domain.Collections;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain;

public class ItemTypeTests
{
    [Test]
    public async Task VinylRecord_ShouldHaveCorrectIdAndName()
    {
        // Act
        var itemType = ItemType.VinylRecord;

        // Assert
        itemType.Id.Should().Be(1);
        itemType.Name.Should().Be("Vinyl Record");
    }

    [Test]
    public async Task FromId_WithValidId_ShouldReturnCorrectType()
    {
        // Act
        var itemType = ItemType.FromId(1);

        // Assert
        itemType.Should().Be(ItemType.VinylRecord);
    }

    [Test]
    public async Task FromId_WithInvalidId_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => ItemType.FromId(999);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid ItemType ID*");
    }
}
```

### Step 4: Implement ItemType Value Object

Create `SuperDuperRescueHeads.Domain/Collections/ItemType.cs`:

```csharp
using SuperDuperRescueHeads.Domain.Common;

namespace SuperDuperRescueHeads.Domain.Collections;

public record ItemType : ValueObject
{
    public int Id { get; }
    public string Name { get; }

    private ItemType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static ItemType VinylRecord => new(1, "Vinyl Record");
    public static ItemType ComicBook => new(2, "Comic Book");
    public static ItemType Puzzle => new(3, "Puzzle");

    public static ItemType FromId(int id) => id switch
    {
        1 => VinylRecord,
        2 => ComicBook,
        3 => Puzzle,
        _ => throw new ArgumentException($"Invalid ItemType ID: {id}", nameof(id))
    };

    public static IReadOnlyList<ItemType> GetAll() => new[]
    {
        VinylRecord,
        ComicBook,
        Puzzle
    };
}
```

**Run tests**: All ItemType tests should pass.

### Step 5: Write Tests for Collection Aggregate Root

Create `SuperDuperRescueHeads.Tests.Unit/Domain/CollectionTests.cs`:

```csharp
using FluentAssertions;
using SuperDuperRescueHeads.Domain.Collections;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain;

public class CollectionTests
{
    private readonly Guid _ownerId = Guid.NewGuid();

    [Test]
    public async Task Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = CollectionName.Create("Jazz Collection");
        var itemType = ItemType.VinylRecord;
        var description = "My jazz albums";

        // Act
        var collection = Collection.Create(name, itemType, _ownerId, description);

        // Assert
        collection.CollectionId.Should().NotBeEmpty();
        collection.Name.Should().Be(name);
        collection.ItemType.Should().Be(itemType);
        collection.OwnerId.Should().Be(_ownerId);
        collection.Description.Should().Be(description);
        collection.ItemCount.Should().Be(0);
        collection.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task UpdateName_WithValidName_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var collection = Collection.Create(
            CollectionName.Create("Old Name"),
            ItemType.VinylRecord,
            _ownerId);

        var newName = CollectionName.Create("New Name");

        // Act
        collection.UpdateName(newName);

        // Assert
        collection.Name.Should().Be(newName);
        collection.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task IsOwnedBy_WithCorrectOwner_ShouldReturnTrue()
    {
        // Arrange
        var collection = Collection.Create(
            CollectionName.Create("Test"),
            ItemType.VinylRecord,
            _ownerId);

        // Act
        var result = collection.IsOwnedBy(_ownerId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task IsOwnedBy_WithDifferentOwner_ShouldReturnFalse()
    {
        // Arrange
        var collection = Collection.Create(
            CollectionName.Create("Test"),
            ItemType.VinylRecord,
            _ownerId);

        var differentOwnerId = Guid.NewGuid();

        // Act
        var result = collection.IsOwnedBy(differentOwnerId);

        // Assert
        result.Should().BeFalse();
    }
}
```

### Step 6: Implement Collection Aggregate Root

Create `SuperDuperRescueHeads.Domain/Common/Entity.cs`:

```csharp
namespace SuperDuperRescueHeads.Domain.Common;

public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

Create `SuperDuperRescueHeads.Domain/Common/DomainEvent.cs`:

```csharp
using MediatR;

namespace SuperDuperRescueHeads.Domain.Common;

public abstract record DomainEvent : INotification
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

Create `SuperDuperRescueHeads.Domain/Collections/Collection.cs`:

```csharp
using SuperDuperRescueHeads.Domain.Common;

namespace SuperDuperRescueHeads.Domain.Collections;

public class Collection : Entity
{
    public Guid CollectionId { get; private set; }
    public CollectionName Name { get; private set; }
    public string? Description { get; private set; }
    public ItemType ItemType { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int ItemCount { get; private set; }

    // EF Core constructor
    private Collection() { }

    private Collection(
        Guid collectionId,
        CollectionName name,
        ItemType itemType,
        Guid ownerId,
        string? description,
        DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        Name = name;
        ItemType = itemType;
        OwnerId = ownerId;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ItemCount = 0;
    }

    public static Collection Create(
        CollectionName name,
        ItemType itemType,
        Guid ownerId,
        string? description = null)
    {
        var collectionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var collection = new Collection(collectionId, name, itemType, ownerId, description, now);

        collection.AddDomainEvent(new CollectionCreatedEvent(
            collectionId,
            name.Value,
            ownerId,
            now));

        return collection;
    }

    public void UpdateName(CollectionName newName)
    {
        var oldName = Name.Value;
        Name = newName;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CollectionNameChangedEvent(
            CollectionId,
            oldName,
            newName.Value,
            UpdatedAt));
    }

    public void UpdateDescription(string? newDescription)
    {
        if (newDescription?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(newDescription));

        Description = newDescription;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsOwnedBy(Guid userId) => OwnerId == userId;

    public bool CanBeDeletedBy(Guid userId) => IsOwnedBy(userId);
}
```

Create `SuperDuperRescueHeads.Domain/Collections/CollectionDomainEvents.cs`:

```csharp
using SuperDuperRescueHeads.Domain.Common;

namespace SuperDuperRescueHeads.Domain.Collections;

public record CollectionCreatedEvent(
    Guid CollectionId,
    string Name,
    Guid OwnerId,
    DateTimeOffset CreatedAt
) : DomainEvent;

public record CollectionNameChangedEvent(
    Guid CollectionId,
    string OldName,
    string NewName,
    DateTimeOffset UpdatedAt
) : DomainEvent;

public record CollectionDeletedEvent(
    Guid CollectionId,
    Guid OwnerId,
    DateTimeOffset DeletedAt
) : DomainEvent;
```

**Run all domain tests**: All should pass (GREEN PHASE complete for domain layer).

```bash
dotnet test SuperDuperRescueHeads.Tests.Unit --filter "FullyQualifiedName~.Domain"
```

## Phase 2: Infrastructure Layer

### Step 7: Create Repository Interface

Create `SuperDuperRescueHeads.Domain/Collections/ICollectionRepository.cs`:

```csharp
namespace SuperDuperRescueHeads.Domain.Collections;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Collection>> GetByOwnerIdAsync(Guid ownerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, CancellationToken cancellationToken = default);
    Task<Collection> AddAsync(Collection collection, CancellationToken cancellationToken = default);
    Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);
    Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Step 8: Implement EF Core DbContext and Configuration

Create `SuperDuperRescueHeads.Infrastructure/Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

Create `SuperDuperRescueHeads.Infrastructure/Data/Configurations/CollectionConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(c => c.CollectionId);

        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => CollectionName.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ItemType)
            .HasConversion(
                itemType => itemType.Id,
                id => ItemType.FromId(id))
            .HasColumnName("ItemTypeId")
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.OwnerId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.ItemCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(c => c.OwnerId)
            .HasDatabaseName("IX_Collections_OwnerId");

        builder.HasIndex(c => new { c.Name, c.OwnerId })
            .IsUnique()
            .HasDatabaseName("UQ_Collections_Name_OwnerId");

        builder.HasIndex(c => c.CreatedAt)
            .IsDescending()
            .HasDatabaseName("IX_Collections_CreatedAt");

        // Ignore domain events (not persisted)
        builder.Ignore(c => c.DomainEvents);
    }
}
```

### Step 9: Implement Repository

Create `SuperDuperRescueHeads.Infrastructure/Data/Repositories/CollectionRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Infrastructure.Data.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly ApplicationDbContext _context;

    public CollectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Collection?> GetByIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.Collections
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Collection>> GetByOwnerIdAsync(
        Guid ownerId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.Collections
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Collections
            .Where(c => c.OwnerId == ownerId)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAndOwnerAsync(
        string name,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Collections
            .AnyAsync(c => c.Name.Value == name && c.OwnerId == ownerId, cancellationToken);
    }

    public async Task<Collection> AddAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await _context.Collections.AddAsync(collection, cancellationToken);
        return collection;
    }

    public Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        _context.Collections.Update(collection);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        _context.Collections.Remove(collection);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Step 10: Create and Run Migrations

```bash
# Install EF Core CLI tool (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate --project SuperDuperRescueHeads.Infrastructure --startup-project SuperDuperRescueHeads.Api

# Update database
dotnet ef database update --project SuperDuperRescueHeads.Infrastructure --startup-project SuperDuperRescueHeads.Api
```

## Phase 3: API Layer

### Step 11: Create DTOs

Create `SuperDuperRescueHeads.Api/Models/CreateCollectionRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace SuperDuperRescueHeads.Api.Models;

public record CreateCollectionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    [Required]
    [Range(1, 3)]
    public required int ItemTypeId { get; init; }
}
```

Create similar DTOs for `UpdateCollectionRequest` and `CollectionResponse`.

### Step 12: Implement Minimal API Endpoints

Create `SuperDuperRescueHeads.Api/Endpoints/CollectionsEndpoints.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class CollectionsEndpoints
{
    public static IEndpointRouteBuilder MapCollectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/collections")
            .WithTags("Collections")
            .RequireAuthorization();

        group.MapGet("/", GetCollections)
            .WithName("GetCollections")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetCollectionById)
            .WithName("GetCollectionById")
            .WithOpenApi();

        group.MapPost("/", CreateCollection)
            .WithName("CreateCollection")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateCollection)
            .WithName("UpdateCollection")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteCollection)
            .WithName("DeleteCollection")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetCollections(
        [FromServices] ICollectionRepository repository,
        HttpContext context,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var userId = GetUserIdFromContext(context);
        var collections = await repository.GetByOwnerIdAsync(userId, skip, take);
        var total = await repository.CountByOwnerIdAsync(userId);

        var response = new
        {
            data = collections.Select(ToResponse),
            pagination = new
            {
                total,
                skip,
                take,
                hasMore = skip + take < total
            }
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCollectionById(
        [FromRoute] Guid id,
        [FromServices] ICollectionRepository repository,
        HttpContext context)
    {
        var userId = GetUserIdFromContext(context);
        var collection = await repository.GetByIdAsync(id);

        if (collection is null)
            return Results.NotFound(ProblemDetailsFactory.NotFound(id));

        if (!collection.IsOwnedBy(userId))
            return Results.Forbid();

        return Results.Ok(ToResponse(collection));
    }

    private static async Task<IResult> CreateCollection(
        [FromBody] CreateCollectionRequest request,
        [FromServices] ICollectionRepository repository,
        HttpContext context)
    {
        var userId = GetUserIdFromContext(context);

        // Check collection limit
        var count = await repository.CountByOwnerIdAsync(userId);
        if (count >= 100)
            return Results.Conflict(ProblemDetailsFactory.CollectionLimitExceeded(count, 100));

        // Check duplicate name
        if (await repository.ExistsByNameAndOwnerAsync(request.Name, userId))
            return Results.Conflict(ProblemDetailsFactory.DuplicateCollectionName(request.Name));

        // Create collection
        var collection = Collection.Create(
            CollectionName.Create(request.Name),
            ItemType.FromId(request.ItemTypeId),
            userId,
            request.Description);

        await repository.AddAsync(collection);
        await repository.SaveChangesAsync();

        return Results.Created($"/api/v1/collections/{collection.CollectionId}", ToResponse(collection));
    }

    // ... Implement UpdateCollection and DeleteCollection similarly ...

    private static Guid GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst("sub")?.Value
                          ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.Parse(userIdClaim!);
    }

    private static CollectionResponse ToResponse(Collection collection)
    {
        return new CollectionResponse
        {
            CollectionId = collection.CollectionId,
            Name = collection.Name.Value,
            Description = collection.Description,
            ItemType = new ItemTypeResponse
            {
                Id = collection.ItemType.Id,
                Name = collection.ItemType.Name
            },
            ItemCount = collection.ItemCount,
            CreatedAt = collection.CreatedAt,
            UpdatedAt = collection.UpdatedAt
        };
    }
}
```

### Step 13: Register Services in Program.cs

Edit `SuperDuperRescueHeads.Api/Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Api.Endpoints;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Infrastructure.Data;
using SuperDuperRescueHeads.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add DbContext
builder.AddSqlServerDbContext<ApplicationDbContext>("collections-db");

// Add repositories
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();

// Add authentication and authorization
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// Add OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCollectionsEndpoints();

app.Run();
```

## Phase 4: Web UI (Blazor)

### Step 14: Create Collection Service

Create `SuperDuperRescueHeads.Web/Services/CollectionService.cs`:

```csharp
using System.Net.Http.Json;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Web.Services;

public class CollectionService
{
    private readonly HttpClient _httpClient;

    public CollectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CollectionListResponse> GetCollectionsAsync(int skip = 0, int take = 20)
    {
        var response = await _httpClient.GetFromJsonAsync<CollectionListResponse>(
            $"api/v1/collections?skip={skip}&take={take}");

        return response!;
    }

    public async Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/collections", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CollectionResponse>())!;
    }

    // ... Add other methods ...
}
```

### Step 15: Create Blazor Components

Create `SuperDuperRescueHeads.Web/Components/Pages/Collections/Index.razor`:

```razor
@page "/collections"
@using SuperDuperRescueHeads.Web.Services
@inject CollectionService CollectionService

<PageTitle>My Collections</PageTitle>

<div class="container mx-auto px-4 py-8">
    <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold">My Collections</h1>
        <a href="/collections/create" class="btn btn-primary">Create Collection</a>
    </div>

    @if (_collections is null)
    {
        <p class="text-center py-8">Loading collections...</p>
    }
    else if (!_collections.Any())
    {
        <div class="text-center py-12">
            <p class="text-xl mb-4">You don't have any collections yet.</p>
            <a href="/collections/create" class="btn btn-primary">Create your first collection</a>
        </div>
    }
    else
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @foreach (var collection in _collections)
            {
                <CollectionCard Collection="@collection" />
            }
        </div>

        @if (_hasMore)
        {
            <div class="text-center mt-8">
                <button @onclick="LoadMore" class="btn btn-secondary">Load More</button>
            </div>
        }
    }
</div>

@code {
    private List<CollectionResponse>? _collections;
    private int _skip = 0;
    private int _take = 20;
    private bool _hasMore;

    protected override async Task OnInitializedAsync()
    {
        await LoadCollections();
    }

    private async Task LoadCollections()
    {
        var response = await CollectionService.GetCollectionsAsync(_skip, _take);
        _collections = response.Data.ToList();
        _hasMore = response.Pagination.HasMore;
    }

    private async Task LoadMore()
    {
        _skip += _take;
        var response = await CollectionService.GetCollectionsAsync(_skip, _take);
        _collections!.AddRange(response.Data);
        _hasMore = response.Pagination.HasMore;
    }
}
```

Create reusable component `SuperDuperRescueHeads.Web/Components/Shared/CollectionCard.razor`:

```razor
@using SuperDuperRescueHeads.Domain.Collections

<div class="card bg-white shadow-md rounded-lg p-6 hover:shadow-lg transition">
    <h3 class="text-xl font-semibold mb-2">@Collection.Name</h3>

    @if (!string.IsNullOrEmpty(Collection.Description))
    {
        <p class="text-gray-600 mb-4">@Collection.Description</p>
    }

    <div class="flex justify-between items-center">
        <span class="text-sm text-gray-500">@Collection.ItemType.Name</span>
        <span class="text-sm font-medium">@Collection.ItemCount items</span>
    </div>

    <div class="mt-4 flex gap-2">
        <a href="/collections/@Collection.CollectionId" class="btn btn-sm btn-secondary">View</a>
        <a href="/collections/@Collection.CollectionId/edit" class="btn btn-sm btn-outline">Edit</a>
    </div>
</div>

@code {
    [Parameter]
    public required CollectionResponse Collection { get; set; }
}
```

## Phase 5: Testing

### Step 16: Write Integration Tests

Create integration tests for API endpoints, repository, etc.

### Step 17: Write UI Component Tests (bUnit)

Create `SuperDuperRescueHeads.Tests.UI/Components/CollectionCardTests.cs`:

```csharp
using Bunit;
using FluentAssertions;
using SuperDuperRescueHeads.Web.Components.Shared;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.UI.Components;

public class CollectionCardTests : TestContext
{
    [Test]
    public void CollectionCard_RendersName()
    {
        // Arrange
        var collection = CreateTestCollection("Jazz Collection");

        // Act
        var component = RenderComponent<CollectionCard>(parameters => parameters
            .Add(p => p.Collection, collection));

        // Assert
        component.Find("h3").TextContent.Should().Be("Jazz Collection");
    }

    // ... more tests ...
}
```

### Step 18: Write E2E Tests (Playwright)

Create `SuperDuperRescueHeads.Tests.E2E/UserJourneys/CreateCollectionE2ETests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Playwright;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.E2E.UserJourneys;

public class CreateCollectionE2ETests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    [Test]
    public async Task User_CanCreateNewCollection()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync("http://localhost:5000/collections/create");

        // Act
        await page.FillAsync("input[name='name']", "Test Collection");
        await page.SelectOptionAsync("select[name='itemTypeId']", "1");
        await page.ClickAsync("button[type='submit']");

        // Assert
        await page.WaitForURLAsync("**/collections");
        var heading = page.Locator("h3:has-text('Test Collection')");
        await heading.Should().BeVisibleAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser?.CloseAsync()!;
        _playwright?.Dispose();
    }
}
```

## Phase 6: Deployment

### Step 19: Run Aspire Locally

```bash
dotnet run --project SuperDuperRescueHeads.AppHost
```

Open the Aspire dashboard (usually http://localhost:15000) to view all services, logs, and traces.

### Step 20: Deploy to Azure

```bash
# Install Azure Developer CLI
winget install microsoft.azd

# Initialize Azure deployment
azd init

# Provision infrastructure and deploy
azd up
```

## Summary

This quickstart provides a comprehensive, step-by-step guide to implementing the Core Collection Management feature following TDD and DDD principles. Each phase builds on the previous one, ensuring tests are written first (RED), implementation follows (GREEN), and refactoring improves the design.

**Next Steps**:
1. Complete all test coverage (target 80%+)
2. Add logging and telemetry
3. Configure CI/CD pipeline
4. Review and merge to main branch
5. Deploy to staging for QA testing

For detailed architecture diagrams, see `/docs/architecture/`.
For API contracts, see `./contracts/collections-api.yaml`.
For data model details, see `./data-model.md`.

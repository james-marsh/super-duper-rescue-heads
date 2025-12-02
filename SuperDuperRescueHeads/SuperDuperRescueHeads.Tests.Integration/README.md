# Integration Tests

## Purpose
Integration tests verify that components work together correctly, focusing on database interactions, repositories, and service integrations.

## Framework
- **Test Framework**: TUnit 0.3.0
- **Database**: EF Core InMemory (for fast tests)
- **Future**: Testcontainers for SQL Server integration

## Test Structure

### Repository Tests
Located in `Infrastructure/Repositories/` folder:
- `ItemRepositoryTests.cs` - Tests for ItemRepository with real DbContext

### Test Pattern
```csharp
[Test]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    await using var context = CreateInMemoryDbContext();
    var repository = new ItemRepository(context);
    var item = CreateTestItem();

    // Act
    await repository.AddAsync(item);
    await repository.SaveChangesAsync();

    // Assert
    var saved = await repository.GetByIdAsync(item.ItemId);
    Assert.That(saved).IsNotNull();
}
```

### Database Setup
Each test uses a unique in-memory database:
```csharp
private ApplicationDbContext CreateInMemoryDbContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    return new ApplicationDbContext(options);
}
```

### What to Test
1. **Repositories**
   - CRUD operations
   - Query filters (e.g., soft delete)
   - Pagination
   - Complex queries
   - Transaction handling

2. **Services**
   - End-to-end service flows
   - Database state changes
   - Error handling with real data

3. **Background Jobs**
   - Job execution
   - Database cleanup
   - Scheduled tasks

## Running Tests

### Command Line
```bash
dotnet test SuperDuperRescueHeads.Tests.Integration.csproj
```

### Parallel Execution
Integration tests can run in parallel since each uses a unique in-memory database.

## Migration to Testcontainers

For more realistic database testing, consider migrating to Testcontainers:

```csharp
// Example with Testcontainers
private async Task<ApplicationDbContext> CreateTestContainerDbContext()
{
    var container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    await container.StartAsync();

    var connectionString = container.GetConnectionString();
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    var context = new ApplicationDbContext(options);
    await context.Database.MigrateAsync();

    return context;
}
```

## Best Practices
1. **Isolation**: Each test uses its own database instance
2. **Cleanup**: Using statements ensure context disposal
3. **Realistic Data**: Use representative test data
4. **Fast**: InMemory database keeps tests fast
5. **Deterministic**: Seed data for predictable results

## Performance
- **InMemory Tests**: < 100ms per test
- **Testcontainer Tests**: < 2s per test (includes container startup)

## Coverage Areas
- Database operations (CRUD)
- Query filters and indexes
- Transactions and concurrency
- Data validation at persistence layer

# Unit Tests

## Purpose
Unit tests verify the behavior of individual components in isolation, focusing on domain entities, value objects, and services with mocked dependencies.

## Framework
- **Test Framework**: TUnit 0.3.0
- **Assertions**: TUnit.Assertions

## Test Structure

### Domain Tests
Located in `Domain/` folder, testing domain entities and value objects:
- `Items/ItemTests.cs` - Tests for Item aggregate behavior
- `Items/ItemNameTests.cs` - Tests for ItemName value object

### Test Pattern
```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var item = CreateTestItem();

    // Act
    var result = item.DoSomething();

    // Assert
    Assert.That(result).IsTrue();
}
```

### Naming Conventions
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedBehavior}`
- Use descriptive names that explain what is being tested
- Group related tests in the same class

### What to Test
1. **Domain Entities**
   - Creation with valid/invalid data
   - Business rule enforcement
   - State transitions
   - Domain events

2. **Value Objects**
   - Validation rules
   - Equality comparisons
   - Immutability

3. **Services**
   - Business logic with mocked dependencies
   - Exception handling
   - Edge cases

### Test Data
- Use helper methods like `CreateTestItem()` for common test data
- Use meaningful test data that represents real scenarios
- Avoid magic numbers - use descriptive constants

## Running Tests

### Visual Studio
- Test Explorer → Run All Tests

### Command Line
```bash
dotnet test SuperDuperRescueHeads.Tests.Unit.csproj
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Coverage Target
- **Minimum**: 70% code coverage
- **Recommended**: 80%+ code coverage
- Focus on critical business logic and domain entities

## Best Practices
1. **Isolation**: Each test should be independent
2. **Fast**: Unit tests should run in milliseconds
3. **Deterministic**: Same input = same output every time
4. **Single Responsibility**: Test one thing per test
5. **Readable**: Tests are documentation - make them clear
6. **No External Dependencies**: No database, network, file system

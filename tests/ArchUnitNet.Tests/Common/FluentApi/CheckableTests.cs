using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using Xunit;

namespace ArchUnitNet.Tests.Common.FluentApi;

public class CheckableTests
{
    // Simple mock implementation for testing the interface contract
    private class MockCheckable : Checkable
    {
        private readonly IReadOnlyList<Violation> _violations;

        public MockCheckable(params Violation[] violations)
        {
            _violations = violations;
        }

        public Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
        {
            return Task.FromResult(_violations);
        }
    }

    [Fact]
    public async Task CheckAsync_ReturnsEmptyListWhenNoViolations()
    {
        // Arrange
        var checkable = new MockCheckable();

        // Act
        var violations = await checkable.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task CheckAsync_ReturnsViolationsWhenPresent()
    {
        // Arrange
        var violation = new EmptyTestViolation("test");
        var checkable = new MockCheckable(violation);

        // Act
        var violations = await checkable.CheckAsync();

        // Assert
        Assert.Single(violations);
        Assert.Equal(violation, violations.First());
    }

    [Fact]
    public async Task CheckAsync_ReturnsReadOnlyList()
    {
        // Arrange
        var violation = new EmptyTestViolation("test");
        var checkable = new MockCheckable(violation);

        // Act
        var violations = await checkable.CheckAsync();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<Violation>>(violations);
    }

    [Fact]
    public async Task CheckAsync_AcceptsCheckOptions()
    {
        // Arrange
        var checkable = new MockCheckable();
        var options = new CheckOptions(AllowEmptyTests: true);

        // Act
        var violations = await checkable.CheckAsync(options);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void CheckAsync_IsAsync()
    {
        // Arrange
        var checkable = new MockCheckable();

        // Act
        var task = checkable.CheckAsync();

        // Assert
        Assert.IsAssignableFrom<Task<IReadOnlyList<Violation>>>(task);
    }
}

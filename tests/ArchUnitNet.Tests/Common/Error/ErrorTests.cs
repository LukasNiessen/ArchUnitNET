using ArchUnitNet.Common.Error;

namespace ArchUnitNet.Tests.Common.Error;

public class TechnicalErrorTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Database connection failed";

        // Act
        var error = new TechnicalError(message);

        // Assert
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Failed to extract graph";

        // Act
        var error = new TechnicalError(message, innerException);

        // Assert
        error.Message.Should().Be(message);
        error.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ShouldBeThrowable()
    {
        // Arrange
        var error = new TechnicalError("Test error");

        // Act & Assert
        ((Action)(() => throw error)).Should().Throw<TechnicalError>();
    }
}

public class UserErrorTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Pattern is invalid";

        // Act
        var error = new UserError(message);

        // Assert
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Bad argument");
        var message = "ProjectFiles() called with invalid path";

        // Act
        var error = new UserError(message, innerException);

        // Assert
        error.Message.Should().Be(message);
        error.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ShouldBeThrowable()
    {
        // Arrange
        var error = new UserError("Test error");

        // Act & Assert
        ((Action)(() => throw error)).Should().Throw<UserError>();
    }
}

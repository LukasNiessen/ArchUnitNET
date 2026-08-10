using ArchUnitNet.Common.Error;
using Xunit;

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
        Assert.Equal(message, error.Message);
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
        Assert.Equal(message, error.Message);
        Assert.Equal(innerException, error.InnerException);
    }

    [Fact]
    public void ShouldBeThrowable()
    {
        // Arrange
        var error = new TechnicalError("Test error");

        // Act & Assert
        try
        {
            throw error;
        }
        catch (TechnicalError ex)
        {
            Assert.Equal("Test error", ex.Message);
        }
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
        Assert.Equal(message, error.Message);
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
        Assert.Equal(message, error.Message);
        Assert.Equal(innerException, error.InnerException);
    }

    [Fact]
    public void ShouldBeThrowable()
    {
        // Arrange
        var error = new UserError("Test error");

        // Act & Assert
        try
        {
            throw error;
        }
        catch (UserError ex)
        {
            Assert.Equal("Test error", ex.Message);
        }
    }
}

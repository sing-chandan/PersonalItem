using FluentAssertions;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Ok_WithoutData_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Ok();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Operation successful");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ok_WithMessage_ShouldCreateSuccessResultWithMessage()
    {
        // Act
        var result = Result.Ok("Operation completed successfully");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Operation completed successfully");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithMessage_ShouldCreateFailureResult()
    {
        // Act
        var result = Result.Fail("Operation failed");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Operation failed");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithErrors_ShouldCreateFailureResultWithErrors()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result.Fail("Validation failed", errors);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void GenericOk_WithData_ShouldCreateSuccessResultWithData()
    {
        // Arrange
        var testData = new { Id = 1, Name = "Test" };

        // Act
        var result = Result<object>.Ok(testData);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(testData);
        result.Message.Should().Be("Operation successful");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void GenericOk_WithDataAndMessage_ShouldCreateSuccessResultWithDataAndMessage()
    {
        // Arrange
        var testData = "Test Data";

        // Act
        var result = Result<string>.Ok(testData, "Data retrieved successfully");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(testData);
        result.Message.Should().Be("Data retrieved successfully");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void GenericFail_WithMessage_ShouldCreateFailureResultWithNullData()
    {
        // Act
        var result = Result<string>.Fail("Operation failed");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Operation failed");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void GenericFail_WithErrors_ShouldCreateFailureResultWithErrors()
    {
        // Arrange
        var errors = new List<string> { "Validation error 1", "Validation error 2" };

        // Act
        var result = Result<int>.Fail("Validation failed", errors);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().Be(0); // Default value
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().HaveCount(2);
    }
}

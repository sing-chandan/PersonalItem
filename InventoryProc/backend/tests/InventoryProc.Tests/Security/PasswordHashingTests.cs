using FluentAssertions;
using BCrypt.Net;

namespace InventoryProc.Tests.Security;

public class PasswordHashingTests
{
    [Fact]
    public void HashPassword_ShouldCreateValidHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password); // Hash should be different from original
        hash.Should().StartWith("$2"); // BCrypt hashes start with $2a, $2b, or $2y
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var result = BCrypt.Net.BCrypt.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var result = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("Admin@123")]
    [InlineData("admin1")]
    [InlineData("SuperSecurePassword!@#123")]
    [InlineData("")]
    public void HashAndVerify_WithVariousPasswords_ShouldWork(string password)
    {
        // Arrange & Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var verifyCorrect = BCrypt.Net.BCrypt.Verify(password, hash);
        var verifyWrong = BCrypt.Net.BCrypt.Verify(password + "wrong", hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        verifyCorrect.Should().BeTrue("correct password should verify");
        verifyWrong.Should().BeFalse("incorrect password should not verify");
    }

    [Fact]
    public void HashPassword_MultipleHashesOfSamePassword_ShouldBeDifferent()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = BCrypt.Net.BCrypt.HashPassword(password);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "BCrypt should generate different salts");
        BCrypt.Net.BCrypt.Verify(password, hash1).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_CaseSensitive_ShouldWork()
    {
        // Arrange
        var password = "TestPassword";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var lowerCaseResult = BCrypt.Net.BCrypt.Verify("testpassword", hash);
        var upperCaseResult = BCrypt.Net.BCrypt.Verify("TESTPASSWORD", hash);
        var correctCaseResult = BCrypt.Net.BCrypt.Verify("TestPassword", hash);

        // Assert
        lowerCaseResult.Should().BeFalse("password is case-sensitive");
        upperCaseResult.Should().BeFalse("password is case-sensitive");
        correctCaseResult.Should().BeTrue("exact match should work");
    }

    [Fact]
    public void SimulateUserCreationAndLogin_ShouldWork()
    {
        // This simulates the exact flow in the application

        // Arrange - User creation (from UserManagementService.CreateUserAsync)
        var originalPassword = "Admin@123";
        var passwordHashDuringCreation = BCrypt.Net.BCrypt.HashPassword(originalPassword);

        // Act - User login (from AuthenticationService.LoginAsync)
        var loginPassword = "Admin@123"; // Same password user enters
        var loginSuccessful = BCrypt.Net.BCrypt.Verify(loginPassword, passwordHashDuringCreation);

        // Assert
        loginSuccessful.Should().BeTrue("user should be able to login with the same password");
    }

    [Fact]
    public void SimulateUserCreationAndLogin_WithWrongPassword_ShouldFail()
    {
        // Arrange - User creation
        var originalPassword = "Admin@123";
        var passwordHashDuringCreation = BCrypt.Net.BCrypt.HashPassword(originalPassword);

        // Act - User login with wrong password
        var wrongPassword = "Admin@124"; // Slightly different
        var loginSuccessful = BCrypt.Net.BCrypt.Verify(wrongPassword, passwordHashDuringCreation);

        // Assert
        loginSuccessful.Should().BeFalse("user should not be able to login with wrong password");
    }
}

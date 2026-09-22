using FluentAssertions;
using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.Modules.Identity.Application.Services;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Enums;
using InventoryProc.Modules.Tenancy.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InventoryProc.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly DbContext _context;
    private readonly AuthenticationService _sut;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationServiceTests()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);

        _jwtSettings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatNeedsToBeAtLeast32CharactersLong123456",
            Issuer = "Test",
            Audience = "Test",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        _sut = new AuthenticationService(_context, Options.Create(_jwtSettings));
    }

    [Fact]
    public async Task LoginAsync_WhenUserExists_ShouldReturnSuccess()
    {
        // Arrange
        var tenant = new Tenant("TEST", "Test Tenant", "Test Company", "admin@test.com");
        _context.Add(tenant);
        await _context.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123");
        var user = new User(tenant.Id, "test@test.com", passwordHash, "Test", "User", UserRole.Admin);
        _context.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Test@123"
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.TenantId.Should().Be(tenant.Id);
        result.Data.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "Test@123"
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var tenant = new Tenant("TEST", "Test Tenant", "Test Company", "admin@test.com");
        _context.Add(tenant);
        await _context.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User(tenant.Id, "test@test.com", passwordHash, "Test", "User", UserRole.Admin);
        _context.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WhenUserInactive_ShouldReturnFailure()
    {
        // Arrange
        var tenant = new Tenant("TEST", "Test Tenant", "Test Company", "admin@test.com");
        _context.Add(tenant);
        await _context.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123");
        var user = new User(tenant.Id, "test@test.com", passwordHash, "Test", "User", UserRole.Admin);
        user.Deactivate();
        _context.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Test@123"
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User account is deactivated");
    }

    [Fact]
    public async Task LoginAsync_WithCaseInsensitiveEmail_ShouldSucceed()
    {
        // Arrange
        var tenant = new Tenant("TEST", "Test Tenant", "Test Company", "admin@test.com");
        _context.Add(tenant);
        await _context.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123");
        var user = new User(tenant.Id, "test@test.com", passwordHash, "Test", "User", UserRole.Admin);
        _context.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "TEST@TEST.COM",
            Password = "Test@123"
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // Test DbContext without tenant filtering
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations without tenant filter for testing
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Tenant).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(User).Assembly);
        }
    }
}

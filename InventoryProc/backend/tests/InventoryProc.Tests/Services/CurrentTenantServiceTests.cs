using FluentAssertions;
using InventoryProc.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace InventoryProc.Tests.Services;

public class CurrentTenantServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentTenantService _sut;

    public CurrentTenantServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sut = new CurrentTenantService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void TenantId_WhenTenantIdClaimExists_ShouldReturnTenantId()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("tenantId", expectedTenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _sut.TenantId;

        // Assert
        result.Should().Be(expectedTenantId);
        _sut.IsSet.Should().BeTrue();
    }

    [Fact]
    public void TenantId_WhenNoHttpContext_ShouldReturnEmpty()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        // Act
        var result = _sut.TenantId;

        // Assert
        result.Should().Be(Guid.Empty);
        _sut.IsSet.Should().BeFalse();
    }

    [Fact]
    public void TenantId_WhenNoTenantIdClaim_ShouldReturnEmpty()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("email", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _sut.TenantId;

        // Assert
        result.Should().Be(Guid.Empty);
        _sut.IsSet.Should().BeFalse();
    }

    [Fact]
    public void TenantId_WhenInvalidGuidInClaim_ShouldReturnEmpty()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("tenantId", "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _sut.TenantId;

        // Assert
        result.Should().Be(Guid.Empty);
        _sut.IsSet.Should().BeFalse();
    }
}

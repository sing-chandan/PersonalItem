using InventoryProc.Infrastructure.Persistence;
using InventoryProc.SharedKernel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DebugController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        ApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,
        ILogger<DebugController> logger)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Debug endpoint to check tenant and user context
    /// </summary>
    [HttpGet("context")]
    public async Task<IActionResult> GetContext()
    {
        var tenantId = _currentTenantService.TenantId;
        var userId = _currentUserService.UserId;
        var email = _currentUserService.Email;
        var isAuthenticated = _currentUserService.IsAuthenticated;

        _logger.LogInformation("Debug Context - TenantId: {TenantId}, UserId: {UserId}, Email: {Email}, IsAuth: {IsAuth}",
            tenantId, userId, email, isAuthenticated);

        // Get counts with and without tenant filter
        var productsWithFilter = await _context.Products.CountAsync();
        var productsWithoutFilter = await _context.Products.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).CountAsync();
        var productsTotal = await _context.Products.IgnoreQueryFilters().CountAsync();

        var customersWithFilter = await _context.Customers.CountAsync();
        var customersWithoutFilter = await _context.Customers.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).CountAsync();

        var salesOrdersWithFilter = await _context.SalesOrders.CountAsync();
        var salesOrdersWithoutFilter = await _context.SalesOrders.IgnoreQueryFilters().Where(so => so.TenantId == tenantId).CountAsync();

        var invoicesWithFilter = await _context.Invoices.CountAsync();
        var invoicesWithoutFilter = await _context.Invoices.IgnoreQueryFilters().Where(i => i.TenantId == tenantId).CountAsync();

        // Get user details
        var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        var tenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);

        return Ok(new
        {
            CurrentContext = new
            {
                TenantId = tenantId,
                UserId = userId,
                Email = email,
                IsAuthenticated = isAuthenticated,
                TenantIsSet = _currentTenantService.IsSet
            },
            UserDetails = user != null ? new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.TenantId,
                user.Role
            } : null,
            TenantDetails = tenant != null ? new
            {
                tenant.Id,
                tenant.Name,
                tenant.Code
            } : null,
            DataCounts = new
            {
                Products = new { WithFilter = productsWithFilter, WithoutFilter = productsWithoutFilter, Total = productsTotal },
                Customers = new { WithFilter = customersWithFilter, WithoutFilter = customersWithoutFilter },
                SalesOrders = new { WithFilter = salesOrdersWithFilter, WithoutFilter = salesOrdersWithoutFilter },
                Invoices = new { WithFilter = invoicesWithFilter, WithoutFilter = invoicesWithoutFilter }
            },
            JwtClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    /// <summary>
    /// Get raw products data for current tenant
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var tenantId = _currentTenantService.TenantId;

        var products = await _context.Products
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.SalePrice,
                p.CurrentStock,
                p.TenantId
            })
            .ToListAsync();

        return Ok(new
        {
            CurrentTenantId = tenantId,
            Count = products.Count,
            Products = products
        });
    }

    /// <summary>
    /// Get raw customers data for current tenant
    /// </summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var tenantId = _currentTenantService.TenantId;

        var customers = await _context.Customers
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.CustomerCode,
                c.CompanyName,
                c.Email,
                c.TenantId
            })
            .ToListAsync();

        return Ok(new
        {
            CurrentTenantId = tenantId,
            Count = customers.Count,
            Customers = customers
        });
    }
}

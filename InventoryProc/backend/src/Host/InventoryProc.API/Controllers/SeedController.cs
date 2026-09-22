using InventoryProc.Infrastructure.Data;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // For development only - remove in production
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSeeder _seeder;
    private readonly ILogger<SeedController> _logger;

    public SeedController(
        ApplicationDbContext context,
        DatabaseSeeder seeder,
        ILogger<SeedController> logger)
    {
        _context = context;
        _seeder = seeder;
        _logger = logger;
    }

    /// <summary>
    /// Run the full database seeder
    /// </summary>
    [HttpPost("seed-all")]
    public async Task<IActionResult> SeedAll()
    {
        try
        {
            await _seeder.SeedAsync();
            return Ok(new { message = "Database seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Add more products to the database
    /// </summary>
    [HttpPost("add-products")]
    public async Task<IActionResult> AddProducts([FromQuery] int count = 5)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync();
            if (tenant == null)
            {
                return BadRequest(new { error = "No tenant found. Run seed-all first." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id);
            if (user == null)
            {
                return BadRequest(new { error = "No user found. Run seed-all first." });
            }

            var categories = await _context.Categories.Where(c => c.TenantId == tenant.Id).ToListAsync();
            var brands = await _context.Brands.Where(b => b.TenantId == tenant.Id).ToListAsync();

            if (!categories.Any() || !brands.Any())
            {
                return BadRequest(new { error = "No categories or brands found. Run seed-all first." });
            }

            var random = new Random();
            var products = new List<Product>();

            for (int i = 0; i < count; i++)
            {
                var category = categories[random.Next(categories.Count)];
                var brand = brands[random.Next(brands.Count)];

                var productCode = $"PROD-{DateTime.Now.Ticks}-{i}";
                var productName = $"Demo Product {DateTime.Now.Ticks}-{i}";

                var costPrice = random.Next(100, 5000);
                var wholesalePrice = costPrice + random.Next(50, 500);
                var salePrice = wholesalePrice + random.Next(100, 1000);

                var product = new Product(
                    tenant.Id,
                    productName,
                    productCode,
                    "PCS",
                    costPrice,
                    wholesalePrice,
                    salePrice,
                    category.Id)
                {
                    Description = $"Demo product created via seed API",
                    BrandId = brand.Id,
                    MinStockLevel = random.Next(5, 20),
                    MaxStockLevel = random.Next(50, 200),
                    CurrentStock = random.Next(10, 100),
                    TaxRate = random.Next(0, 3) switch
                    {
                        0 => 0,
                        1 => 12,
                        _ => 18
                    },
                    HSNCode = random.Next(1000, 9999).ToString(),
                    Barcode = $"BAR{DateTime.Now.Ticks}{i}",
                    CreatedBy = user.Id
                };

                products.Add(product);
            }

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Added {products.Count} products successfully",
                products = products.Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Name,
                    p.SalePrice,
                    p.CurrentStock
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding products");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Add more customers to the database
    /// </summary>
    [HttpPost("add-customers")]
    public async Task<IActionResult> AddCustomers([FromQuery] int count = 3)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync();
            if (tenant == null)
            {
                return BadRequest(new { error = "No tenant found. Run seed-all first." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id);
            if (user == null)
            {
                return BadRequest(new { error = "No user found. Run seed-all first." });
            }

            var random = new Random();
            var customers = new List<Customer>();

            var cities = new[] { "Mumbai", "Delhi", "Bangalore", "Chennai", "Kolkata", "Pune", "Hyderabad", "Ahmedabad" };
            var states = new[] { "Maharashtra", "Delhi", "Karnataka", "Tamil Nadu", "West Bengal", "Maharashtra", "Telangana", "Gujarat" };

            for (int i = 0; i < count; i++)
            {
                var customerCode = $"CUST-{DateTime.Now.Ticks}-{i}";
                var companyName = $"Demo Customer Ltd {DateTime.Now.Ticks}-{i}";
                var email = $"demo{DateTime.Now.Ticks}{i}@customer.com";

                var cityIndex = random.Next(cities.Length);

                var customer = new Customer(
                    tenant.Id,
                    customerCode,
                    companyName,
                    email,
                    random.Next(50000, 200000),
                    random.Next(15, 60))
                {
                    ContactPerson = $"Contact Person {i}",
                    Phone = $"022-{random.Next(10000000, 99999999)}",
                    Mobile = $"9{random.Next(100000000, 999999999)}",
                    Address = $"{random.Next(1, 999)} Street {i}",
                    City = cities[cityIndex],
                    State = states[cityIndex],
                    Country = "India",
                    Pincode = $"{random.Next(100000, 999999)}",
                    GSTNumber = $"{random.Next(10, 37):D2}AABC{random.Next(1000, 9999)}D1Z{random.Next(1, 9)}",
                    PANNumber = $"AABC{random.Next(1000, 9999)}D",
                    CreatedBy = user.Id
                };

                customers.Add(customer);
            }

            _context.Customers.AddRange(customers);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Added {customers.Count} customers successfully",
                customers = customers.Select(c => new
                {
                    c.Id,
                    c.CustomerCode,
                    c.CompanyName,
                    c.Email,
                    c.City
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customers");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Add sales orders with random data
    /// </summary>
    [HttpPost("add-sales-orders")]
    public async Task<IActionResult> AddSalesOrders([FromQuery] int count = 5)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync();
            if (tenant == null)
            {
                return BadRequest(new { error = "No tenant found. Run seed-all first." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id);
            if (user == null)
            {
                return BadRequest(new { error = "No user found. Run seed-all first." });
            }

            var customers = await _context.Customers.Where(c => c.TenantId == tenant.Id).ToListAsync();
            var products = await _context.Products.Where(p => p.TenantId == tenant.Id && p.IsActive).ToListAsync();

            if (!customers.Any() || !products.Any())
            {
                return BadRequest(new { error = "No customers or products found. Run seed-all first." });
            }

            var random = new Random();
            var orders = new List<SalesOrder>();

            for (int i = 0; i < count; i++)
            {
                var customer = customers[random.Next(customers.Count)];
                var orderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks}-{i}";
                var orderDate = DateTime.UtcNow.AddDays(-random.Next(0, 30));

                var order = new SalesOrder(
                    tenant.Id,
                    orderNumber,
                    customer.Id,
                    customer.CompanyName,
                    orderDate)
                {
                    Notes = $"Demo order created via seed API",
                    ShippingAddress = customer.Address,
                    BillingAddress = customer.Address,
                    CreatedBy = user.Id
                };

                // Add random number of items (1-5)
                var itemCount = random.Next(1, 6);
                var selectedProducts = products.OrderBy(x => random.Next()).Take(itemCount).ToList();

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(1, 20);
                    var item = new SalesOrderItem(
                        product.Id,
                        product.Name,
                        product.Code,
                        quantity,
                        product.Unit,
                        product.SalePrice,
                        product.TaxRate ?? 0);

                    order.AddItem(item);
                }

                // Randomly confirm some orders
                if (random.Next(0, 2) == 1)
                {
                    order.Confirm();

                    // Randomly ship some confirmed orders
                    if (random.Next(0, 2) == 1)
                    {
                        order.Ship(orderDate.AddDays(random.Next(1, 5)));
                    }
                }

                orders.Add(order);
            }

            _context.SalesOrders.AddRange(orders);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Added {orders.Count} sales orders successfully",
                orders = orders.Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CustomerName,
                    o.TotalAmount,
                    o.Status,
                    ItemCount = o.Items.Count
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding sales orders");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get database statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync();
            if (tenant == null)
            {
                return Ok(new { message = "No data found. Run seed-all first." });
            }

            var stats = new
            {
                Tenants = await _context.Tenants.CountAsync(),
                Users = await _context.Users.CountAsync(),
                Categories = await _context.Categories.Where(c => c.TenantId == tenant.Id).CountAsync(),
                Brands = await _context.Brands.Where(b => b.TenantId == tenant.Id).CountAsync(),
                Products = await _context.Products.Where(p => p.TenantId == tenant.Id).CountAsync(),
                Customers = await _context.Customers.Where(c => c.TenantId == tenant.Id).CountAsync(),
                SalesOrders = await _context.SalesOrders.Where(so => so.TenantId == tenant.Id).CountAsync(),
                Invoices = await _context.Invoices.Where(i => i.TenantId == tenant.Id).CountAsync(),
                TenantInfo = new
                {
                    tenant.Id,
                    tenant.Name,
                    tenant.Code
                }
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

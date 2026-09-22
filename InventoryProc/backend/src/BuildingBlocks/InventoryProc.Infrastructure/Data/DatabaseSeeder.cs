using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.Modules.Tenancy.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace InventoryProc.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Get or create demo tenant for development
            var tenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
            if (tenant == null)
            {
                _logger.LogInformation("No tenant found. Creating demo tenant...");

                // Create demo tenant
                tenant = new Tenant("demo-company", "Demo Company", "Demo Company Pvt Ltd", "admin@demo.com");
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Demo tenant created: {TenantName}", tenant.Name);
            }

            var tenantId = tenant.Id;

            // Get or create demo admin user
            var user = await _context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TenantId == tenantId);

            if (user == null)
            {
                _logger.LogInformation("No user found for tenant. Creating demo admin user...");

                // Create demo admin user
                user = new User(
                    tenantId,
                    "admin@demo.com",
                    BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    "Admin",
                    "User",
                    UserRole.Admin
                );
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Demo admin user created: {Email}", user.Email);
            }

            var userId = user.Id;

            // Seed Categories
            await SeedCategoriesAsync(tenantId, userId);

            // Seed Brands
            await SeedBrandsAsync(tenantId, userId);

            // Seed Products
            await SeedProductsAsync(tenantId, userId);

            // Seed Customers
            await SeedCustomersAsync(tenantId, userId);

            // Seed Sales Orders
            await SeedSalesOrdersAsync(tenantId, userId);

            // Seed Invoices
            await SeedInvoicesAsync(tenantId, userId);

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database.");
            throw;
        }
    }

    private async Task<Guid> GetFirstUserId(Guid tenantId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
        return user?.Id ?? Guid.NewGuid();
    }

    private async Task SeedCategoriesAsync(Guid tenantId, Guid userId)
    {
        if (await _context.Categories.IgnoreQueryFilters().AnyAsync(c => c.TenantId == tenantId))
        {
            _logger.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        var categories = new List<Category>
        {
            new Category(tenantId, "Electronics", "ELEC", null)
            {
                Description = "Electronic devices and accessories",
                CreatedBy = userId
            },
            new Category(tenantId, "Food & Beverages", "FOOD", null)
            {
                Description = "Food items and beverages",
                CreatedBy = userId
            },
            new Category(tenantId, "Clothing", "CLTH", null)
            {
                Description = "Apparel and fashion items",
                CreatedBy = userId
            },
            new Category(tenantId, "Home & Kitchen", "HOME", null)
            {
                Description = "Home appliances and kitchen items",
                CreatedBy = userId
            },
            new Category(tenantId, "Stationery", "STAT", null)
            {
                Description = "Office and school supplies",
                CreatedBy = userId
            },
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {categories.Count} categories.");
    }

    private async Task SeedBrandsAsync(Guid tenantId, Guid userId)
    {
        if (await _context.Brands.IgnoreQueryFilters().AnyAsync(b => b.TenantId == tenantId))
        {
            _logger.LogInformation("Brands already exist. Skipping brand seeding.");
            return;
        }

        var brands = new List<Brand>
        {
            new Brand(tenantId, "Samsung", "SAMS")
            {
                Description = "Electronics manufacturer",
                CreatedBy = userId
            },
            new Brand(tenantId, "Sony", "SONY")
            {
                Description = "Consumer electronics",
                CreatedBy = userId
            },
            new Brand(tenantId, "Nestle", "NEST")
            {
                Description = "Food and beverage company",
                CreatedBy = userId
            },
            new Brand(tenantId, "Nike", "NIKE")
            {
                Description = "Sports and apparel brand",
                CreatedBy = userId
            },
            new Brand(tenantId, "Pentel", "PENS")
            {
                Description = "Stationery brand",
                CreatedBy = userId
            },
        };

        _context.Brands.AddRange(brands);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {brands.Count} brands.");
    }

    private async Task SeedProductsAsync(Guid tenantId, Guid userId)
    {
        if (await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.TenantId == tenantId))
        {
            _logger.LogInformation("Products already exist. Skipping product seeding.");
            return;
        }

        var categories = await _context.Categories.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).ToListAsync();
        var brands = await _context.Brands.IgnoreQueryFilters().Where(b => b.TenantId == tenantId).ToListAsync();

        if (!categories.Any() || !brands.Any())
        {
            _logger.LogWarning("No categories or brands found. Skipping product seeding.");
            return;
        }

        var electronicsCategory = categories.FirstOrDefault(c => c.Code == "ELEC");
        var foodCategory = categories.FirstOrDefault(c => c.Code == "FOOD");
        var clothingCategory = categories.FirstOrDefault(c => c.Code == "CLTH");
        var stationeryCategory = categories.FirstOrDefault(c => c.Code == "STAT");

        var samsungBrand = brands.FirstOrDefault(b => b.Code == "SAMS");
        var sonyBrand = brands.FirstOrDefault(b => b.Code == "SONY");
        var nestleBrand = brands.FirstOrDefault(b => b.Code == "NEST");
        var nikeBrand = brands.FirstOrDefault(b => b.Code == "NIKE");
        var pentelBrand = brands.FirstOrDefault(b => b.Code == "PENS");

        var products = new List<Product>();

        if (electronicsCategory != null && samsungBrand != null)
        {
            var tv = new Product(tenantId, "Samsung 50\" LED TV", "SAMS-TV50", "PCS", 25000, 32000, 35000, electronicsCategory.Id)
            {
                Description = "50 inch Full HD LED Television",
                Barcode = "8801643668990",
                BrandId = samsungBrand.Id,
                MinStockLevel = 5,
                MaxStockLevel = 50,
                CurrentStock = 15,
                TaxRate = 18,
                HSNCode = "8528",
                CreatedBy = userId
            };
            products.Add(tv);

            var mobile = new Product(tenantId, "Samsung Galaxy S21", "SAMS-MOB", "PCS", 45000, 55000, 60000, electronicsCategory.Id)
            {
                Description = "Flagship smartphone with 5G",
                Barcode = "8801643770001",
                BrandId = samsungBrand.Id,
                MinStockLevel = 10,
                MaxStockLevel = 100,
                CurrentStock = 25,
                TaxRate = 18,
                HSNCode = "8517",
                CreatedBy = userId
            };
            products.Add(mobile);
        }

        if (electronicsCategory != null && sonyBrand != null)
        {
            var headphones = new Product(tenantId, "Sony WH-1000XM4 Headphones", "SONY-HP", "PCS", 18000, 24000, 26000, electronicsCategory.Id)
            {
                Description = "Noise cancelling wireless headphones",
                Barcode = "4548736112345",
                BrandId = sonyBrand.Id,
                MinStockLevel = 10,
                MaxStockLevel = 50,
                CurrentStock = 8,
                TaxRate = 18,
                HSNCode = "8518",
                CreatedBy = userId
            };
            products.Add(headphones);
        }

        if (foodCategory != null && nestleBrand != null)
        {
            var milk = new Product(tenantId, "Nestle Milkmaid", "NEST-MILK", "PCS", 80, 105, 110, foodCategory.Id)
            {
                Description = "Sweetened condensed milk 400g",
                Barcode = "8901058003345",
                BrandId = nestleBrand.Id,
                MinStockLevel = 50,
                MaxStockLevel = 500,
                CurrentStock = 120,
                TaxRate = 12,
                HSNCode = "0402",
                CreatedBy = userId
            };
            products.Add(milk);

            var coffee = new Product(tenantId, "Nescafe Classic Coffee", "NEST-COFFEE", "PCS", 250, 320, 340, foodCategory.Id)
            {
                Description = "Instant coffee 100g jar",
                Barcode = "8901058002345",
                BrandId = nestleBrand.Id,
                MinStockLevel = 30,
                MaxStockLevel = 200,
                CurrentStock = 4,  // Low stock
                TaxRate = 12,
                HSNCode = "0901",
                CreatedBy = userId
            };
            products.Add(coffee);
        }

        if (clothingCategory != null && nikeBrand != null)
        {
            var shoes = new Product(tenantId, "Nike Air Max", "NIKE-SHOE", "PAIR", 3500, 5500, 6000, clothingCategory.Id)
            {
                Description = "Running shoes - Size 9",
                Barcode = "192499334567",
                BrandId = nikeBrand.Id,
                MinStockLevel = 10,
                MaxStockLevel = 100,
                CurrentStock = 18,
                TaxRate = 12,
                HSNCode = "6403",
                CreatedBy = userId
            };
            products.Add(shoes);

            var tshirt = new Product(tenantId, "Nike Dri-FIT T-Shirt", "NIKE-TSHIRT", "PCS", 800, 1200, 1400, clothingCategory.Id)
            {
                Description = "Sports t-shirt - Medium",
                Barcode = "192499445678",
                BrandId = nikeBrand.Id,
                MinStockLevel = 20,
                MaxStockLevel = 200,
                CurrentStock = 45,
                TaxRate = 12,
                HSNCode = "6109",
                CreatedBy = userId
            };
            products.Add(tshirt);
        }

        if (stationeryCategory != null && pentelBrand != null)
        {
            var pen = new Product(tenantId, "Pentel EnerGel Pen", "PEN-001", "PCS", 15, 25, 30, stationeryCategory.Id)
            {
                Description = "0.7mm gel pen - Blue",
                Barcode = "7290002997789",
                BrandId = pentelBrand.Id,
                MinStockLevel = 100,
                MaxStockLevel = 1000,
                CurrentStock = 250,
                TaxRate = 18,
                HSNCode = "9608",
                CreatedBy = userId
            };
            products.Add(pen);

            var pencil = new Product(tenantId, "Pentel Pencil Set", "PEN-002", "PACK", 40, 60, 70, stationeryCategory.Id)
            {
                Description = "HB pencil pack of 10",
                Barcode = "7290002998890",
                BrandId = pentelBrand.Id,
                MinStockLevel = 50,
                MaxStockLevel = 500,
                CurrentStock = 2,  // Low stock
                TaxRate = 18,
                HSNCode = "9609",
                CreatedBy = userId
            };
            products.Add(pencil);
        }

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {products.Count} products.");
    }

    private async Task SeedCustomersAsync(Guid tenantId, Guid userId)
    {
        if (await _context.Customers.IgnoreQueryFilters().AnyAsync(c => c.TenantId == tenantId))
        {
            _logger.LogInformation("Customers already exist. Skipping customer seeding.");
            return;
        }

        var customers = new List<Customer>
        {
            new Customer(tenantId, "CUST001", "ABC Electronics Pvt Ltd", "contact@abc.com", 100000, 30)
            {
                ContactPerson = "Rajesh Kumar",
                Phone = "022-12345678",
                Mobile = "9876543210",
                Address = "123 MG Road",
                City = "Mumbai",
                State = "Maharashtra",
                Country = "India",
                Pincode = "400001",
                GSTNumber = "27AABCU9603R1ZM",
                PANNumber = "AABCU9603R",
                CreatedBy = userId
            },
            new Customer(tenantId, "CUST002", "XYZ Retail Store", "info@xyzretail.com", 50000, 15)
            {
                ContactPerson = "Priya Sharma",
                Phone = "011-98765432",
                Mobile = "9123456789",
                Address = "456 Connaught Place",
                City = "Delhi",
                State = "Delhi",
                Country = "India",
                Pincode = "110001",
                GSTNumber = "07AACCX1234Q1ZX",
                PANNumber = "AACCX1234Q",
                CreatedBy = userId
            },
            new Customer(tenantId, "CUST003", "PQR Wholesalers", "sales@pqr.com", 200000, 45)
            {
                ContactPerson = "Amit Patel",
                Phone = "079-23456789",
                Mobile = "9998887776",
                Address = "789 CG Road",
                City = "Ahmedabad",
                State = "Gujarat",
                Country = "India",
                Pincode = "380009",
                GSTNumber = "24AADCP1234R1ZP",
                PANNumber = "AADCP1234R",
                CreatedBy = userId
            },
            new Customer(tenantId, "CUST004", "LMN Supermarket", "contact@lmn.com", 75000, 20)
            {
                ContactPerson = "Sunita Reddy",
                Phone = "040-12349876",
                Mobile = "9876512345",
                Address = "321 Banjara Hills",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                Pincode = "500034",
                GSTNumber = "36AABCL5678M1Z5",
                PANNumber = "AABCL5678M",
                CreatedBy = userId
            },
            new Customer(tenantId, "CUST005", "RST Trading Co", "info@rst.com", 150000, 60)
            {
                ContactPerson = "Vikram Singh",
                Phone = "044-87654321",
                Mobile = "9444333222",
                Address = "567 Anna Salai",
                City = "Chennai",
                State = "Tamil Nadu",
                Country = "India",
                Pincode = "600002",
                GSTNumber = "33AAECR7890P1ZR",
                PANNumber = "AAECR7890P",
                CreatedBy = userId
            }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {customers.Count} customers.");
    }

    private async Task SeedSalesOrdersAsync(Guid tenantId, Guid userId)
    {
        if (await _context.SalesOrders.IgnoreQueryFilters().AnyAsync(so => so.TenantId == tenantId))
        {
            _logger.LogInformation("Sales orders already exist. Skipping sales order seeding.");
            return;
        }

        var customers = await _context.Customers.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).Take(3).ToListAsync();
        var products = await _context.Products.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).Take(5).ToListAsync();

        if (!customers.Any() || !products.Any())
        {
            _logger.LogWarning("No customers or products found. Skipping sales order seeding.");
            return;
        }

        var orders = new List<SalesOrder>();

        // Order 1 - Draft
        if (customers.Count > 0 && products.Count > 0)
        {
            var order1 = new SalesOrder(tenantId, "SO-2024-001", customers[0].Id, customers[0].CompanyName, DateTime.UtcNow.AddDays(-5))
            {
                Notes = "Test order - Draft state",
                ShippingAddress = customers[0].Address,
                BillingAddress = customers[0].Address,
                CreatedBy = userId
            };

            order1.AddItem(new SalesOrderItem(
                products[0].Id,
                products[0].Name,
                products[0].Code,
                2,
                products[0].Unit,
                products[0].SalePrice,
                products[0].TaxRate ?? 0));

            orders.Add(order1);
        }

        // Order 2 - Confirmed
        if (customers.Count > 1 && products.Count > 1)
        {
            var order2 = new SalesOrder(tenantId, "SO-2024-002", customers[1].Id, customers[1].CompanyName, DateTime.UtcNow.AddDays(-3))
            {
                Notes = "Test order - Confirmed state",
                ShippingAddress = customers[1].Address,
                BillingAddress = customers[1].Address,
                CreatedBy = userId
            };

            order2.AddItem(new SalesOrderItem(
                products[1].Id,
                products[1].Name,
                products[1].Code,
                5,
                products[1].Unit,
                products[1].SalePrice,
                products[1].TaxRate ?? 0));

            order2.AddItem(new SalesOrderItem(
                products[2].Id,
                products[2].Name,
                products[2].Code,
                3,
                products[2].Unit,
                products[2].SalePrice,
                products[2].TaxRate ?? 0));

            order2.Confirm();
            orders.Add(order2);
        }

        // Order 3 - Shipped
        if (customers.Count > 2 && products.Count > 2)
        {
            var order3 = new SalesOrder(tenantId, "SO-2024-003", customers[2].Id, customers[2].CompanyName, DateTime.UtcNow.AddDays(-7))
            {
                Notes = "Test order - Shipped state",
                ShippingAddress = customers[2].Address,
                BillingAddress = customers[2].Address,
                CreatedBy = userId
            };

            order3.AddItem(new SalesOrderItem(
                products[3].Id,
                products[3].Name,
                products[3].Code,
                10,
                products[3].Unit,
                products[3].SalePrice,
                products[3].TaxRate ?? 0));

            order3.Confirm();
            order3.Ship(DateTime.UtcNow.AddDays(-2));
            orders.Add(order3);
        }

        _context.SalesOrders.AddRange(orders);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {orders.Count} sales orders.");
    }

    private async Task SeedInvoicesAsync(Guid tenantId, Guid userId)
    {
        if (await _context.Invoices.IgnoreQueryFilters().AnyAsync(i => i.TenantId == tenantId))
        {
            _logger.LogInformation("Invoices already exist. Skipping invoice seeding.");
            return;
        }

        // Get confirmed and shipped sales orders to generate invoices from
        var confirmedOrders = await _context.SalesOrders
            .IgnoreQueryFilters()
            .Include(so => so.Items)
            .Include(so => so.Customer)
            .Where(so => so.TenantId == tenantId &&
                   (so.Status == SalesOrderStatus.Confirmed || so.Status == SalesOrderStatus.Shipped))
            .ToListAsync();

        if (!confirmedOrders.Any())
        {
            _logger.LogWarning("No confirmed sales orders found. Skipping invoice seeding.");
            return;
        }

        var invoices = new List<Invoice>();

        // Generate invoices from sales orders
        foreach (var order in confirmedOrders)
        {
            var customer = order.Customer ?? await _context.Customers.FindAsync(order.CustomerId);

            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{(invoices.Count + 1):D4}";

            var invoice = new Invoice(
                tenantId,
                invoiceNumber,
                DateTime.UtcNow.AddDays(-2),
                DateTime.UtcNow.AddDays(customer?.CreditDays ?? 30),
                order.CustomerId,
                order.CustomerName)
            {
                BillingAddress = order.BillingAddress,
                ShippingAddress = order.ShippingAddress,
                CustomerGSTNumber = customer?.GSTNumber,
                SalesOrderId = order.Id,
                SalesOrderNumber = order.OrderNumber,
                Notes = $"Invoice for {order.OrderNumber}",
                CreatedBy = userId
            };

            // Copy items from sales order
            foreach (var orderItem in order.Items)
            {
                var invoiceItem = new InvoiceItem(
                    orderItem.ProductId,
                    orderItem.ProductName,
                    orderItem.ProductCode,
                    orderItem.Quantity,
                    orderItem.Unit,
                    orderItem.UnitPrice,
                    orderItem.TaxRate)
                {
                    Description = orderItem.Notes
                };

                invoice.AddItem(invoiceItem);
            }

            // Apply discount if order had discount
            if (order.DiscountAmount > 0)
            {
                invoice.ApplyDiscount(order.DiscountAmount);
            }

            invoices.Add(invoice);
        }

        _context.Invoices.AddRange(invoices);
        await _context.SaveChangesAsync();

        // Detach all tracked invoices
        foreach (var invoice in invoices)
        {
            _context.Entry(invoice).State = EntityState.Detached;
        }

        // Reload invoices from database with fresh tracking
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var reloadedInvoices = await _context.Invoices
            .IgnoreQueryFilters()
            .Where(i => invoiceIds.Contains(i.Id))
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        // Record payments on some invoices to demonstrate different payment statuses
        // Wrapped in try-catch to handle concurrency issues during seeding
        try
        {
            if (reloadedInvoices.Count > 0)
            {
                // Invoice 1 - Full payment
                reloadedInvoices[0].RecordPayment(
                    reloadedInvoices[0].TotalAmount,
                    "Bank Transfer",
                    "TXN123456");

                _logger.LogInformation($"Invoice {reloadedInvoices[0].InvoiceNumber} - Fully paid");

                // Save immediately
                await _context.SaveChangesAsync();
            }

            if (reloadedInvoices.Count > 1)
            {
                // Reload invoice 2 to get fresh RowVersion
                var invoice2 = await _context.Invoices
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(i => i.Id == reloadedInvoices[1].Id);

                if (invoice2 != null)
                {
                    // Invoice 2 - Partial payment (50%)
                    var partialAmount = invoice2.TotalAmount * 0.5m;
                    invoice2.RecordPayment(
                        partialAmount,
                        "Cash",
                        null);

                    _logger.LogInformation($"Invoice {invoice2.InvoiceNumber} - Partially paid");

                    // Save immediately
                    await _context.SaveChangesAsync();
                }
            }

            // Invoice 3 (if exists) - Leave unpaid for overdue testing

            _logger.LogInformation($"Seeded {invoices.Count} invoices with payment records.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency exception while seeding invoice payments. Invoices created successfully, but payment demo data skipped.");
            _logger.LogInformation($"Seeded {invoices.Count} invoices (payment records skipped due to concurrency).");
        }
    }
}

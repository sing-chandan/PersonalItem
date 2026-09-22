using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<InvoiceResponse>>> GetAllAsync(Guid tenantId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.TenantId == tenantId && i.IsActive)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var response = invoices.Select(MapToResponse).ToList();
        return Result<List<InvoiceResponse>>.Ok(response);
    }

    public async Task<Result<InvoiceResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && i.IsActive);

        if (invoice == null)
            return Result<InvoiceResponse>.Fail("Invoice not found");

        return Result<InvoiceResponse>.Ok(MapToResponse(invoice));
    }

    public async Task<Result<InvoiceResponse>> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && i.TenantId == tenantId && i.IsActive);

        if (invoice == null)
            return Result<InvoiceResponse>.Fail("Invoice not found");

        return Result<InvoiceResponse>.Ok(MapToResponse(invoice));
    }

    public async Task<Result<List<InvoiceResponse>>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.CustomerId == customerId && i.TenantId == tenantId && i.IsActive)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var response = invoices.Select(MapToResponse).ToList();
        return Result<List<InvoiceResponse>>.Ok(response);
    }

    public async Task<Result<List<InvoiceResponse>>> GetOverdueInvoicesAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.Date;
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.TenantId == tenantId
                && i.IsActive
                && i.DueDate < today
                && i.PaymentStatus != InvoicePaymentStatus.Paid
                && i.PaymentStatus != InvoicePaymentStatus.Cancelled)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        var response = invoices.Select(MapToResponse).ToList();
        return Result<List<InvoiceResponse>>.Ok(response);
    }

    public async Task<Result<List<InvoiceResponse>>> GetUnpaidInvoicesAsync(Guid tenantId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.TenantId == tenantId
                && i.IsActive
                && (i.PaymentStatus == InvoicePaymentStatus.Unpaid || i.PaymentStatus == InvoicePaymentStatus.PartiallyPaid))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var response = invoices.Select(MapToResponse).ToList();
        return Result<List<InvoiceResponse>>.Ok(response);
    }

    public async Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request, Guid tenantId, Guid userId)
    {
        // Check for duplicate invoice number
        var exists = await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == request.InvoiceNumber && i.TenantId == tenantId);

        if (exists)
            return Result<InvoiceResponse>.Fail("Invoice number already exists");

        // Verify customer exists
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId && c.TenantId == tenantId && c.IsActive);

        if (!customerExists)
            return Result<InvoiceResponse>.Fail("Customer not found");

        try
        {
            var invoice = new Invoice(
                tenantId,
                request.InvoiceNumber,
                request.InvoiceDate,
                request.DueDate,
                request.CustomerId,
                request.CustomerName)
            {
                BillingAddress = request.BillingAddress,
                ShippingAddress = request.ShippingAddress,
                CustomerGSTNumber = request.CustomerGSTNumber,
                SalesOrderId = request.SalesOrderId,
                SalesOrderNumber = request.SalesOrderNumber,
                Notes = request.Notes,
                TermsAndConditions = request.TermsAndConditions,
                CreatedBy = userId
            };

            // Add items
            foreach (var itemRequest in request.Items)
            {
                var item = new InvoiceItem(
                    itemRequest.ProductId,
                    itemRequest.ProductName,
                    itemRequest.ProductCode,
                    itemRequest.Quantity,
                    itemRequest.Unit,
                    itemRequest.UnitPrice,
                    itemRequest.TaxRate)
                {
                    Description = itemRequest.Description,
                    HSNCode = itemRequest.HSNCode
                };

                invoice.AddItem(item);
            }

            // Apply discount and shipping
            if (request.DiscountAmount > 0)
                invoice.ApplyDiscount(request.DiscountAmount);

            if (request.ShippingCharges > 0)
                invoice.AddShippingCharges(request.ShippingCharges);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Result<InvoiceResponse>.Ok(MapToResponse(invoice), "Invoice created successfully");
        }
        catch (Exception ex)
        {
            return Result<InvoiceResponse>.Fail($"Error creating invoice: {ex.Message}");
        }
    }

    public async Task<Result<InvoiceResponse>> CreateFromSalesOrderAsync(Guid salesOrderId, Guid tenantId, Guid userId)
    {
        // Get sales order with items
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<InvoiceResponse>.Fail("Sales order not found");

        if (salesOrder.Status == SalesOrderStatus.Draft)
            return Result<InvoiceResponse>.Fail("Cannot create invoice from draft sales order. Please confirm the order first.");

        if (salesOrder.Status == SalesOrderStatus.Cancelled)
            return Result<InvoiceResponse>.Fail("Cannot create invoice from cancelled sales order");

        // Check if invoice already exists for this order
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.SalesOrderId == salesOrderId && i.TenantId == tenantId && i.IsActive);

        if (existingInvoice != null)
            return Result<InvoiceResponse>.Fail($"Invoice {existingInvoice.InvoiceNumber} already exists for this sales order");

        try
        {
            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync(tenantId);

            // Get customer details
            var customer = await _context.Customers.FindAsync(salesOrder.CustomerId);

            var invoice = new Invoice(
                tenantId,
                invoiceNumber,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(customer?.CreditDays ?? 30),
                salesOrder.CustomerId,
                salesOrder.CustomerName)
            {
                BillingAddress = salesOrder.BillingAddress,
                ShippingAddress = salesOrder.ShippingAddress,
                CustomerGSTNumber = customer?.GSTNumber,
                SalesOrderId = salesOrder.Id,
                SalesOrderNumber = salesOrder.OrderNumber,
                Notes = salesOrder.Notes,
                CreatedBy = userId
            };

            // Copy items from sales order
            foreach (var orderItem in salesOrder.Items)
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

            // Apply discount from sales order
            if (salesOrder.DiscountAmount > 0)
                invoice.ApplyDiscount(salesOrder.DiscountAmount);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Result<InvoiceResponse>.Ok(MapToResponse(invoice), "Invoice generated from sales order successfully");
        }
        catch (Exception ex)
        {
            return Result<InvoiceResponse>.Fail($"Error generating invoice: {ex.Message}");
        }
    }

    public async Task<Result<InvoiceResponse>> RecordPaymentAsync(Guid id, RecordPaymentRequest request, Guid tenantId, Guid userId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && i.IsActive);

        if (invoice == null)
            return Result<InvoiceResponse>.Fail("Invoice not found");

        if (invoice.PaymentStatus == InvoicePaymentStatus.Paid)
            return Result<InvoiceResponse>.Fail("Invoice is already fully paid");

        if (invoice.PaymentStatus == InvoicePaymentStatus.Cancelled)
            return Result<InvoiceResponse>.Fail("Cannot record payment for cancelled invoice");

        try
        {
            var paymentDate = request.PaymentDate ?? DateTime.UtcNow;
            invoice.RecordPayment(request.Amount, request.PaymentMethod, request.Reference);

            // Add notes to the payment if provided
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                var payment = invoice.Payments.Last();
                payment.Notes = request.Notes;
            }

            invoice.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<InvoiceResponse>.Ok(MapToResponse(invoice), "Payment recorded successfully");
        }
        catch (Exception ex)
        {
            return Result<InvoiceResponse>.Fail($"Error recording payment: {ex.Message}");
        }
    }

    public async Task<Result<InvoiceResponse>> VoidInvoiceAsync(Guid id, Guid tenantId, Guid userId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && i.IsActive);

        if (invoice == null)
            return Result<InvoiceResponse>.Fail("Invoice not found");

        try
        {
            invoice.MarkAsVoid();
            invoice.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<InvoiceResponse>.Ok(MapToResponse(invoice), "Invoice voided successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvoiceResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, Guid tenantId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && i.IsActive);

        if (invoice == null)
            return Result.Fail("Invoice not found");

        if (invoice.PaymentStatus == InvoicePaymentStatus.Paid || invoice.PaymentStatus == InvoicePaymentStatus.PartiallyPaid)
            return Result.Fail("Cannot delete invoice with payments. Please void it instead.");

        invoice.IsActive = false;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Ok("Invoice deleted successfully");
    }

    private async Task<string> GenerateInvoiceNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMM}-";

        var lastInvoice = await _context.Invoices
            .Where(i => i.TenantId == tenantId && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastInvoice.InvoiceNumber.Replace(prefix, "");
        if (int.TryParse(lastNumber, out var number))
        {
            return $"{prefix}{(number + 1):D4}";
        }

        return $"{prefix}0001";
    }

    private static InvoiceResponse MapToResponse(Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            TenantId = invoice.TenantId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.CustomerName,
            BillingAddress = invoice.BillingAddress,
            ShippingAddress = invoice.ShippingAddress,
            CustomerGSTNumber = invoice.CustomerGSTNumber,
            SalesOrderId = invoice.SalesOrderId,
            SalesOrderNumber = invoice.SalesOrderNumber,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            ShippingCharges = invoice.ShippingCharges,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            PaymentStatus = invoice.PaymentStatus,
            PaymentDate = invoice.PaymentDate,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes,
            TermsAndConditions = invoice.TermsAndConditions,
            IsActive = invoice.IsActive,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items.Select(item => new InvoiceItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductCode = item.ProductCode,
                Description = item.Description,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                HSNCode = item.HSNCode
            }).ToList(),
            Payments = invoice.Payments.Select(payment => new InvoicePaymentResponse
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Reference = payment.Reference,
                Notes = payment.Notes,
                CreatedAt = payment.CreatedAt
            }).ToList()
        };
    }
}

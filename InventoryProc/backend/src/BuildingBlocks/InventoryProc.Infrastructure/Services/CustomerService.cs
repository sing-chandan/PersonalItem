using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CustomerResponse>>> GetAllAsync(Guid tenantId)
    {
        var customers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        var response = customers.Select(MapToResponse).ToList();
        return Result<List<CustomerResponse>>.Ok(response);
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.IsActive);

        if (customer == null)
            return Result<CustomerResponse>.Fail("Customer not found");

        return Result<CustomerResponse>.Ok(MapToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> GetByCodeAsync(string customerCode, Guid tenantId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerCode == customerCode && c.TenantId == tenantId && c.IsActive);

        if (customer == null)
            return Result<CustomerResponse>.Fail("Customer not found");

        return Result<CustomerResponse>.Ok(MapToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, Guid tenantId, Guid userId)
    {
        // Check for duplicate customer code
        var exists = await _context.Customers
            .AnyAsync(c => c.CustomerCode == request.CustomerCode && c.TenantId == tenantId);

        if (exists)
            return Result<CustomerResponse>.Fail("Customer code already exists");

        try
        {
            var customer = new Customer(
                tenantId,
                request.CustomerCode,
                request.CompanyName,
                request.Email ?? string.Empty,
                request.CreditLimit ?? 0,
                request.CreditDays ?? 0);

            customer.UpdateDetails(
                request.CompanyName,
                request.ContactPerson,
                request.Email ?? string.Empty,
                request.Phone,
                request.Mobile,
                request.Address,
                request.City,
                request.State,
                request.Country,
                request.Pincode,
                request.GSTNumber,
                request.PANNumber);

            customer.CreatedBy = userId;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Result<CustomerResponse>.Ok(MapToResponse(customer), "Customer created successfully");
        }
        catch (Exception ex)
        {
            return Result<CustomerResponse>.Fail($"Error creating customer: {ex.Message}");
        }
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, Guid tenantId, Guid userId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.IsActive);

        if (customer == null)
            return Result<CustomerResponse>.Fail("Customer not found");

        try
        {
            customer.UpdateDetails(
                request.CompanyName,
                request.ContactPerson,
                request.Email ?? customer.Email,
                request.Phone,
                request.Mobile,
                request.Address,
                request.City,
                request.State,
                request.Country,
                request.Pincode,
                request.GSTNumber,
                request.PANNumber);

            if (request.CreditLimit.HasValue || request.CreditDays.HasValue)
            {
                customer.UpdateCreditTerms(
                    request.CreditLimit ?? customer.CreditLimit,
                    request.CreditDays ?? customer.CreditDays);
            }

            customer.UpdatedBy = userId;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<CustomerResponse>.Ok(MapToResponse(customer), "Customer updated successfully");
        }
        catch (Exception ex)
        {
            return Result<CustomerResponse>.Fail($"Error updating customer: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, Guid tenantId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.IsActive);

        if (customer == null)
            return Result.Fail("Customer not found");

        // Check if customer has any sales orders
        var hasSalesOrders = await _context.SalesOrders
            .AnyAsync(so => so.CustomerId == id && so.TenantId == tenantId);

        if (hasSalesOrders)
            return Result.Fail("Cannot delete customer with existing sales orders");

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Ok("Customer deleted successfully");
    }

    public async Task<Result<List<CustomerResponse>>> SearchAsync(string searchTerm, Guid tenantId)
    {
        var customers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsActive &&
                (c.CompanyName.Contains(searchTerm) ||
                 c.CustomerCode.Contains(searchTerm) ||
                 c.ContactPerson.Contains(searchTerm) ||
                 (c.Email != null && c.Email.Contains(searchTerm))))
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        var response = customers.Select(MapToResponse).ToList();
        return Result<List<CustomerResponse>>.Ok(response);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            CustomerCode = customer.CustomerCode,
            CompanyName = customer.CompanyName,
            ContactPerson = customer.ContactPerson,
            Email = customer.Email,
            Phone = customer.Phone,
            Mobile = customer.Mobile,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Country = customer.Country,
            Pincode = customer.Pincode,
            GSTNumber = customer.GSTNumber,
            PANNumber = customer.PANNumber,
            CreditLimit = customer.CreditLimit,
            CreditDays = customer.CreditDays,
            OutstandingBalance = customer.OutstandingBalance,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}

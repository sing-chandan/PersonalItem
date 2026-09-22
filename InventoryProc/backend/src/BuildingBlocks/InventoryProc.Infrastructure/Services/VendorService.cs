using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.Modules.Purchases.Application.Services;
using InventoryProc.Modules.Purchases.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly ApplicationDbContext _context;

    public VendorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<VendorResponse>>> GetAllAsync(Guid tenantId)
    {
        var vendors = await _context.Vendors
            .Where(v => v.TenantId == tenantId && v.IsActive)
            .OrderBy(v => v.CompanyName)
            .ToListAsync();

        var response = vendors.Select(MapToResponse).ToList();
        return Result<List<VendorResponse>>.Ok(response);
    }

    public async Task<Result<VendorResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.IsActive);

        if (vendor == null)
            return Result<VendorResponse>.Fail("Vendor not found");

        return Result<VendorResponse>.Ok(MapToResponse(vendor));
    }

    public async Task<Result<VendorResponse>> GetByCodeAsync(string code, Guid tenantId)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Code == code && v.TenantId == tenantId && v.IsActive);

        if (vendor == null)
            return Result<VendorResponse>.Fail("Vendor not found");

        return Result<VendorResponse>.Ok(MapToResponse(vendor));
    }

    public async Task<Result<VendorResponse>> CreateAsync(CreateVendorRequest request, Guid tenantId, Guid userId)
    {
        // Check if vendor code already exists
        var existingVendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Code == request.Code && v.TenantId == tenantId);

        if (existingVendor != null)
            return Result<VendorResponse>.Fail("Vendor code already exists");

        var vendor = new Vendor(
            tenantId,
            request.Code,
            request.CompanyName,
            request.Email,
            request.CreditLimit,
            request.PaymentTermDays)
        {
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Mobile = request.Mobile,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            Pincode = request.Pincode,
            GSTNumber = request.GSTNumber,
            PANNumber = request.PANNumber,
            TaxIdentificationNumber = request.TaxIdentificationNumber,
            BankName = request.BankName,
            BankAccountNumber = request.BankAccountNumber,
            BankIFSCCode = request.BankIFSCCode,
            Notes = request.Notes,
            CreatedBy = userId
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return Result<VendorResponse>.Ok(MapToResponse(vendor), "Vendor created successfully");
    }

    public async Task<Result<VendorResponse>> UpdateAsync(Guid id, UpdateVendorRequest request, Guid tenantId, Guid userId)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.IsActive);

        if (vendor == null)
            return Result<VendorResponse>.Fail("Vendor not found");

        vendor.Update(
            request.CompanyName,
            request.Email,
            request.ContactPerson,
            request.Phone,
            request.Mobile);

        vendor.Address = request.Address;
        vendor.City = request.City;
        vendor.State = request.State;
        vendor.Country = request.Country;
        vendor.Pincode = request.Pincode;
        vendor.GSTNumber = request.GSTNumber;
        vendor.PANNumber = request.PANNumber;
        vendor.TaxIdentificationNumber = request.TaxIdentificationNumber;
        vendor.BankName = request.BankName;
        vendor.BankAccountNumber = request.BankAccountNumber;
        vendor.BankIFSCCode = request.BankIFSCCode;
        vendor.Notes = request.Notes;
        vendor.UpdatedBy = userId;

        if (request.CreditLimit.HasValue && request.PaymentTermDays.HasValue)
        {
            vendor.UpdatePaymentTerms(request.CreditLimit.Value, request.PaymentTermDays.Value);
        }

        await _context.SaveChangesAsync();

        return Result<VendorResponse>.Ok(MapToResponse(vendor), "Vendor updated successfully");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.IsActive);

        if (vendor == null)
            return Result<bool>.Fail("Vendor not found");

        vendor.Deactivate();
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true, "Vendor deleted successfully");
    }

    public async Task<Result<List<VendorResponse>>> SearchAsync(string searchTerm, Guid tenantId)
    {
        var query = _context.Vendors
            .Where(v => v.TenantId == tenantId && v.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(v =>
                v.CompanyName.ToLower().Contains(searchTerm) ||
                v.Code.ToLower().Contains(searchTerm) ||
                (v.Email != null && v.Email.ToLower().Contains(searchTerm)) ||
                (v.ContactPerson != null && v.ContactPerson.ToLower().Contains(searchTerm)));
        }

        var vendors = await query.OrderBy(v => v.CompanyName).ToListAsync();
        var response = vendors.Select(MapToResponse).ToList();

        return Result<List<VendorResponse>>.Ok(response);
    }

    private static VendorResponse MapToResponse(Vendor vendor)
    {
        return new VendorResponse
        {
            Id = vendor.Id,
            TenantId = vendor.TenantId,
            Code = vendor.Code,
            CompanyName = vendor.CompanyName,
            ContactPerson = vendor.ContactPerson,
            Email = vendor.Email,
            Phone = vendor.Phone,
            Mobile = vendor.Mobile,
            Address = vendor.Address,
            City = vendor.City,
            State = vendor.State,
            Country = vendor.Country,
            Pincode = vendor.Pincode,
            GSTNumber = vendor.GSTNumber,
            PANNumber = vendor.PANNumber,
            TaxIdentificationNumber = vendor.TaxIdentificationNumber,
            CreditLimit = vendor.CreditLimit,
            PaymentTermDays = vendor.PaymentTermDays,
            OutstandingBalance = vendor.OutstandingBalance,
            BankName = vendor.BankName,
            BankAccountNumber = vendor.BankAccountNumber,
            BankIFSCCode = vendor.BankIFSCCode,
            Notes = vendor.Notes,
            IsActive = vendor.IsActive,
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt
        };
    }
}

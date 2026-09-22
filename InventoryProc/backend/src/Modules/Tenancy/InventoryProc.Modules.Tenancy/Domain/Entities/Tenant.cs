using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Tenancy.Domain.Entities;

/// <summary>
/// Tenant entity representing a customer business
/// </summary>
public class Tenant : Entity, IAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? GSTNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    private Tenant() { } // EF Core

    public Tenant(string code, string name, string businessName, string email)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tenant code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name cannot be empty", nameof(businessName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Code = code;
        Name = name;
        BusinessName = businessName;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string businessName, string email, string? phone,
        string? address, string? city, string? state, string? pincode, string? gstNumber)
    {
        BusinessName = businessName;
        Email = email;
        Phone = phone;
        Address = address;
        City = city;
        State = state;
        Pincode = pincode;
        GSTNumber = gstNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

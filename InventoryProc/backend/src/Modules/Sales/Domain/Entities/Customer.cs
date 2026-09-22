using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Domain.Entities;

/// <summary>
/// Customer entity for sales management
/// </summary>
public class Customer : Entity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public decimal OutstandingBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    private Customer() { } // EF Core

    public Customer(
        Guid tenantId,
        string customerCode,
        string companyName,
        string email,
        decimal creditLimit = 0,
        int creditDays = 0)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            throw new ArgumentException("Customer code cannot be empty", nameof(customerCode));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        TenantId = tenantId;
        CustomerCode = customerCode;
        CompanyName = companyName;
        Email = email;
        CreditLimit = creditLimit;
        CreditDays = creditDays;
        OutstandingBalance = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string companyName,
        string? contactPerson,
        string email,
        string? phone,
        string? mobile,
        string? address,
        string? city,
        string? state,
        string? country,
        string? pincode,
        string? gstNumber,
        string? panNumber)
    {
        CompanyName = companyName;
        ContactPerson = contactPerson;
        Email = email;
        Phone = phone;
        Mobile = mobile;
        Address = address;
        City = city;
        State = state;
        Country = country;
        Pincode = pincode;
        GSTNumber = gstNumber;
        PANNumber = panNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCreditTerms(decimal creditLimit, int creditDays)
    {
        CreditLimit = creditLimit;
        CreditDays = creditDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOutstandingBalance(decimal amount)
    {
        OutstandingBalance += amount;
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

    public bool HasCreditAvailable(decimal orderAmount)
    {
        return (OutstandingBalance + orderAmount) <= CreditLimit;
    }
}

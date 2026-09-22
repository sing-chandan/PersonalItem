using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Domain.Entities;

public class Vendor : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public string Code { get; private set; }
    public string CompanyName { get; private set; }
    public string? ContactPerson { get; set; }
    public string Email { get; private set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public decimal CreditLimit { get; private set; }
    public int PaymentTermDays { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSCCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    private Vendor() { } // EF Core

    public Vendor(Guid tenantId, string code, string companyName, string email, decimal creditLimit, int paymentTermDays)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Vendor code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Code = code;
        CompanyName = companyName;
        Email = email;
        CreditLimit = creditLimit;
        PaymentTermDays = paymentTermDays;
        OutstandingBalance = 0;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string companyName, string email, string? contactPerson, string? phone, string? mobile)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        CompanyName = companyName;
        Email = email;
        ContactPerson = contactPerson;
        Phone = phone;
        Mobile = mobile;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePaymentTerms(decimal creditLimit, int paymentTermDays)
    {
        CreditLimit = creditLimit;
        PaymentTermDays = paymentTermDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOutstandingBalance(decimal amount)
    {
        OutstandingBalance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasCreditAvailable()
    {
        return OutstandingBalance < CreditLimit;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

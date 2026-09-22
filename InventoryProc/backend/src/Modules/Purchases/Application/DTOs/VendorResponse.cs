namespace InventoryProc.Modules.Purchases.Application.DTOs;

public class VendorResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
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
    public string? TaxIdentificationNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSCCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateVendorRequest
{
    public string Code { get; set; } = string.Empty;
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
    public string? TaxIdentificationNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSCCode { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVendorRequest
{
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
    public string? TaxIdentificationNumber { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermDays { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSCCode { get; set; }
    public string? Notes { get; set; }
}

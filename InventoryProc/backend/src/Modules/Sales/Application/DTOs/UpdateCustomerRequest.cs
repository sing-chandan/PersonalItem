namespace InventoryProc.Modules.Sales.Application.DTOs;

public class UpdateCustomerRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? CreditDays { get; set; }
}

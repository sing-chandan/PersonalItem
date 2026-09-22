using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Application.Services;

public interface IInvoiceService
{
    Task<Result<List<InvoiceResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<InvoiceResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<InvoiceResponse>> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId);
    Task<Result<List<InvoiceResponse>>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<Result<List<InvoiceResponse>>> GetOverdueInvoicesAsync(Guid tenantId);
    Task<Result<List<InvoiceResponse>>> GetUnpaidInvoicesAsync(Guid tenantId);
    Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request, Guid tenantId, Guid userId);
    Task<Result<InvoiceResponse>> CreateFromSalesOrderAsync(Guid salesOrderId, Guid tenantId, Guid userId);
    Task<Result<InvoiceResponse>> RecordPaymentAsync(Guid id, RecordPaymentRequest request, Guid tenantId, Guid userId);
    Task<Result<InvoiceResponse>> VoidInvoiceAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid tenantId);
}

using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Reports.Application.Services;

public interface ISalesReportService
{
    Task<Result<SalesReportResponse>> GetSalesReportAsync(SalesReportRequest request, Guid tenantId);
}

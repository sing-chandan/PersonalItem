using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.Modules.Reports.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISalesReportService _salesReportService;
    private readonly IPurchaseReportService _purchaseReportService;
    private readonly IInventoryReportService _inventoryReportService;
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentTenantService _currentTenantService;

    public ReportsController(
        ISalesReportService salesReportService,
        IPurchaseReportService purchaseReportService,
        IInventoryReportService inventoryReportService,
        IDashboardService dashboardService,
        ICurrentTenantService currentTenantService)
    {
        _salesReportService = salesReportService;
        _purchaseReportService = purchaseReportService;
        _inventoryReportService = inventoryReportService;
        _dashboardService = dashboardService;
        _currentTenantService = currentTenantService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await _dashboardService.GetDashboardSummaryAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("sales")]
    public async Task<IActionResult> GetSalesReport([FromBody] SalesReportRequest request)
    {
        var result = await _salesReportService.GetSalesReportAsync(request, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("purchases")]
    public async Task<IActionResult> GetPurchaseReport([FromBody] PurchaseReportRequest request)
    {
        var result = await _purchaseReportService.GetPurchaseReportAsync(request, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromBody] InventoryReportRequest request)
    {
        var result = await _inventoryReportService.GetInventoryReportAsync(request, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

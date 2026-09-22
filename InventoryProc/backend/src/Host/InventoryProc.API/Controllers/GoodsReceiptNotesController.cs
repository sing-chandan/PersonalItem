using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.Modules.Purchases.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GoodsReceiptNotesController : ControllerBase
{
    private readonly IGoodsReceiptNoteService _grnService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public GoodsReceiptNotesController(
        IGoodsReceiptNoteService grnService,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _grnService = grnService;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _grnService.GetAllAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _grnService.GetByIdAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("grn-number/{grnNumber}")]
    public async Task<IActionResult> GetByGRNNumber(string grnNumber)
    {
        var result = await _grnService.GetByGRNNumberAsync(grnNumber, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("purchase-order/{purchaseOrderId}")]
    public async Task<IActionResult> GetByPurchaseOrder(Guid purchaseOrderId)
    {
        var result = await _grnService.GetByPurchaseOrderAsync(purchaseOrderId, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptNoteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _grnService.CreateAsync(
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _grnService.CompleteAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _grnService.CancelAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _grnService.DeleteAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

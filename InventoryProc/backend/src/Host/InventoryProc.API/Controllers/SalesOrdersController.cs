using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public SalesOrdersController(
        ISalesOrderService salesOrderService,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _salesOrderService = salesOrderService;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _salesOrderService.GetAllAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _salesOrderService.GetByIdAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("order-number/{orderNumber}")]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber)
    {
        var result = await _salesOrderService.GetByOrderNumberAsync(orderNumber, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var result = await _salesOrderService.GetByCustomerAsync(customerId, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _salesOrderService.CreateAsync(
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _salesOrderService.UpdateAsync(
            id,
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _salesOrderService.DeleteAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _salesOrderService.ConfirmAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(Guid id, [FromBody] ShipOrderRequest? request = null)
    {
        var result = await _salesOrderService.ShipAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId,
            request?.ShippedDate);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, [FromBody] DeliverOrderRequest? request = null)
    {
        var result = await _salesOrderService.DeliverAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId,
            request?.DeliveredDate);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _salesOrderService.CancelAsync(
            id,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/discount")]
    public async Task<IActionResult> ApplyDiscount(Guid id, [FromBody] ApplyDiscountRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _salesOrderService.ApplyDiscountAsync(
            id,
            request.DiscountAmount,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

public class ShipOrderRequest
{
    public DateTime? ShippedDate { get; set; }
}

public class DeliverOrderRequest
{
    public DateTime? DeliveredDate { get; set; }
}

public class ApplyDiscountRequest
{
    public decimal DiscountAmount { get; set; }
}

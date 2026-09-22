using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public InvoicesController(
        IInvoiceService invoiceService,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _invoiceService = invoiceService;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _invoiceService.GetAllAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _invoiceService.GetByIdAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("number/{invoiceNumber}")]
    public async Task<IActionResult> GetByInvoiceNumber(string invoiceNumber)
    {
        var result = await _invoiceService.GetByInvoiceNumberAsync(invoiceNumber, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var result = await _invoiceService.GetByCustomerAsync(customerId, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueInvoices()
    {
        var result = await _invoiceService.GetOverdueInvoicesAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("unpaid")]
    public async Task<IActionResult> GetUnpaidInvoices()
    {
        var result = await _invoiceService.GetUnpaidInvoicesAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _invoiceService.CreateAsync(
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPost("from-order/{orderId}")]
    public async Task<IActionResult> CreateFromSalesOrder(Guid orderId)
    {
        var result = await _invoiceService.CreateFromSalesOrderAsync(
            orderId,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _invoiceService.RecordPaymentAsync(
            id,
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> VoidInvoice(Guid id)
    {
        var result = await _invoiceService.VoidInvoiceAsync(
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
        var result = await _invoiceService.DeleteAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public CustomersController(
        ICustomerService customerService,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _customerService = customerService;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _customerService.GetAllAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("code/{customerCode}")]
    public async Task<IActionResult> GetByCode(string customerCode)
    {
        var result = await _customerService.GetByCodeAsync(customerCode, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var result = await _customerService.SearchAsync(searchTerm, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _customerService.CreateAsync(
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _customerService.UpdateAsync(
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
        var result = await _customerService.DeleteAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

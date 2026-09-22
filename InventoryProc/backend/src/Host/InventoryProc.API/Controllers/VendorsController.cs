using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.Modules.Purchases.Application.Services;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public VendorsController(
        IVendorService vendorService,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _vendorService = vendorService;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _vendorService.GetAllAsync(_currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _vendorService.GetByIdAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _vendorService.GetByCodeAsync(code, _currentTenantService.TenantId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        var result = await _vendorService.SearchAsync(searchTerm, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _vendorService.CreateAsync(
            request,
            _currentTenantService.TenantId,
            _currentUserService.UserId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _vendorService.UpdateAsync(
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
        var result = await _vendorService.DeleteAsync(id, _currentTenantService.TenantId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

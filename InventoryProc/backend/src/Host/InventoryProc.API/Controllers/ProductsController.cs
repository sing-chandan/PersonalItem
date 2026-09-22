using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryProc.Modules.Products.Application.Services;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Infrastructure.Services;
using InventoryProc.SharedKernel.Authorization;

namespace InventoryProc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IExcelImportService _excelService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IExcelImportService excelService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _excelService = excelService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permissions.ProductsView)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.ProductsView)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Search products by name, code, or barcode
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = Permissions.ProductsView)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var result = await _productService.SearchProductsAsync(searchTerm);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    [HttpGet("low-stock")]
    [Authorize(Policy = Permissions.ProductsView)]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _productService.GetLowStockProductsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Permissions.ProductsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateProductAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.ProductsEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productService.UpdateProductAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.ProductsDelete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Adjust product stock
    /// </summary>
    [HttpPost("{id}/adjust-stock")]
    [Authorize(Policy = Permissions.ProductsEdit)]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] StockAdjustmentRequest request)
    {
        var result = await _productService.AdjustStockAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Download product import template
    /// </summary>
    [HttpGet("template")]
    [Authorize(Policy = Permissions.ProductsView)]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileBytes = await _excelService.GenerateProductTemplatAsync();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products_Template.xlsx");
    }

    /// <summary>
    /// Bulk import products from Excel
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = Permissions.ProductsCreate)]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid Excel file");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only Excel files (.xlsx) are supported");

        using var stream = file.OpenReadStream();
        var importResult = await _excelService.ImportProductsAsync(stream);

        if (!importResult.Success || importResult.Data == null)
            return BadRequest(importResult);

        // Create products in batch
        var successCount = 0;
        var errors = new List<string>();

        foreach (var productRequest in importResult.Data)
        {
            var result = await _productService.CreateProductAsync(productRequest);
            if (result.Success)
                successCount++;
            else
                errors.Add($"{productRequest.Code}: {result.Message}");
        }

        return Ok(new
        {
            success = true,
            message = $"Imported {successCount} of {importResult.Data.Count} products",
            successCount,
            errorCount = errors.Count,
            errors
        });
    }
}

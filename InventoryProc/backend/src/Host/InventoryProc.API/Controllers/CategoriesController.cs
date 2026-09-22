using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryProc.Modules.Products.Application.Services;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Infrastructure.Services;

namespace InventoryProc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IExcelImportService _excelService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        IExcelImportService excelService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _excelService = excelService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create new category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateCategoryAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Download category import template
    /// </summary>
    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileBytes = await _excelService.GenerateCategoryTemplateAsync();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Categories_Template.xlsx");
    }

    /// <summary>
    /// Bulk import categories from Excel
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportCategories(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid Excel file");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only Excel files (.xlsx) are supported");

        using var stream = file.OpenReadStream();
        var importResult = await _excelService.ImportCategoriesAsync(stream);

        if (!importResult.Success || importResult.Data == null)
            return BadRequest(importResult);

        // Create categories in batch
        var successCount = 0;
        var errors = new List<string>();

        foreach (var categoryRequest in importResult.Data)
        {
            var result = await _categoryService.CreateCategoryAsync(categoryRequest);
            if (result.Success)
                successCount++;
            else
                errors.Add($"{categoryRequest.Code}: {result.Message}");
        }

        return Ok(new
        {
            success = true,
            message = $"Imported {successCount} of {importResult.Data.Count} categories",
            successCount,
            errorCount = errors.Count,
            errors
        });
    }
}

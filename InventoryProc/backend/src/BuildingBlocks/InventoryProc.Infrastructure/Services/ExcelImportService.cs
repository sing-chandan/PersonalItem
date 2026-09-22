using OfficeOpenXml;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public interface IExcelImportService
{
    Task<byte[]> GenerateProductTemplatAsync();
    Task<byte[]> GenerateCategoryTemplateAsync();
    Task<Result<List<CreateProductRequest>>> ImportProductsAsync(Stream fileStream);
    Task<Result<List<CreateCategoryRequest>>> ImportCategoriesAsync(Stream fileStream);
}

public class ExcelImportService : IExcelImportService
{
    private readonly ApplicationDbContext _context;

    public ExcelImportService(ApplicationDbContext context)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _context = context;
    }

    public async Task<byte[]> GenerateProductTemplatAsync()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        // Get actual categories and brands from database
        var categories = await _context.Categories.Take(10).ToListAsync();
        var brands = await _context.Brands.Take(10).ToListAsync();

        // Headers
        worksheet.Cells[1, 1].Value = "Name*";
        worksheet.Cells[1, 2].Value = "Code*";
        worksheet.Cells[1, 3].Value = "Description";
        worksheet.Cells[1, 4].Value = "Barcode";
        worksheet.Cells[1, 5].Value = "CategoryId* (GUID)";
        worksheet.Cells[1, 6].Value = "BrandId (GUID)";
        worksheet.Cells[1, 7].Value = "Unit* (PCS/KG/LITER)";
        worksheet.Cells[1, 8].Value = "PurchasePrice*";
        worksheet.Cells[1, 9].Value = "SalePrice*";
        worksheet.Cells[1, 10].Value = "MRP*";
        worksheet.Cells[1, 11].Value = "MinStockLevel";
        worksheet.Cells[1, 12].Value = "MaxStockLevel";
        worksheet.Cells[1, 13].Value = "TaxType (GST/VAT)";
        worksheet.Cells[1, 14].Value = "TaxRate";
        worksheet.Cells[1, 15].Value = "HSNCode";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 15])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Sample data with actual GUIDs
        if (categories.Any() && brands.Any())
        {
            worksheet.Cells[2, 1].Value = "Sample Product 1";
            worksheet.Cells[2, 2].Value = "PROD001";
            worksheet.Cells[2, 3].Value = "Sample description";
            worksheet.Cells[2, 4].Value = "1234567890";
            worksheet.Cells[2, 5].Value = categories.First().Id.ToString();
            worksheet.Cells[2, 6].Value = brands.First().Id.ToString();
            worksheet.Cells[2, 7].Value = "PCS";
            worksheet.Cells[2, 8].Value = 100;
            worksheet.Cells[2, 9].Value = 150;
            worksheet.Cells[2, 10].Value = 180;
            worksheet.Cells[2, 11].Value = 10;
            worksheet.Cells[2, 12].Value = 100;
            worksheet.Cells[2, 13].Value = "GST";
            worksheet.Cells[2, 14].Value = 18;
            worksheet.Cells[2, 15].Value = "1234";
        }

        worksheet.Cells.AutoFitColumns();

        // Add Reference sheet for Categories and Brands
        var refSheet = package.Workbook.Worksheets.Add("Reference");

        // Categories section
        refSheet.Cells[1, 1].Value = "Available Categories";
        refSheet.Cells[1, 1].Style.Font.Bold = true;
        refSheet.Cells[1, 1].Style.Font.Size = 14;

        refSheet.Cells[2, 1].Value = "Category Name";
        refSheet.Cells[2, 2].Value = "Category Code";
        refSheet.Cells[2, 3].Value = "Category ID (use this in Products sheet)";

        using (var range = refSheet.Cells[2, 1, 2, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        int row = 3;
        foreach (var category in categories)
        {
            refSheet.Cells[row, 1].Value = category.Name;
            refSheet.Cells[row, 2].Value = category.Code;
            refSheet.Cells[row, 3].Value = category.Id.ToString();
            row++;
        }

        // Brands section
        row += 2;
        refSheet.Cells[row, 1].Value = "Available Brands";
        refSheet.Cells[row, 1].Style.Font.Bold = true;
        refSheet.Cells[row, 1].Style.Font.Size = 14;

        row++;
        refSheet.Cells[row, 1].Value = "Brand Name";
        refSheet.Cells[row, 2].Value = "Brand Code";
        refSheet.Cells[row, 3].Value = "Brand ID (use this in Products sheet)";

        using (var range = refSheet.Cells[row, 1, row, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        row++;
        foreach (var brand in brands)
        {
            refSheet.Cells[row, 1].Value = brand.Name;
            refSheet.Cells[row, 2].Value = brand.Code;
            refSheet.Cells[row, 3].Value = brand.Id.ToString();
            row++;
        }

        refSheet.Cells.AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> GenerateCategoryTemplateAsync()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Categories");

        // Headers
        worksheet.Cells[1, 1].Value = "Name*";
        worksheet.Cells[1, 2].Value = "Code*";
        worksheet.Cells[1, 3].Value = "Description";
        worksheet.Cells[1, 4].Value = "ParentCategoryId (GUID)";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 4])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        // Sample data
        worksheet.Cells[2, 1].Value = "Electronics";
        worksheet.Cells[2, 2].Value = "CAT001";
        worksheet.Cells[2, 3].Value = "Electronic items";
        worksheet.Cells[2, 4].Value = "Leave empty for top level";

        worksheet.Cells[3, 1].Value = "Mobiles";
        worksheet.Cells[3, 2].Value = "CAT002";
        worksheet.Cells[3, 3].Value = "Mobile phones";
        worksheet.Cells[3, 4].Value = "Replace with Electronics CategoryId for sub-category";

        worksheet.Cells.AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<Result<List<CreateProductRequest>>> ImportProductsAsync(Stream fileStream)
    {
        var products = new List<CreateProductRequest>();
        var errors = new List<string>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
                return Result<List<CreateProductRequest>>.Fail("No data found in Excel file");

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, 1].Text;
                    var code = worksheet.Cells[row, 2].Text;

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
                    {
                        errors.Add($"Row {row}: Name and Code are required");
                        continue;
                    }

                    var categoryIdText = worksheet.Cells[row, 5].Text;
                    if (!Guid.TryParse(categoryIdText, out var categoryId))
                    {
                        errors.Add($"Row {row}: Invalid CategoryId GUID");
                        continue;
                    }

                    var product = new CreateProductRequest
                    {
                        Name = name,
                        Code = code,
                        Description = worksheet.Cells[row, 3].Text,
                        Barcode = worksheet.Cells[row, 4].Text,
                        CategoryId = categoryId,
                        BrandId = Guid.TryParse(worksheet.Cells[row, 6].Text, out var brandId) ? brandId : null,
                        Unit = worksheet.Cells[row, 7].Text,
                        PurchasePrice = decimal.TryParse(worksheet.Cells[row, 8].Text, out var pp) ? pp : 0,
                        SalePrice = decimal.TryParse(worksheet.Cells[row, 9].Text, out var sp) ? sp : 0,
                        MRP = decimal.TryParse(worksheet.Cells[row, 10].Text, out var mrp) ? mrp : 0,
                        MinStockLevel = decimal.TryParse(worksheet.Cells[row, 11].Text, out var minStock) ? minStock : null,
                        MaxStockLevel = decimal.TryParse(worksheet.Cells[row, 12].Text, out var maxStock) ? maxStock : null,
                        TaxType = worksheet.Cells[row, 13].Text,
                        TaxRate = decimal.TryParse(worksheet.Cells[row, 14].Text, out var taxRate) ? taxRate : null,
                        HSNCode = worksheet.Cells[row, 15].Text
                    };

                    products.Add(product);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            if (products.Count == 0 && errors.Count > 0)
                return Result<List<CreateProductRequest>>.Fail(string.Join("; ", errors));

            var message = errors.Count > 0
                ? $"Imported {products.Count} products with {errors.Count} errors: {string.Join("; ", errors)}"
                : $"Successfully imported {products.Count} products";

            return Result<List<CreateProductRequest>>.Ok(products, message);
        }
        catch (Exception ex)
        {
            return Result<List<CreateProductRequest>>.Fail($"Error reading Excel file: {ex.Message}");
        }
    }

    public async Task<Result<List<CreateCategoryRequest>>> ImportCategoriesAsync(Stream fileStream)
    {
        var categories = new List<CreateCategoryRequest>();
        var errors = new List<string>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
                return Result<List<CreateCategoryRequest>>.Fail("No data found in Excel file");

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, 1].Text;
                    var code = worksheet.Cells[row, 2].Text;

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
                    {
                        errors.Add($"Row {row}: Name and Code are required");
                        continue;
                    }

                    var category = new CreateCategoryRequest
                    {
                        Name = name,
                        Code = code,
                        Description = worksheet.Cells[row, 3].Text,
                        ParentCategoryId = Guid.TryParse(worksheet.Cells[row, 4].Text, out var parentId) ? parentId : null
                    };

                    categories.Add(category);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            if (categories.Count == 0 && errors.Count > 0)
                return Result<List<CreateCategoryRequest>>.Fail(string.Join("; ", errors));

            var message = errors.Count > 0
                ? $"Imported {categories.Count} categories with {errors.Count} errors: {string.Join("; ", errors)}"
                : $"Successfully imported {categories.Count} categories";

            return Result<List<CreateCategoryRequest>>.Ok(categories, message);
        }
        catch (Exception ex)
        {
            return Result<List<CreateCategoryRequest>>.Fail($"Error reading Excel file: {ex.Message}");
        }
    }
}

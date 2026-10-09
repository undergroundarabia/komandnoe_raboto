using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Services;

public record ProductPage(
    IReadOnlyList<Product> Items,
    int PageNumber,
    int TotalPages,
    int TotalItems);

public class ProductCatalogService
{
    private readonly ShoeStoreContext _context;

    public ProductCatalogService(ShoeStoreContext context)
    {
        _context = context;
    }

    public Task<List<Product>> GetAllAsync()
    {
        return CreateCatalogQuery()
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public Task<List<Product>> SearchByNameAsync(string searchText)
    {
        return CreateCatalogQuery()
            .Where(product => product.Name.Contains(searchText))
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public Task<List<Product>> FilterByCategoryAsync(string categoryName)
    {
        return CreateCatalogQuery()
            .Where(product => product.Category.Name.Contains(categoryName))
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public Task<List<Product>> SortByPriceAsync(bool descending)
    {
        IQueryable<Product> query = CreateCatalogQuery();

        query = descending
            ? query.OrderByDescending(product => product.Price)
            : query.OrderBy(product => product.Price);

        return query.ToListAsync();
    }

    public async Task<ProductPage> GetPageAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        int totalItems = await _context.Products.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        List<Product> items = await CreateCatalogQuery()
            .OrderBy(product => product.ProductId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductPage(items, pageNumber, totalPages, totalItems);
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public Task<List<Manufacturer>> GetManufacturersAsync()
    {
        return _context.Manufacturers
            .AsNoTracking()
            .OrderBy(manufacturer => manufacturer.Name)
            .ToListAsync();
    }

    public async Task<int> AddProductAsync(
        string name,
        string? description,
        string? composition,
        decimal price,
        int categoryId,
        int manufacturerId)
    {
        ValidateProduct(name, price);
        await ValidateReferencesAsync(categoryId, manufacturerId);

        var product = new Product
        {
            Name = name.Trim(),
            Description = NormalizeOptionalText(description),
            Composition = NormalizeOptionalText(composition),
            Price = price,
            CategoryId = categoryId,
            ManufacturerId = manufacturerId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product.ProductId;
    }

    public async Task<bool> UpdateProductAsync(
        int productId,
        string name,
        string? description,
        string? composition,
        decimal price,
        int categoryId,
        int manufacturerId)
    {
        ValidateProduct(name, price);
        await ValidateReferencesAsync(categoryId, manufacturerId);

        Product? product = await _context.Products.FindAsync(productId);
        if (product is null)
            return false;

        product.Name = name.Trim();
        product.Description = NormalizeOptionalText(description);
        product.Composition = NormalizeOptionalText(composition);
        product.Price = price;
        product.CategoryId = categoryId;
        product.ManufacturerId = manufacturerId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        Product? product = await _context.Products.FindAsync(productId);
        if (product is null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Product> CreateCatalogQuery()
    {
        return _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Manufacturer)
            .Include(product => product.ProductSizes)
                .ThenInclude(productSize => productSize.Size);
    }

    private async Task ValidateReferencesAsync(int categoryId, int manufacturerId)
    {
        if (!await _context.Categories.AnyAsync(category => category.CategoryId == categoryId))
            throw new ArgumentException("категория с таким ID не найдена");

        if (!await _context.Manufacturers.AnyAsync(manufacturer => manufacturer.ManufacturerId == manufacturerId))
            throw new ArgumentException("производитель с таким ID не найден");
    }

    private static void ValidateProduct(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("название товара не может быть пустым");

        if (price <= 0)
            throw new ArgumentException("цена должна быть больше нуля");
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

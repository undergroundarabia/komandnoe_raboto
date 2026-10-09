using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Services;


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

    private IQueryable<Product> CreateCatalogQuery()
    {
        return _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Manufacturer)
            .Include(product => product.ProductSizes)
                .ThenInclude(productSize => productSize.Size);
    }
}

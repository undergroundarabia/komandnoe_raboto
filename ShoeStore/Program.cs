using System.Text;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.Services;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

await using var context = new ShoeStoreContext();
var catalogService = new ProductCatalogService(context);

try
{
    if (!await context.Database.CanConnectAsync())
    {
        Console.WriteLine("не получилось подключиться к базе");
        Console.WriteLine("выполните скрипт Database/ShoeStoreDb.sql");
        return;
    }

    await RunMenuAsync(catalogService);
}
catch (Exception exception)
{
    Console.WriteLine("ошибка при работе с бд:");
    Console.WriteLine(exception.Message);
}
static async Task RunMenuAsync(ProductCatalogService catalogService)
{
    while (true)
    {
        PrintMenu();
        string? command = Console.ReadLine()?.Trim();
        Console.WriteLine();

        switch (command)
        {
            case "1":
                PrintProducts(await catalogService.GetAllAsync());
                break;

            case "2":
                Console.Write("введите часть названия товара: ");
                string productName = Console.ReadLine()?.Trim() ?? string.Empty;
                PrintProducts(await catalogService.SearchByNameAsync(productName));
                break;

            case "3":
                Console.Write("введите название категории: ");
                string categoryName = Console.ReadLine()?.Trim() ?? string.Empty;
                PrintProducts(await catalogService.FilterByCategoryAsync(categoryName));
                break;

            case "4":
                PrintProducts(await catalogService.SortByPriceAsync(descending: false));
                break;

            case "5":
                PrintProducts(await catalogService.SortByPriceAsync(descending: true));
                break;

            case "0":
                Console.WriteLine("всё пока");
                return;

            default:
                Console.WriteLine("неправильно попробуй еще раз");
                break;
        }

        Console.WriteLine();
    }
}

static void PrintMenu()
{
    Console.WriteLine("магазин обуви оля поля");
    Console.WriteLine("1. показать все товары");
    Console.WriteLine("2. найти товар по названию");
    Console.WriteLine("3. отфильтровать по категории");
    Console.WriteLine("4. отсортировать по цене хочу дешёво");
    Console.WriteLine("5. отсортировать по цене хочу дорого");
    Console.WriteLine("0. выход");
    Console.Write("выберите действие: ");
}

static void PrintProducts(IReadOnlyCollection<Product> products)
{
    if (products.Count == 0)
    {
        Console.WriteLine("нет товаров");
        return;
    }

    foreach (Product product in products)
    {
        string sizes = string.Join(", ", product.ProductSizes
            .Where(productSize => productSize.Quantity > 0)
            .OrderBy(productSize => productSize.Size.Value)
            .Select(productSize => $"{productSize.Size.Value:g} ({productSize.Quantity} шт)"));

        if (string.IsNullOrEmpty(sizes))
            sizes = "нет в наличии";

        Console.WriteLine($"[{product.ProductId}] {product.Name}");
        Console.WriteLine($"категория: {product.Category.Name}");
        Console.WriteLine($"производитель: {product.Manufacturer.Name}");
        Console.WriteLine($"цена: {product.Price:N2} руб");
        Console.WriteLine($"размеры: {sizes}");
        Console.WriteLine($"всего в наличии: {product.TotalQuantity} шт");
    }
}

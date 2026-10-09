using System.Globalization;
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

        try
        {
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
                case "6":
                    await ShowPagesAsync(catalogService);
                    break;
                case "7":
                    await AddProductAsync(catalogService);
                    break;
                case "8":
                    await UpdateProductAsync(catalogService);
                    break;
                case "9":
                    await DeleteProductAsync(catalogService);
                    break;
                case "0":
                    Console.WriteLine("всё пока");
                    return;
                default:
                    Console.WriteLine("неправильно, попробуйте ещё раз");
                    break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"операция не выполнена: {exception.Message}");
        }

        Console.WriteLine();
    }
}

static async Task ShowPagesAsync(ProductCatalogService catalogService)
{
    const int pageSize = 2;
    int pageNumber = 1;

    while (true)
    {
        ProductPage page = await catalogService.GetPageAsync(pageNumber, pageSize);

        if (page.TotalItems == 0)
        {
            Console.WriteLine("в каталоге пока нет товаров");
            return;
        }

        Console.WriteLine($"страница {page.PageNumber} из {page.TotalPages}");
        PrintProducts(page.Items);
        Console.WriteLine("N — следующая, P — предыдущая, 0 — вернуться в меню");
        Console.Write("выберите действие: ");

        string? command = Console.ReadLine()?.Trim().ToUpperInvariant();
        if (command == "0")
            return;

        if (command == "N" && pageNumber < page.TotalPages)
            pageNumber++;
        else if (command == "P" && pageNumber > 1)
            pageNumber--;
        else
            Console.WriteLine("дальше переходить нельзя");

        Console.WriteLine();
    }
}

static async Task AddProductAsync(ProductCatalogService catalogService)
{
    await PrintReferenceDataAsync(catalogService);
    ProductInput input = ReadProductInput();

    int productId = await catalogService.AddProductAsync(
        input.Name, input.Description, input.Composition, input.Price,
        input.CategoryId, input.ManufacturerId);

    Console.WriteLine($"товар успешно добавлен, ID = {productId}");
}

static async Task UpdateProductAsync(ProductCatalogService catalogService)
{
    int productId = ReadPositiveInt("введите ID товара для изменения: ");
    await PrintReferenceDataAsync(catalogService);
    ProductInput input = ReadProductInput();

    bool updated = await catalogService.UpdateProductAsync(
        productId, input.Name, input.Description, input.Composition, input.Price,
        input.CategoryId, input.ManufacturerId);

    Console.WriteLine(updated ? "товар успешно изменён" : "товара с таким айди нет");
}

static async Task DeleteProductAsync(ProductCatalogService catalogService)
{
    int productId = ReadPositiveInt("введите айди товара для удаления: ");
    Console.Write("точно удалить товар? (да или нет): ");

    if (!string.Equals(Console.ReadLine()?.Trim(), "да", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("удаление отменено");
        return;
    }

    bool deleted = await catalogService.DeleteProductAsync(productId);
    Console.WriteLine(deleted ? "товар успешно удалён" : "товар с таким ID не найден");
}

static async Task PrintReferenceDataAsync(ProductCatalogService catalogService)
{
    List<Category> categories = await catalogService.GetCategoriesAsync();
    List<Manufacturer> manufacturers = await catalogService.GetManufacturersAsync();

    Console.WriteLine("категории:");
    foreach (Category category in categories)
        Console.WriteLine($"  {category.CategoryId} — {category.Name}");

    Console.WriteLine("производители:");
    foreach (Manufacturer manufacturer in manufacturers)
        Console.WriteLine($"  {manufacturer.ManufacturerId} — {manufacturer.Name}");
}

static ProductInput ReadProductInput()
{
    string name = ReadRequiredText("название: ");
    Console.Write("описание (можно оставить пустым): ");
    string? description = Console.ReadLine();
    Console.Write("состав (можно оставить пустым): ");
    string? composition = Console.ReadLine();
    decimal price = ReadPositiveDecimal("цена: ");
    int categoryId = ReadPositiveInt("айди категории: ");
    int manufacturerId = ReadPositiveInt("айди производителя: ");

    return new ProductInput(name, description, composition, price, categoryId, manufacturerId);
}

static string ReadRequiredText(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string value = Console.ReadLine()?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(value))
            return value;
        Console.WriteLine("значение не может быть пустым");
    }
}

static int ReadPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
            return value;
        Console.WriteLine("введите целое число больше нуля");
    }
}

static decimal ReadPositiveDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string value = Console.ReadLine()?.Trim().Replace(',', '.') ?? string.Empty;
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)
            && price > 0)
            return price;
        Console.WriteLine("введите цену больше нуля");
    }
}

static void PrintMenu()
{
    Console.WriteLine("магазин обуви оля поля");
    Console.WriteLine("1. показать все товары");
    Console.WriteLine("2. найти товар по названию");
    Console.WriteLine("3. отфильтровать по категории");
    Console.WriteLine("4. отсортировать по цене сначала дешёво");
    Console.WriteLine("5. отсортировать по цене сначала дорого");
    Console.WriteLine("6. показать каталог по страницам");
    Console.WriteLine("7. добавить товар");
    Console.WriteLine("8. изменить товар");
    Console.WriteLine("9. удалить товар");
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

internal record ProductInput(
    string Name,
    string? Description,
    string? Composition,
    decimal Price,
    int CategoryId,
    int ManufacturerId);

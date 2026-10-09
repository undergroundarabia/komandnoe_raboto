using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLog;
using ShoeStore.Exceptions;

namespace ShoeStore.Infrastructure;

/// <summary>
/// Централизованная обработка исключений: запись в лог и понятное сообщение пользователю.
/// </summary>
public static class ExceptionHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>Подписка на необработанные исключения. Вызывается при старте приложения.</summary>
    public static void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Logger.Fatal(e.ExceptionObject as Exception, "Необработанное исключение, приложение будет закрыто");
            ConsoleMessage.Error("Произошла критическая ошибка. Подробности записаны в папку logs.");
            LogManager.Shutdown();
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Logger.Error(e.Exception, "Необработанное исключение в фоновой задаче");
            e.SetObserved();
        };
    }

    /// <summary>Записывает исключение в лог и выводит пользователю понятный текст.</summary>
    public static void Handle(Exception ex, string actionName)
    {
        switch (ex)
        {
            // Ожидаемые ошибки ввода — уровень Warning, текст для пользователя уже готов
            case InvalidInputException or EntityNotFoundException:
                Logger.Warn("{Action}: {Message}", actionName, ex.Message);
                ConsoleMessage.Warning(ex.Message);
                break;

            case ShoeStoreException:
                Logger.Error(ex, "{Action}: ошибка приложения", actionName);
                ConsoleMessage.Error(ex.Message);
                break;

            case DbUpdateException dbEx:
                Logger.Error(dbEx, "{Action}: ошибка сохранения в БД", actionName);
                ConsoleMessage.Error(DescribeDbUpdateError(dbEx));
                break;

            case SqlException sqlEx when IsConnectionError(sqlEx):
                Logger.Error(sqlEx, "{Action}: нет подключения к БД", actionName);
                ConsoleMessage.Error("Не удалось подключиться к базе данных ShoeStoreDb. " +
                                     "Проверьте, что SQL Server LocalDB запущен и база создана скриптом.");
                break;

            case ArgumentException:
                Logger.Warn("{Action}: неверный аргумент. {Message}", actionName, ex.Message);
                ConsoleMessage.Warning(ex.Message);
                break;

            default:
                Logger.Error(ex, "{Action}: непредвиденная ошибка", actionName);
                ConsoleMessage.Error("Непредвиденная ошибка. Операция не выполнена, подробности записаны в лог.");
                break;
        }
    }

    // Коды ошибок SQL Server: 547 — нарушение FK/CHECK, 2601/2627 — дубликат уникального значения
    private static string DescribeDbUpdateError(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sql)
        {
            return sql.Number switch
            {
                547 => "Операция нарушает связи между данными: например, товар уже есть в заказах " +
                       "и не может быть удалён.",
                2601 or 2627 => "Такая запись уже существует. Измените значение и повторите.",
                _ => "Не удалось сохранить изменения в базе данных."
            };
        }

        return "Не удалось сохранить изменения в базе данных.";
    }

    // -1, 2, 53 — сервер недоступен; 4060 — база не найдена
    private static bool IsConnectionError(SqlException ex) =>
        ex.Number is -1 or 2 or 53 or 4060;
}

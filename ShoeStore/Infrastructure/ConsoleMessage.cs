namespace ShoeStore.Infrastructure;

/// <summary>Сообщения пользователю с типом и цветом: ошибка, предупреждение, информация.</summary>
public static class ConsoleMessage
{
    public static void Error(string text) => Write("ОШИБКА", text, ConsoleColor.Red);

    public static void Warning(string text) => Write("ПРЕДУПРЕЖДЕНИЕ", text, ConsoleColor.Yellow);

    public static void Info(string text) => Write("ИНФОРМАЦИЯ", text, ConsoleColor.Cyan);

    /// <summary>Подтверждение необратимой операции (например, удаления).</summary>
    public static bool Confirm(string question)
    {
        Write("ПОДТВЕРЖДЕНИЕ", $"{question} Действие нельзя отменить. (д/н)", ConsoleColor.Yellow);
        var answer = Console.ReadLine()?.Trim().ToLower();
        return answer is "д" or "да" or "y" or "yes";
    }

    private static void Write(string title, string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{title}] {text}");
        Console.ForegroundColor = previous;
    }
}

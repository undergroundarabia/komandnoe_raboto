namespace ShoeStore.Exceptions;

/// <summary>
/// Базовое исключение приложения. Message — текст для пользователя:
/// что произошло и что сделать, чтобы исправить.
/// </summary>
public class ShoeStoreException : Exception
{
    public ShoeStoreException(string message) : base(message) { }

    public ShoeStoreException(string message, Exception innerException)
        : base(message, innerException) { }
}

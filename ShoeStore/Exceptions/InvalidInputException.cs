namespace ShoeStore.Exceptions;

/// <summary>Пользователь ввёл некорректное значение.</summary>
public class InvalidInputException : ShoeStoreException
{
    public string FieldName { get; }

    public InvalidInputException(string fieldName, string message)
        : base($"Поле «{fieldName}»: {message}")
    {
        FieldName = fieldName;
    }
}

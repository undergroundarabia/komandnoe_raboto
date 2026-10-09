namespace ShoeStore.Exceptions;

/// <summary>Запись с указанным идентификатором не найдена в БД.</summary>
public class EntityNotFoundException : ShoeStoreException
{
    public string EntityName { get; }
    public int EntityId { get; }

    public EntityNotFoundException(string entityName, int entityId)
        : base($"{entityName} с номером {entityId} не найден(а). Проверьте номер и повторите ввод.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

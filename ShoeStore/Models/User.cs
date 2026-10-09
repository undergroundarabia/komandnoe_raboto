namespace ShoeStore.Models;

public class User
{
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>ФИО для вывода в правом верхнем углу окна.</summary>
    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
}

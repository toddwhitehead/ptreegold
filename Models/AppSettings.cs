namespace PTreeGold.Models;

public class AppSettings
{
    public Dictionary<string, PasswordPattern> Patterns { get; set; } = new();
}

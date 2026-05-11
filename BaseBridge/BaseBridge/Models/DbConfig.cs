namespace BaseBridge.Models;

public class DbConfig
{
    public int Id { get; set; }
    public string DbType { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
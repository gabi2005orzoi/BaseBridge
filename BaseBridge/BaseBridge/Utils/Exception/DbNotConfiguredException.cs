namespace BaseBridge.Utils.Exception;

public class DbNotConfiguredException: System.Exception
{
    public DbNotConfiguredException() : base("Db not configured") {}
    public DbNotConfiguredException(string message) : base(message) {}
}
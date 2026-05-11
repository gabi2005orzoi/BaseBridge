namespace BaseBridge.Utils.Exception;

public class QueryValidationException: System.Exception
{
    public QueryValidationException() : base("Invalid query") {}
    public QueryValidationException(string message) : base(message) {}
}
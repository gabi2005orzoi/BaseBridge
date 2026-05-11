namespace BaseBridge.Utils.Exception;

public class EndpointAlreadyExistsException: System.Exception
{
    public EndpointAlreadyExistsException() : base("Endpoint already exists"){}
    public EndpointAlreadyExistsException(string message) : base(message) {}
}
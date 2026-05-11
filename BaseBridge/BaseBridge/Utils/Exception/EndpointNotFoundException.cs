namespace BaseBridge.Utils.Exception;

public class EndpointNotFoundException: System.Exception
{
    public EndpointNotFoundException(): base("Endpoint not found"){}
    public EndpointNotFoundException(string path): base($"No endpoint registered at path: {path}"){}
}
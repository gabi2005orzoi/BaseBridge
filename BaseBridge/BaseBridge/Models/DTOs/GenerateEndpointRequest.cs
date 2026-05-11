namespace BaseBridge.Models.DTOs;

public class GenerateEndpointRequest(string name, string endpointDescription)
{
    public string Name { get; set; } = name;
    public string EndpointDescription { get; set; } = endpointDescription;
    public List<EndpointParameter> Parameters { get; set; } = new();
}
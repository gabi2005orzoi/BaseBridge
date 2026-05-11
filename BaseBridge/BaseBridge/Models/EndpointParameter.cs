namespace BaseBridge.Models;

public class EndpointParameter
{
    public int Id { get; set; }
    public int EndpointDataId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}
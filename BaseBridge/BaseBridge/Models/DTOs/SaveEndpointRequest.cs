using System.ComponentModel.DataAnnotations;

namespace BaseBridge.Models.DTOs;

public class SaveEndpointRequest
{
    public string Name { get; set; } = string.Empty;
    [RegularExpression(@"^[a-zA-Z0-9\-/]+$", ErrorMessage = "Invalid path. The path should contain just letters, digits, hyphens and slashes")]
    public string Path { get; set; } = string.Empty;
    public string EndpointDescription { get; set; } = string.Empty;
    public string ValidatedQuery { get; set; } = string.Empty;
    public List<EndpointParameter> Parameters { get; set; } = new();
}
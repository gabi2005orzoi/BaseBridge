using System.ComponentModel.DataAnnotations;

namespace BaseBridge.Models;

public class EndpointData
{
    public int Id { get; set; }
    [Required] 
    public string Name { get; set; } = null!;
    [Required]
    public string Path { get; set; } = null!;
    [Required]
    public string Query { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<EndpointParameter> Parameters { get; set; } = new();
}
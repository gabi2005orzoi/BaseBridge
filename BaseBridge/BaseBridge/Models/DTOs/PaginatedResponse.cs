namespace BaseBridge.Models.DTOs;

public class PaginatedResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public List<Dictionary<string, object>> Data { get; set; } = new();
}
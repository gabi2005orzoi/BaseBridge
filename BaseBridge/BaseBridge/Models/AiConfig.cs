namespace BaseBridge.Models;

public class AiConfig
{
    public int Id { get; set; }
    public string Provider { get; set; } = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName = string.Empty;
}
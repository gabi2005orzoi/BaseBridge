namespace BaseBridge.Services.AiProviders;

public interface IAiProvider
{
    Task<string> GenerateQueryAsync(string prompt, string apiKey, string modelName);
}
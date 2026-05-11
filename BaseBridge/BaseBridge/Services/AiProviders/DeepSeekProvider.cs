using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseBridge.Utils.Exception;

namespace BaseBridge.Services.AiProviders;

// 1. Definim clar structura pe care o așteaptă DeepSeek folosind [JsonPropertyName]
public class DeepSeekMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class DeepSeekRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public DeepSeekMessage[] Messages { get; set; } = Array.Empty<DeepSeekMessage>();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}

public class DeepSeekProvider : IAiProvider
{
    public async Task<string> GenerateQueryAsync(string prompt, string apiKey, string modelName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // 2. Folosim clasele noastre concrete în loc de obiecte anonime
        var requestBody = new DeepSeekRequest
        {
            Model = string.IsNullOrWhiteSpace(modelName) ? "deepseek-chat" : modelName,
            Messages = new[]
            {
                new DeepSeekMessage { Role = "user", Content = prompt }
            },
            Temperature = 0.2
        };

        // 3. Serializăm. Atributele [JsonPropertyName] garantează formatul corect.
        var jsonString = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.deepseek.com/chat/completions", httpContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new AiQueryGenerationException($"DeepSeek API returned an error: {response.StatusCode} - {errorContent}");
        }

        // 4. Parsăm răspunsul folosind JsonDocument pentru maximă siguranță
        var rawResponse = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(rawResponse);
        
        try
        {
            var content = jsonDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? throw new Exception("Response content was null.");
        }
        catch (Exception ex)
        {
            throw new AiQueryGenerationException("Failed to parse DeepSeek response format." + ex);
        }
    }
}
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseBridge.Services.AiProviders;
using BaseBridge.Utils.Exception;

namespace BaseBridge.Services;

public class GroqMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class GroqRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public GroqMessage[] Messages { get; set; } = Array.Empty<GroqMessage>();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}
public class GroqProvider: IAiProvider
{
    public async Task<string> GenerateQueryAsync(string prompt, string apiKey, string modelName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new GroqRequest
        {
            Model = string.IsNullOrWhiteSpace(modelName) ? "llama-3.3-70b-versatile" : modelName,
            Messages = new[]
            {
                new GroqMessage { Role = "user", Content = prompt }
            },
            Temperature = 0.2
        };
        
        var jsonString = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        // Endpoint-ul oficial Groq pentru Chat Completions
        var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", httpContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new AiQueryGenerationException($"Groq API returned an error: {response.StatusCode} - {errorContent}");
        }

        var rawResponse = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(rawResponse);

        try
        {
            var content = jsonDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("Response content from Groq was null or empty.");
            }

            string rawSql = content.Replace("```sql", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();
            return rawSql;
        }
        catch (Exception e)
        {
            throw new AiQueryGenerationException("Failed to parse Groq response format: " + e.Message);
        }
        
        throw new NotImplementedException();
    }
}
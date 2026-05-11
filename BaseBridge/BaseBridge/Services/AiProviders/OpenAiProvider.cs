using System.ClientModel;
using BaseBridge.Utils.Exception;
using OpenAI;
using OpenAI.Chat;

namespace BaseBridge.Services.AiProviders;

public class OpenAiProvider: IAiProvider
{
    public async Task<string> GenerateQueryAsync(string prompt, string apiKey, string modelName)
    {
        try
        {
            var model = string.IsNullOrWhiteSpace(modelName) ? "gpt-4o-mini" : modelName;

            var chatClient = new ChatClient(model, apiKey);

            ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);

            if (completion.Content == null || completion.Content.Count == 0)
                throw new AiQueryGenerationException();

            string rawSql = completion.Content[0].Text;

            rawSql = rawSql.Replace("```sql", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();

            return rawSql;
        }
        catch (ClientResultException e)
        {
            throw new Exception($"OpenAiError: {e.Message}");
        }
        catch (Exception e)
        {
            throw new Exception($"An unexpected error occurred while generating the query: {e.Message}");
        }
    }
}
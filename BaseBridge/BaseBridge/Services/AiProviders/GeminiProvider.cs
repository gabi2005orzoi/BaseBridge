using BaseBridge.Utils.Exception;
using GenerativeAI;

namespace BaseBridge.Services.AiProviders;

public class GeminiProvider: IAiProvider
{

    public async Task<string> GenerateQueryAsync(string prompt, string apiKey, string modelName)
    {
        try
        {
            var googleAi = new GoogleAi(apiKey);
            var model = googleAi.CreateGeminiModel(string.IsNullOrWhiteSpace(modelName) ? "gemini-2.0-flash" : modelName);
            
            var response = await model.GenerateContentAsync(prompt);

            if (string.IsNullOrWhiteSpace(response.Text))
            {
                throw new Exception("Ai return an empty response");
            }

            string rawSql = response.Text;

            rawSql = rawSql.Replace("```sql", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();
            return rawSql;
        }
        catch (Exception e)
        {
            throw new AiQueryGenerationException(e.Message);
        }
    }
}
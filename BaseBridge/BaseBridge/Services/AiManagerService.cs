using BaseBridge.Database;
using BaseBridge.Models;
using BaseBridge.Models.DTOs;
using BaseBridge.Services.AiProviders;
using BaseBridge.Utils;
using BaseBridge.Utils.Exception;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 

namespace BaseBridge.Services;

public class AiManagerService(
    AppDbContext dbContext,
    ILogger<AiManagerService> logger,
    IConfiguration configuration) 
{
    public async Task<string> GenerateDynamicQueryAsync(string description, DatabaseSchemaResponse dbSchema, string dbType, List<EndpointParameter>? parameters = null)
    {
        var aiConfig = await dbContext.AiConfigs.FirstOrDefaultAsync();

        if (aiConfig == null)
            throw new Exception("AI configuration not found. Please go to settings.");

        string apiKey = "";
        string modelName = aiConfig.ModelName;
        string providerKey = aiConfig.Provider?.ToLower() ?? "groq";
        
        if (providerKey == "groq")
        {
            apiKey = configuration["Groq:DefaultApiKey"];
            
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Server is missing the default Groq API Key. Please check appsettings.json.");
                
            modelName = string.IsNullOrWhiteSpace(modelName) ? "llama-3.3-70b-versatile" : modelName;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(aiConfig.ApiKey))
                throw new Exception("AI provider not configured. Please set your API Key first");
                
            apiKey = EncryptionUtils.Decrypt(aiConfig.ApiKey);
        }

        string prompt = PromptBuilder.BuildSqlPrompt(description, dbSchema, dbType, parameters);

        IAiProvider provider = providerKey switch
        {
            "chatgpt" => new OpenAiProvider(),
            "openai" => new OpenAiProvider(),
            "gemini-custom" => new GeminiProvider(),
            "gemini" => new GeminiProvider(),
            "deepseek" => new DeepSeekProvider(),
            "groq" => new GroqProvider(),
            _ => new GroqProvider() // DeepSeek este acum provider-ul DEFAULT
        };

        try
        {
            return await provider.GenerateQueryAsync(prompt, apiKey, modelName);
        }
        catch (AiQueryGenerationException e)
        {
            logger.LogError(e, "Error generating query with AI Provider: {Provider}", aiConfig.Provider);
            throw new Exception($"AI provider error: {e.Message}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating query with AI Provider: {Provider}", aiConfig.Provider);
            throw new Exception("Unexpected error while generating query.");
        }
    }

    public async Task SaveAiConfig(AiConfig config)
    {
        var existing = await dbContext.AiConfigs.FirstOrDefaultAsync();
        if (existing != null)
            dbContext.AiConfigs.Remove(existing);
            
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            config.ApiKey = EncryptionUtils.Encrypt(config.ApiKey);
        }
        
        dbContext.AiConfigs.Add(config);
        await dbContext.SaveChangesAsync();
    }

    public async Task<AiConfig?> GetAiConfig()
    {
        return await dbContext.AiConfigs.FirstOrDefaultAsync();
    }
}
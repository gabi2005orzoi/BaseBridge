using System.Text;
using BaseBridge.Models;

namespace BaseBridge.Utils;

public static class PromptBuilder
{
    public static string BuildSqlPrompt(string description, string dbSchema, string dbType, List<EndpointParameter>? parameters = null)
    {
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine($"You are an expert SQL developer for {dbType.ToUpper()}. Your task is to translate a natural language request into a valid SQL query based on the provided database schema.");
        prompt.AppendLine("\nCRITICAL SECURITY CONSTRAINTS:");
        prompt.AppendLine("1. You must ONLY generate \"SELECT\" queries.");
        prompt.AppendLine("2. NEVER generate queries that modify data or schema (e.g., NO INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, EXEC).");
        prompt.AppendLine("3. If the user's request asks or implies modifying data, you must not generate a query. Instead, return exactly this string: \"ERROR: Only read operations are allowed.\"");
        prompt.AppendLine("4. IMPORTANT: If the request asks for dynamic filters(e.g., 'by name', 'for a specific status', 'greater than an amount') you MUST use parameterized queries.");
        prompt.AppendLine("5. Format the parameters using the '@' symbol followed by the parameter name in lowercase (e.g., WHERE age > @age AND status = @status).");
        prompt.AppendLine("6. NEVER hardcode filters values unless explicitly told to do so by the user");
        prompt.AppendLine("\nDATABASE SCHEMA:");
        prompt.AppendLine(dbSchema);
        prompt.AppendLine("\nUSER REQUEST:");
        prompt.AppendLine(description);
        if (parameters != null && parameters.Count > 0)
        {
            prompt.AppendLine("\nPARAMETERS DEFINED BY THE USER (MANDATORY):");
            prompt.AppendLine("The user has explicitly defined the following parameters. You MUST use them in the WHERE clause using EXACTLY these names with the '@' prefix:");
            foreach (var param in parameters)
            {
                prompt.AppendLine($"  - @{param.Name} (type: {param.Type})");
            }
            prompt.AppendLine("Do NOT rename them, do NOT use different parameter names, do NOT invent new parameters.");
        }

        prompt.AppendLine("OUTPUT FORMAT:");
        prompt.AppendLine("Return ONLY the raw SQL query.");
        prompt.AppendLine("Do not wrap it in markdown code blocks (like ```sql ... ```).");
        prompt.AppendLine("Do not include any explanations, greetings, or additional text. Just the executable SQL.");

        return prompt.ToString();
    }
}
using System.Text;
using BaseBridge.Models;
using BaseBridge.Models.DTOs;

namespace BaseBridge.Utils;

public static class PromptBuilder
{
    public static string BuildSqlPrompt(string description, DatabaseSchemaResponse schemaResponse, string dbType, List<EndpointParameter>? parameters = null)
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
        
        prompt.AppendLine("\nDATABASE SCHEMA AND SAMPLE DATA:");
        foreach (var table in schemaResponse.Tables)
        {
            prompt.AppendLine($"- Table: {table.Name}");
            foreach (var col in table.Columns)
            {
                prompt.AppendLine($"  * {col.Name} ({col.DataType})");
            }

            if (table.Relationships.Count > 0)
            {
                prompt.AppendLine("  Relationships:");
                foreach (var rel in table.Relationships)
                {
                    prompt.AppendLine($"    - {table.Name}.{rel.Column} -> {rel.ReferencedTable}.{rel.ReferencedColumn}");
                }
            }

            if (table.SampleData.Count > 0)
            {
                prompt.AppendLine("  Sample Data (up to 3 rows):");
                // Get header
                var columns = table.SampleData[0].Keys.ToList();
                prompt.AppendLine($"    {string.Join(" | ", columns)}");
                
                // Get rows
                foreach (var row in table.SampleData)
                {
                    var values = columns.Select(c => row[c]?.ToString() ?? "NULL");
                    prompt.AppendLine($"    {string.Join(" | ", values)}");
                }
            }
            prompt.AppendLine();
        }

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
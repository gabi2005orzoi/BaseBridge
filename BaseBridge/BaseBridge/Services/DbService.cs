using System.Data;
using System.Data.Common;
using System.Text;
using BaseBridge.Database;
using BaseBridge.Models.DTOs;
using GenerativeAI;

namespace BaseBridge.Services;

public class DbService(string dbType, string host, string dbName, string user, string password)
{
    private readonly DynamicConnection _connection = new(dbType, host, dbName, user, password);

    public async Task<List<Dictionary<string, object>>> ExecuteDynamicQueryAsync(string? query, Dictionary<string, object>? parameters = null)
    {
        var result = new List<Dictionary<string, object>>();
        await using DbConnection con = _connection.CreateConnection();
        await con.OpenAsync();
        
        await using DbCommand cmd = con.CreateCommand();
        cmd.CommandText = query;

        if (parameters != null && parameters.Count > 0)
        {
            foreach (var param in parameters)
            {
                var dbParam = cmd.CreateParameter();
                dbParam.ParameterName = param.Key.TrimStart('@');
                object finalValue = param.Value;
                if (finalValue is string strValue)
                {
                    if (int.TryParse(strValue, out int intVal))
                        finalValue = intVal;
                    else if (decimal.TryParse(strValue, System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture, out decimal decVal))
                        finalValue = decVal;
                    else if (bool.TryParse(strValue, out bool boolVal))
                        finalValue = boolVal;
                }
                
                dbParam.Value = finalValue ?? DBNull.Value;

                cmd.Parameters.Add(dbParam);
            }
            
            await cmd.PrepareAsync();
        }

        await using DbDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.GetName(i), reader.GetValue(i));
            }
            
            result.Add(row);
        }

        return result;
    }



    public async Task<DatabaseSchemaResponse> GetFullSchemaAsync()
    {
        var response = new DatabaseSchemaResponse();
        await using var con = _connection.CreateConnection();
        await con.OpenAsync();

        //tables and columns
        string colQuery = dbType.ToLower() switch
        {
            "postgresql" =>
                "SELECT table_name, column_name, data_type FROM information_schema.columns WHERE table_schema = 'public' ORDER BY table_name, ordinal_position;",
            "mysql" =>
                $"SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{dbName}' ORDER BY TABLE_NAME, ORDINAL_POSITION;",
            _ =>
                "SELECT t.name, c.name, tp.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id ORDER BY t.name, c.column_id;"
        };

        var tableDict = new Dictionary<string, TableInfo>();
        await using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = colQuery;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var tableName = reader.GetString(0);
                if (!tableDict.TryGetValue(tableName, out var tableInfo))
                {
                    tableInfo = new TableInfo { Name = tableName };
                    tableDict[tableName] = tableInfo;
                    response.Tables.Add(tableInfo);
                }
                tableInfo.Columns.Add(new ColumnInfo
                {
                    Name = reader.GetString(1),
                    DataType = reader.GetString(2)
                });
            }
        }
        
        // relationships
        string fkQuery = dbType.ToLower() switch
        {
            "postgresql" =>
                "SELECT tc.table_name, kcu.column_name, ccu.table_name, ccu.column_name FROM information_schema.table_constraints tc JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public';",
            "mysql" =>
                $"SELECT TABLE_NAME, COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE REFERENCED_TABLE_SCHEMA = '{dbName}';",
            _ =>
                "SELECT tp.name, cp.name, tr.name, cr.name FROM sys.foreign_keys fk INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id;"
        };

        await using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = fkQuery;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var tableName = reader.GetString(0);
                if (tableDict.TryGetValue(tableName, out var table))
                {
                    table.Relationships.Add(new RelationshipInfo
                    {
                        Column = reader.GetString(1),
                        ReferencedTable = reader.GetString(2),
                        ReferencedColumn = reader.GetString(3)
                    });
                }
            }
        }

        foreach (var table in response.Tables)
        {
            try
            {
                // Incearca sa preia pana la 3 inregistrari. Daca sunt mai putine, SQL-ul va returna automat tot ce gaseste.
                table.SampleData = await GetTableSampleAsync(table.Name, 3);
            }
            catch (Exception ex)
            {
                // Daca apare o eroare (ex: lipsa permisiuni pe un tabel specific), continuam fara sample data pentru acel tabel.
                Console.WriteLine($"Warning: Could not fetch sample data for table {table.Name}: {ex.Message}");
            }
        }

        return response;
    }

    public async Task<List<Dictionary<string, object>>> GetTableSampleAsync(string tableName, int limit)
    {
        string query = dbType.ToLower() switch
        {
            "sqlserver" => $"SELECT TOP {limit} * FROM [{tableName}]",
            _ => $"SELECT * FROM {tableName} LIMIT {limit}"
        };
        return await ExecuteDynamicQueryAsync(query);
    }
}
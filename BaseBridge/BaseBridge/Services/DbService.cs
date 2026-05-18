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
                @"SELECT c.table_name, c.column_name, c.data_type,
                CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key
                FROM information_schema.columns c
                LEFT JOIN (
                    SELECT kcu.table_name, kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu 
                        ON tc.constraint_name = kcu.constraint_name
                    WHERE tc.constraint_type = 'PRIMARY KEY' AND tc.table_schema = 'public'
                ) pk ON c.table_name = pk.table_name AND c.column_name = pk.column_name
                WHERE c.table_schema = 'public'
                ORDER BY c.table_name, c.ordinal_position",
            "mysql" =>
                $@"SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE,
                CASE WHEN c.COLUMN_KEY = 'PRI' THEN true ELSE false END as is_primary_key
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_SCHEMA = '{dbName}'
                ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION",
            _ =>
                @"SELECT t.name, c.name, tp.name,
                CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END as is_primary_key
                FROM sys.tables t
                INNER JOIN sys.columns c ON t.object_id = c.object_id
                INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id
                LEFT JOIN (
                    SELECT ic.object_id, ic.column_id
                    FROM sys.index_columns ic
                    JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                    WHERE i.is_primary_key = 1
                ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
                ORDER BY t.name, c.column_id"
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
                    DataType = reader.GetString(2),
                    IsPrimaryKey = reader.GetValue(3) is not DBNull && Convert.ToBoolean(reader.GetValue(3))
                });
            }
        }
        
        // relationships
        string fkQuery = dbType.ToLower() switch
        {
            "postgresql" =>
                @"SELECT tc.table_name, kcu.column_name, ccu.table_name, ccu.column_name,
                CASE WHEN uq.column_name IS NOT NULL THEN 'one-to-one' ELSE 'one-to-many' END as rel_type
                FROM information_schema.table_constraints tc 
                JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name 
                JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name 
                LEFT JOIN (
                    SELECT kcu2.table_name, kcu2.column_name
                    FROM information_schema.table_constraints tc2
                    JOIN information_schema.key_column_usage kcu2 ON tc2.constraint_name = kcu2.constraint_name
                    WHERE tc2.constraint_type = 'UNIQUE' AND tc2.table_schema = 'public'
                ) uq ON tc.table_name = uq.table_name AND kcu.column_name = uq.column_name
                WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public'",
            "mysql" =>
                $@"SELECT k.TABLE_NAME, k.COLUMN_NAME, k.REFERENCED_TABLE_NAME, k.REFERENCED_COLUMN_NAME,
                CASE WHEN s.NON_UNIQUE = 0 THEN 'one-to-one' ELSE 'one-to-many' END as rel_type
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                LEFT JOIN INFORMATION_SCHEMA.STATISTICS s 
                    ON k.TABLE_SCHEMA = s.TABLE_SCHEMA 
                    AND k.TABLE_NAME = s.TABLE_NAME 
                    AND k.COLUMN_NAME = s.COLUMN_NAME
                    AND s.NON_UNIQUE = 0
                WHERE k.REFERENCED_TABLE_SCHEMA = '{{dbName}}'
                AND k.REFERENCED_TABLE_NAME IS NOT NULL",
            _ =>
                @"SELECT tp.name, cp.name, tr.name, cr.name,
                CASE WHEN uq.column_id IS NOT NULL THEN 'one-to-one' ELSE 'one-to-many' END as rel_type
                FROM sys.foreign_keys fk 
                INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id 
                INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id 
                INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id 
                INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id 
                INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
                LEFT JOIN (
                    SELECT ic.object_id, ic.column_id
                    FROM sys.indexes i
                    JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                    WHERE i.is_unique = 1 AND i.is_primary_key = 0
                ) uq ON cp.object_id = uq.object_id AND cp.column_id = uq.column_id"
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
                        ReferencedColumn = reader.GetString(3),
                        Type = reader.GetString(4)
                    });
                    var fkCol = table.Columns.FirstOrDefault(c => c.Name == reader.GetString(1));
                    if (fkCol != null) fkCol.IsForeignKey = true;
                }
            }
        }

        //samples
        foreach (var table in response.Tables)
        {
            try
            {
                table.SampleData = await GetTableSampleAsync(table.Name, 3);
            }
            catch (Exception ex)
            {
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
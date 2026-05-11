using System.Data;
using System.Data.Common;
using System.Text;
using BaseBridge.Database;
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

    public async Task<string> GetDatabaseSchema()
    {
        await using var con = _connection.CreateConnection();
        await con.OpenAsync();

        var schemaBuilder = new StringBuilder();
        schemaBuilder.AppendLine("Database structure");

        // ==========================================
        // 1. EXTRAGEREA TABELELOR ȘI COLOANELOR
        // ==========================================
        var tables = new Dictionary<string, List<string>>();

        await using (var cmdCols = con.CreateCommand())
        {
            cmdCols.CommandText = dbType.ToLower() switch
            {
                "postgresql" =>
                    "SELECT table_name, column_name, data_type " +
                    "FROM information_schema.columns " +
                    "WHERE table_schema = 'public' " +
                    "ORDER BY table_name, ordinal_position;",
                "mysql" =>
                    "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE " +
                    "FROM INFORMATION_SCHEMA.COLUMNS " +
                    $"WHERE TABLE_SCHEMA = '{dbName}' " +
                    "ORDER BY TABLE_NAME, ORDINAL_POSITION;",
                _ => // SQL Server
                    "SELECT t.name AS table_name, c.name AS column_name, tp.name AS data_type " +
                    "FROM sys.tables t " +
                    "INNER JOIN sys.columns c ON t.object_id = c.object_id " +
                    "INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id " +
                    "ORDER BY t.name, c.column_id;"
            };

            await using var readerCols = await cmdCols.ExecuteReaderAsync();

            while (await readerCols.ReadAsync())
            {
                string tableName = readerCols.GetString(0);
                string columnName = readerCols.GetString(1);
                string dataType = readerCols.GetString(2);

                if (!tables.ContainsKey(tableName))
                    tables[tableName] = new List<string>();

                tables[tableName].Add($"  * {columnName} ({dataType})");
            }
        }

        foreach (var table in tables)
        {
            schemaBuilder.AppendLine($"- Table: {table.Key}");
            foreach (var col in table.Value)
                schemaBuilder.AppendLine(col);
        }

        // ==========================================
        // 2. EXTRAGEREA RELAȚIILOR (FOREIGN KEYS)
        // ==========================================
        schemaBuilder.AppendLine("\nRelationships between tables (Foreign Keys):");

        await using (var cmdFk = con.CreateCommand())
        {
            cmdFk.CommandText = dbType.ToLower() switch
            {
                "postgresql" => @"
                    SELECT
                        tc.table_name,
                        kcu.column_name,
                        ccu.table_name  AS referenced_table_name,
                        ccu.column_name AS referenced_column_name
                    FROM information_schema.table_constraints AS tc
                    JOIN information_schema.key_column_usage AS kcu
                        ON tc.constraint_name = kcu.constraint_name
                        AND tc.table_schema   = kcu.table_schema
                    JOIN information_schema.constraint_column_usage AS ccu
                        ON tc.constraint_name = ccu.constraint_name
                        AND tc.table_schema   = ccu.table_schema
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                      AND tc.table_schema    = 'public'
                    ORDER BY tc.table_name;",

                "mysql" => $@"
                    SELECT
                        kcu.TABLE_NAME,
                        kcu.COLUMN_NAME,
                        kcu.REFERENCED_TABLE_NAME,
                        kcu.REFERENCED_COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        ON  kcu.CONSTRAINT_NAME   = tc.CONSTRAINT_NAME
                        AND kcu.TABLE_SCHEMA      = tc.TABLE_SCHEMA
                        AND kcu.TABLE_NAME        = tc.TABLE_NAME
                    WHERE tc.CONSTRAINT_TYPE        = 'FOREIGN KEY'
                      AND kcu.TABLE_SCHEMA          = '{dbName}'
                    ORDER BY kcu.TABLE_NAME;",

                _ => @" -- SQL Server
                    SELECT
                        tp.name  AS table_name,
                        cp.name  AS column_name,
                        tr.name  AS referenced_table_name,
                        cr.name  AS referenced_column_name
                    FROM sys.foreign_keys fk
                    INNER JOIN sys.tables             tp  ON fk.parent_object_id      = tp.object_id
                    INNER JOIN sys.tables             tr  ON fk.referenced_object_id  = tr.object_id
                    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                    INNER JOIN sys.columns            cp  ON fkc.parent_column_id     = cp.column_id
                                                         AND fkc.parent_object_id     = cp.object_id
                    INNER JOIN sys.columns            cr  ON fkc.referenced_column_id = cr.column_id
                                                         AND fkc.referenced_object_id = cr.object_id
                    ORDER BY tp.name;"
            };

            await using var readerFk = await cmdFk.ExecuteReaderAsync();

            while (await readerFk.ReadAsync())
            {
                string tableName    = readerFk.GetString(0);
                string columnName   = readerFk.GetString(1);
                string refTable     = readerFk.GetString(2);
                string refColumn    = readerFk.GetString(3);

                schemaBuilder.AppendLine($"- {tableName}.{columnName} -> {refTable}.{refColumn}");
            }
        }

        return schemaBuilder.ToString();
    }
}
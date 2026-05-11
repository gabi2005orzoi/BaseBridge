using BaseBridge.Utils.Exception;

namespace BaseBridge.Validations;

public class QueryValidator
{
    public void Validate(string? query, string dbType)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryValidationException("Query can't be empty");
        SqlParser.Dialects.Dialect dialect = dbType.ToLower() switch
        {
            "postgres" => new SqlParser.Dialects.PostgreSqlDialect(),
            "mysql" => new SqlParser.Dialects.MySqlDialect(),
            "sqlserver" => new SqlParser.Dialects.MsSqlDialect(),
            _ => new SqlParser.Dialects.GenericDialect()
        };

        try
        {
            var parser = new SqlParser.Parser();
            var ast = parser.ParseSql(query, dialect);

            if (ast == null || ast.Count == 0)
                throw new QueryValidationException("Couldn't parse the SQL query");

            if (ast.Count > 1)
                throw new QueryValidationException("Only single queries(a single statement) are allowed");

            if (ast[0] is not SqlParser.Ast.Statement.Select)
                throw new QueryValidationException();
        }
        catch(QueryValidationException)
        {
            throw;
        }
        catch(Exception ex)
        {
            throw new QueryValidationException($"Syntax error for dialect: {dbType}. Details: {ex.Message}");
        }
    }
}
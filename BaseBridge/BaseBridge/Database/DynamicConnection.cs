using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;

namespace BaseBridge.Database;

public class DynamicConnection(string dbType, string host, string dbName, string user, string password)
{
    public DbConnection CreateConnection()
    {
        DbConnectionStringBuilder builder;
        switch (dbType.ToLower())
        {
            case "sqlserver":
                builder = new SqlConnectionStringBuilder
                {
                    DataSource = host,
                    InitialCatalog = dbName,
                    UserID = user,
                    Password = password,
                    TrustServerCertificate = true
                };
                return new SqlConnection(builder.ConnectionString);
            case "postgresql":
                builder = new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Database = dbName,
                    Username = user,
                    Password = password
                };
                return new NpgsqlConnection(builder.ConnectionString);
            case "mysql":
                builder = new MySqlConnectionStringBuilder
                {
                    Server = host,
                    Database = dbName,
                    UserID = user,
                    Password = password,
                };
                return new MySqlConnection(builder.ConnectionString);
            default:
                throw new NotSupportedException("This type of database is not supported yet");
        }
    }
}
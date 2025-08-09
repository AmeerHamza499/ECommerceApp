using System.Data;
using Microsoft.Data.Sqlite;

namespace ProductManager.Infrastructure.Data;

public interface ISqliteConnectionFactory
{
    IDbConnection Create();
}

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        return connection;
    }
}
using Dapper;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Core.Domain.Entities;

namespace ProductManager.Infrastructure.Data;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public DatabaseInitializer(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        connection.Open();

        // Create tables
        const string createCategory = @"CREATE TABLE IF NOT EXISTS Category (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL UNIQUE
        );";

        const string createProduct = @"CREATE TABLE IF NOT EXISTS Product (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProductName TEXT NOT NULL,
            Sku TEXT NOT NULL UNIQUE,
            Price REAL NOT NULL CHECK (Price > 0),
            CategoryId INTEGER NOT NULL,
            Status INTEGER NOT NULL,
            PhotoPath TEXT NULL,
            FOREIGN KEY(CategoryId) REFERENCES Category(Id)
        );";

        await connection.ExecuteAsync(createCategory);
        await connection.ExecuteAsync(createProduct);

        // Seed categories
        const string countCategories = "SELECT COUNT(*) FROM Category";
        var categoryCount = await connection.ExecuteScalarAsync<int>(countCategories);
        if (categoryCount == 0)
        {
            const string insertCategories = @"INSERT INTO Category (Id, Name) VALUES
                (1, 'Electronics'),
                (2, 'Home Appliances'),
                (3, 'Books'),
                (4, 'Furniture');";
            await connection.ExecuteAsync(insertCategories);
        }
    }
}
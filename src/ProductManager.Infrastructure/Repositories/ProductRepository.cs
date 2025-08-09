using Dapper;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Core.Domain.Entities;
using ProductStatus = ProductManager.Core.Domain.Entities.ProductStatus;

namespace ProductManager.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly Data.ISqliteConnectionFactory _connectionFactory;

    public ProductRepository(Data.ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        const string sql = @"INSERT INTO Product (ProductName, Sku, Price, CategoryId, Status, PhotoPath)
                             VALUES (@ProductName, @Sku, @Price, @CategoryId, @Status, @PhotoPath);
                             SELECT last_insert_rowid();";
        using var connection = _connectionFactory.Create();
        connection.Open();
        var id = await connection.ExecuteScalarAsync<long>(sql, new
        {
            product.ProductName,
            product.Sku,
            product.Price,
            product.CategoryId,
            Status = (int)product.Status,
            product.PhotoPath
        });
        return (int)id;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        const string sql = @"UPDATE Product SET
                                ProductName = @ProductName,
                                Sku = @Sku,
                                Price = @Price,
                                CategoryId = @CategoryId,
                                Status = @Status,
                                PhotoPath = @PhotoPath
                             WHERE Id = @Id";
        using var connection = _connectionFactory.Create();
        connection.Open();
        await connection.ExecuteAsync(sql, new
        {
            product.ProductName,
            product.Sku,
            product.Price,
            product.CategoryId,
            Status = (int)product.Status,
            product.PhotoPath,
            product.Id
        });
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM Product WHERE Id = @Id";
        using var connection = _connectionFactory.Create();
        connection.Open();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id, ProductName, Sku, Price, CategoryId, Status, PhotoPath FROM Product WHERE Id = @Id";
        using var connection = _connectionFactory.Create();
        connection.Open();
        var product = await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
        return product;
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId, CancellationToken cancellationToken)
    {
        const string sql = @"SELECT COUNT(1) FROM Product WHERE Sku = @Sku AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";
        using var connection = _connectionFactory.Create();
        connection.Open();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Sku = sku, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string? search, int? categoryId, CancellationToken cancellationToken)
    {
        string sql = @"SELECT Id, ProductName, Sku, Price, CategoryId, Status, PhotoPath
                       FROM Product
                       WHERE ( (@Search IS NULL) OR (ProductName LIKE '%' || @Search || '%' OR Sku LIKE '%' || @Search || '%') )
                         AND ( (@CategoryId IS NULL) OR (CategoryId = @CategoryId) )
                       ORDER BY Id DESC";
        using var connection = _connectionFactory.Create();
        connection.Open();
        var results = await connection.QueryAsync<Product>(sql, new { Search = string.IsNullOrWhiteSpace(search) ? null : search, CategoryId = categoryId });
        return results.ToList();
    }
}
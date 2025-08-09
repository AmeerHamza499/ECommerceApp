using Dapper;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Core.Domain.Entities;

namespace ProductManager.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly Data.ISqliteConnectionFactory _connectionFactory;

    public CategoryRepository(Data.ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id, Name FROM Category ORDER BY Name";
        using var connection = _connectionFactory.Create();
        connection.Open();
        var results = await connection.QueryAsync<Category>(sql);
        return results.ToList();
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id, Name FROM Category WHERE Id = @Id";
        using var connection = _connectionFactory.Create();
        connection.Open();
        return await connection.QuerySingleOrDefaultAsync<Category>(sql, new { Id = id });
    }
}
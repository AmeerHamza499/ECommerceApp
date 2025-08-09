using ProductManager.Core.Domain.Entities;

namespace ProductManager.Core.Application.Interfaces;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, int? excludeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> SearchAsync(string? search, int? categoryId, CancellationToken cancellationToken);
}
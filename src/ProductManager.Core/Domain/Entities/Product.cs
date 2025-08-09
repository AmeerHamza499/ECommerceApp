namespace ProductManager.Core.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public ProductStatus Status { get; set; }
    public string? PhotoPath { get; set; }
}

public enum ProductStatus
{
    Inactive = 0,
    Active = 1
}
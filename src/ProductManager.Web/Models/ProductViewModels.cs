using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProductManager.Core.Domain.Entities;

namespace ProductManager.Web.Models;

public class ProductListFilterViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public string? PhotoPath { get; set; }
}

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Required]
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public IFormFile? Photo { get; set; }
    public string? ExistingPhotoPath { get; set; }

    public IEnumerable<Category> Categories { get; set; } = Array.Empty<Category>();
}
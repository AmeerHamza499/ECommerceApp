using FluentValidation;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Web.Models;

namespace ProductManager.Web.Validators;

public class ProductFormViewModelValidator : AbstractValidator<ProductFormViewModel>
{
    public ProductFormViewModelValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductName)
            .NotEmpty();

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MustAsync(async (model, sku, ct) => !await productRepository.SkuExistsAsync(sku, model.Id, ct))
            .WithMessage("SKU must be unique");

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("Category is required");

        RuleFor(x => x.Photo)
            .Must(file => file == null || new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage("Only .jpg, .jpeg, .png are allowed")
            .Must(file => file == null || file.Length <= 25 * 1024)
            .WithMessage("Image must be less than 25 KB");
    }
}
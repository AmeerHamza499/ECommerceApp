using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Core.Domain.Entities;
using ProductManager.Web.Models;

namespace ProductManager.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IValidator<ProductFormViewModel> _validator;

    public ProductsController(IProductRepository productRepository,
                              ICategoryRepository categoryRepository,
                              IWebHostEnvironment env,
                              IValidator<ProductFormViewModel> validator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _env = env;
        _validator = validator;
    }

    [HttpGet("/products")]
    public async Task<IActionResult> Index([FromQuery] ProductListFilterViewModel filter, CancellationToken ct)
    {
        var products = await _productRepository.SearchAsync(filter.Search, filter.CategoryId, ct);
        var categories = await _categoryRepository.GetAllAsync(ct);
        var categoryLookup = categories.ToDictionary(c => c.Id, c => c.Name);
        var viewModel = products.Select(p => new ProductListItemViewModel
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Sku = p.Sku,
            Price = p.Price,
            CategoryName = categoryLookup.TryGetValue(p.CategoryId, out var name) ? name : "",
            Status = p.Status,
            PhotoPath = p.PhotoPath
        }).ToList();
        ViewBag.Categories = categories;
        ViewBag.Filter = filter;
        return View(viewModel);
    }

    [HttpGet("/products/add")]
    public async Task<IActionResult> Add(CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        var vm = new ProductFormViewModel
        {
            Categories = categories,
            Status = ProductStatus.Active
        };
        return PartialView("_ProductForm", vm);
    }

    [HttpPost("/products/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductFormViewModel vm, CancellationToken ct)
    {
        vm.Categories = await _categoryRepository.GetAllAsync(ct);
        var validation = await _validator.ValidateAsync(vm, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return PartialView("_ProductForm", vm);
        }

        string? savedPath = null;
        if (vm.Photo != null && vm.Photo.Length > 0)
        {
            savedPath = await SavePhotoAsync(vm.Photo, ct);
        }

        var product = new Product
        {
            ProductName = vm.ProductName.Trim(),
            Sku = vm.Sku.Trim(),
            Price = vm.Price,
            CategoryId = vm.CategoryId!.Value,
            Status = vm.Status,
            PhotoPath = savedPath
        };
        await _productRepository.CreateAsync(product, ct);
        return Json(new { success = true });
    }

    [HttpGet("/products/edit/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null) return NotFound();
        var categories = await _categoryRepository.GetAllAsync(ct);
        var vm = new ProductFormViewModel
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Sku = product.Sku,
            Price = product.Price,
            CategoryId = product.CategoryId,
            Status = product.Status,
            ExistingPhotoPath = product.PhotoPath,
            Categories = categories
        };
        return View(vm);
    }

    [HttpPost("/products/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, ProductFormViewModel vm, CancellationToken ct)
    {
        vm.Id = id;
        vm.Categories = await _categoryRepository.GetAllAsync(ct);
        var validation = await _validator.ValidateAsync(vm, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(vm);
        }

        string? savedPath = vm.ExistingPhotoPath;
        if (vm.Photo != null && vm.Photo.Length > 0)
        {
            savedPath = await SavePhotoAsync(vm.Photo, ct);
        }

        var product = new Product
        {
            Id = id,
            ProductName = vm.ProductName.Trim(),
            Sku = vm.Sku.Trim(),
            Price = vm.Price,
            CategoryId = vm.CategoryId!.Value,
            Status = vm.Status,
            PhotoPath = savedPath
        };
        await _productRepository.UpdateAsync(product, ct);
        TempData["Message"] = "Product updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/products/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _productRepository.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SavePhotoAsync(IFormFile file, CancellationToken ct)
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsRoot, fileName);
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, ct);
        }
        var relativePath = $"/uploads/{fileName}";
        return relativePath;
    }
}
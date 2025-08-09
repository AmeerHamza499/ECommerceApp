using FluentValidation;
using FluentValidation.AspNetCore;
using ProductManager.Core.Application.Interfaces;
using ProductManager.Infrastructure.Data;
using ProductManager.Infrastructure.Repositories;
using ProductManager.Web.Models;
using ProductManager.Web.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// SQLite connection string and factory
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dbPath);
var connectionString = $"Data Source={Path.Combine(dbPath, "products.db")}";

builder.Services.AddSingleton<ISqliteConnectionFactory>(sp => new SqliteConnectionFactory(connectionString));

// Dapper repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// DB initializer
builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddTransient<IValidator<ProductFormViewModel>, ProductFormViewModelValidator>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

await app.RunAsync();

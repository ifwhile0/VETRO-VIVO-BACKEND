using Microsoft.EntityFrameworkCore;
using VetroVivo.API.DTOs;
using VetroVivo.API.Models;

namespace VetroVivo.API.Services;

public interface IProductService
{
    Task<PaginatedResponse<ProductDto>> GetProductsAsync(Guid storeId, int pageNumber = 1, int pageSize = 20);
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
    Task<ProductDto?> GetProductBySlugAsync(Guid storeId, string slug);
    Task<PaginatedResponse<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, int pageNumber = 1, int pageSize = 20);
    Task<PaginatedResponse<ProductDto>> SearchProductsAsync(Guid storeId, string searchTerm, int pageNumber = 1, int pageSize = 20);
    Task<ApiResponse<ProductDto>> CreateProductAsync(Guid storeId, CreateProductRequest request);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid productId, CreateProductRequest request);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid productId);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductDto>> GetProductsAsync(Guid storeId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Products.Where(p => p.StoreId == storeId && p.IsActive);
        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Category)
            .ToListAsync();

        return new PaginatedResponse<ProductDto>
        {
            Success = true,
            Items = products.Select(MapToProductDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        return product != null ? MapToProductDto(product) : null;
    }

    public async Task<ProductDto?> GetProductBySlugAsync(Guid storeId, string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Slug == slug);

        return product != null ? MapToProductDto(product) : null;
    }

    public async Task<PaginatedResponse<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Products.Where(p => p.CategoryId == categoryId && p.IsActive);
        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Category)
            .ToListAsync();

        return new PaginatedResponse<ProductDto>
        {
            Success = true,
            Items = products.Select(MapToProductDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PaginatedResponse<ProductDto>> SearchProductsAsync(Guid storeId, string searchTerm, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Products.Where(p =>
            p.StoreId == storeId &&
            p.IsActive &&
            (p.Name.Contains(searchTerm) || p.Description!.Contains(searchTerm) || p.Sku.Contains(searchTerm))
        );

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Category)
            .ToListAsync();

        return new PaginatedResponse<ProductDto>
        {
            Success = true,
            Items = products.Select(MapToProductDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(Guid storeId, CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                StoreId = storeId,
                CategoryId = request.CategoryId,
                Sku = request.Sku,
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                LongDescription = request.LongDescription,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Cost = request.Cost,
                Weight = request.Weight,
                Dimensions = request.Dimensions,
                StockQuantity = request.StockQuantity,
                ReservedQuantity = 0,
                Images = request.Images,
                IsFeatured = request.IsFeatured,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Produto criado: {product.ProductId}");

            return new ApiResponse<ProductDto>
            {
                Success = true,
                Message = "Produto criado com sucesso",
                Data = await GetProductByIdAsync(product.ProductId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar produto: {ex.Message}");
            return new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Erro ao criar produto"
            };
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid productId, CreateProductRequest request)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Produto não encontrado"
                };
            }

            product.Name = request.Name;
            product.Slug = request.Slug;
            product.Description = request.Description;
            product.LongDescription = request.LongDescription;
            product.Price = request.Price;
            product.DiscountPrice = request.DiscountPrice;
            product.Cost = request.Cost;
            product.Weight = request.Weight;
            product.Dimensions = request.Dimensions;
            product.StockQuantity = request.StockQuantity;
            product.Images = request.Images;
            product.IsFeatured = request.IsFeatured;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Produto {productId} atualizado");

            return new ApiResponse<ProductDto>
            {
                Success = true,
                Message = "Produto atualizado com sucesso",
                Data = await GetProductByIdAsync(productId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar produto: {ex.Message}");
            return new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Erro ao atualizar produto"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Produto não encontrado",
                    Data = false
                };
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Produto {productId} deletado");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Produto deletado com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao deletar produto: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao deletar produto",
                Data = false
            };
        }
    }

    private ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Sku = product.Sku,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            Images = product.Images,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name
        };
    }
}

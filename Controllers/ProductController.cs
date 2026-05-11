using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;
    private readonly Guid _storeId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetProductsAsync(_storeId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new ApiResponse<ProductDto> 
            { 
                Success = false, 
                Message = "Produto não encontrado" 
            });

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Data = product
        });
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductBySlug(string slug)
    {
        var product = await _productService.GetProductBySlugAsync(_storeId, slug);
        if (product == null)
            return NotFound(new ApiResponse<ProductDto> 
            { 
                Success = false, 
                Message = "Produto não encontrado" 
            });

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Data = product
        });
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsByCategory(Guid categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetProductsByCategoryAsync(categoryId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("search/{searchTerm}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> SearchProducts(string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _productService.SearchProductsAsync(_storeId, searchTerm, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.CreateProductAsync(_storeId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data?.ProductId }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(Guid id, [FromBody] CreateProductRequest request)
    {
        var result = await _productService.UpdateProductAsync(id, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

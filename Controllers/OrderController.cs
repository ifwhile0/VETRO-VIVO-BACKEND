using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;
    private readonly Guid _storeId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new ApiResponse<OrderDto> 
            { 
                Success = false, 
                Message = "Pedido não encontrado" 
            });

        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Data = order
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var customerId = GetCustomerId();
        var orders = await _orderService.GetCustomerOrdersAsync(customerId, pageNumber, pageSize);
        
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = GetCustomerId();
        var result = await _orderService.CreateOrderAsync(customerId, _storeId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Data?.OrderId }, result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateOrderStatus(Guid id, [FromBody] dynamic statusUpdate)
    {
        var status = statusUpdate.status as string;
        var result = await _orderService.UpdateOrderStatusAsync(id, status);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    private Guid GetCustomerId()
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(customerId ?? Guid.Empty.ToString());
    }
}

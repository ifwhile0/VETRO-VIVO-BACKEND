using Microsoft.EntityFrameworkCore;
using VetroVivo.API.DTOs;
using VetroVivo.API.Models;

namespace VetroVivo.API.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
    Task<List<OrderDto>> GetCustomerOrdersAsync(Guid customerId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid customerId, Guid storeId, CreateOrderRequest request);
    Task<ApiResponse<bool>> UpdateOrderStatusAsync(Guid orderId, string status);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        return order != null ? MapToOrderDto(order) : null;
    }

    public async Task<List<OrderDto>> GetCustomerOrdersAsync(Guid customerId, int pageNumber = 1, int pageSize = 10)
    {
        var orders = await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid customerId, Guid storeId, CreateOrderRequest request)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        OrderNumber = GenerateOrderNumber(),
                        StoreId = storeId,
                        CustomerId = customerId,
                        Status = "pending",
                        PaymentStatus = "unpaid",
                        ShippingMethod = request.ShippingMethod,
                        ShippingAddressId = request.ShippingAddressId,
                        BillingAddressId = request.BillingAddressId ?? request.ShippingAddressId,
                        Notes = request.Notes,
                        OrderDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    decimal subtotal = 0;
                    foreach (var itemRequest in request.Items)
                    {
                        var product = await _context.Products.FindAsync(itemRequest.ProductId);
                        if (product == null)
                        {
                            return new ApiResponse<OrderDto>
                            {
                                Success = false,
                                Message = $"Produto {itemRequest.ProductId} não encontrado"
                            };
                        }

                        if (product.StockQuantity < itemRequest.Quantity)
                        {
                            return new ApiResponse<OrderDto>
                            {
                                Success = false,
                                Message = $"Quantidade insuficiente de {product.Name}"
                            };
                        }

                        var price = product.DiscountPrice ?? product.Price;
                        var itemTotal = price * itemRequest.Quantity;
                        subtotal += itemTotal;

                        var orderItem = new OrderItem
                        {
                            OrderItemId = Guid.NewGuid(),
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            ProductName = product.Name,
                            ProductSku = product.Sku,
                            Quantity = itemRequest.Quantity,
                            UnitPrice = price,
                            TotalPrice = itemTotal
                        };

                        order.Items.Add(orderItem);

                        // Atualizar estoque
                        product.StockQuantity -= itemRequest.Quantity;
                        product.ReservedQuantity += itemRequest.Quantity;
                    }

                    order.SubtotalAmount = subtotal;
                    order.TaxAmount = Math.Round(subtotal * 0.15m, 2); // 15% de imposto
                    order.ShippingAmount = 15; // Valor padrão
                    order.TotalAmount = order.SubtotalAmount + order.TaxAmount + order.ShippingAmount;

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Pedido criado: {order.OrderNumber}");

                    return new ApiResponse<OrderDto>
                    {
                        Success = true,
                        Message = "Pedido criado com sucesso",
                        Data = await GetOrderByIdAsync(order.OrderId)
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar pedido: {ex.Message}");
            return new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Erro ao criar pedido"
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Pedido não encontrado",
                    Data = false
                };
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == "shipped")
                order.ShippedDate = DateTime.UtcNow;
            else if (status == "delivered")
                order.DeliveredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Status do pedido atualizado",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar status: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao atualizar status",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Pedido não encontrado",
                    Data = false
                };
            }

            if (order.Status != "pending" && order.Status != "confirmed")
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Não é possível cancelar pedido neste status",
                    Data = false
                };
            }

            // Devolver estoque
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.ReservedQuantity -= item.Quantity;
                }
            }

            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Pedido {orderId} cancelado");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Pedido cancelado com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao cancelar pedido: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao cancelar pedido",
                Data = false
            };
        }
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubtotalAmount = order.SubtotalAmount,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            Items = order.Items.Select(i => new OrderItemDto
            {
                OrderItemId = i.OrderItemId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSku = i.ProductSku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            OrderDate = order.OrderDate
        };
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}

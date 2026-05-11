namespace VetroVivo.API.DTOs;

// Auth DTOs
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CPF { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public CustomerDto? Customer { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

// Customer DTOs
public class CustomerDto
{
    public Guid CustomerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateCustomerRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public bool? NewsletterOptIn { get; set; }
    public bool? SMSOptIn { get; set; }
    public bool? PushOptIn { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Product DTOs
public class ProductDto
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateProductRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LongDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Cost { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
    public List<string>? Images { get; set; }
    public bool IsFeatured { get; set; }
}

// Order DTOs
public class OrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime OrderDate { get; set; }
}

public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public Guid ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// Budget DTOs
public class BudgetDto
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public List<BudgetItemDto> Items { get; set; } = new();
}

public class BudgetItemDto
{
    public Guid BudgetItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateBudgetRequest
{
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateBudgetItemRequest> Items { get; set; } = new();
    public decimal? DiscountAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateBudgetItemRequest
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// Maintenance DTOs
public class MaintenanceDto
{
    public Guid MaintenanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? Cost { get; set; }
    public List<MaintenanceTaskDto> Tasks { get; set; } = new();
}

public class MaintenanceTaskDto
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}

public class CreateMaintenanceRequest
{
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public Guid? AquariumProjectId { get; set; }
    public List<CreateMaintenanceTaskRequest> Tasks { get; set; } = new();
}

public class CreateMaintenanceTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// Aquarium Project DTOs
public class AquariumProjectDto
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? BudgetAmount { get; set; }
    public decimal? SpentAmount { get; set; }
    public string? Description { get; set; }
    public int? VolumeLiters { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateAquariumProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? BudgetAmount { get; set; }
    public string? Description { get; set; }
    public int? VolumeLiters { get; set; }
}

// Address DTOs
public class AddressDto
{
    public Guid AddressId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "BR";
    public bool IsDefault { get; set; }
}

public class CreateAddressRequest
{
    public string Type { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "BR";
    public bool IsDefault { get; set; }
}

// Generic Response
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class PaginatedResponse<T>
{
    public bool Success { get; set; }
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

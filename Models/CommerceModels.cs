namespace VetroVivo.API.Models;

public class Product
{
    public Guid ProductId { get; set; }
    public Guid StoreId { get; set; }
    public Guid CategoryId { get; set; }
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
    public int ReservedQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Store? Store { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class Category
{
    public Guid CategoryId { get; set; }
    public Guid StoreId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Store? Store { get; set; }
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Order
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = "pending"; // 'pending', 'confirmed', 'shipped', 'delivered', 'cancelled'
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = "unpaid"; // 'unpaid', 'paid', 'partially_paid', 'refunded'
    public string? PaymentMethod { get; set; }
    public string? ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public Guid? ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public string? Notes { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Store? Store { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderItem
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Dictionary<string, object>? SelectedAttributes { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string Method { get; set; } = string.Empty; // 'credit_card', 'debit_card', 'pix', 'boleto'
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending"; // 'pending', 'processing', 'approved', 'declined', 'cancelled'
    public string? TransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Order? Order { get; set; }
}

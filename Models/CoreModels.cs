namespace VetroVivo.API.Models;

public class Store
{
    public Guid StoreId { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string StoreSlug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string Currency { get; set; } = "BRL";
    public string Timezone { get; set; } = "America/Sao_Paulo";
    public string Locale { get; set; } = "pt-BR";
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object>? ThemeConfig { get; set; }
    public Dictionary<string, object>? MetaConfig { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Customer
{
    public Guid CustomerId { get; set; }
    public Guid StoreId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Phone { get; set; }
    public bool PhoneVerified { get; set; }
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public bool NewsletterOptIn { get; set; }
    public bool SMSOptIn { get; set; }
    public bool PushOptIn { get; set; }
    public string PreferredLanguage { get; set; } = "pt";
    public string? CustomerSegment { get; set; }
    public decimal? LtvScore { get; set; }
    public decimal? ChurnProbability { get; set; }
    public string? AcquisitionChannel { get; set; }
    public string? ReferralCode { get; set; }
    public Guid? ReferredBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual Store? Store { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<CustomerSession> Sessions { get; set; } = new List<CustomerSession>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}

public class CustomerSession
{
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StoreId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceOs { get; set; }
    public string? Browser { get; set; }
    public string? BrowserVersion { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Customer? Customer { get; set; }
    public virtual Store? Store { get; set; }
}

public class Address
{
    public Guid AddressId { get; set; }
    public Guid CustomerId { get; set; }
    public string Type { get; set; } = "billing"; // 'billing', 'shipping'
    public string FullName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "BR";
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }
}

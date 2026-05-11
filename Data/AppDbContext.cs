using Microsoft.EntityFrameworkCore;
using VetroVivo.API.Models;

namespace VetroVivo.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core
    public DbSet<Store> Stores { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerSession> CustomerSessions { get; set; }
    public DbSet<Address> Addresses { get; set; }

    // Commerce
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Services
    public DbSet<Maintenance> Maintenances { get; set; }
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    public DbSet<AquariumProject> AquariumProjects { get; set; }
    public DbSet<AquariumInventory> AquariumInventories { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetItem> BudgetItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store Configuration
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId);
            entity.HasIndex(e => e.StoreCode).IsUnique();
            entity.HasIndex(e => e.StoreSlug).IsUnique();
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.Property(e => e.ThemeConfig).HasColumnType("jsonb");
            entity.Property(e => e.MetaConfig).HasColumnType("jsonb");
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.HasIndex(e => new { e.StoreId, e.Email }).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CPF);
            entity.HasOne(e => e.Store)
                .WithMany(s => s.Customers)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer Session Configuration
        modelBuilder.Entity<CustomerSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Sessions)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Store)
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Address Configuration
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId);
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.HasIndex(e => e.Sku);
            entity.HasIndex(e => e.Slug);
            entity.Property(e => e.Images).HasColumnType("jsonb");
            entity.Property(e => e.Attributes).HasColumnType("jsonb");
            entity.HasOne(e => e.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.HasIndex(e => e.Slug);
            entity.HasOne(e => e.Store)
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasOne(e => e.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.SelectedAttributes).HasColumnType("jsonb");
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Maintenance Configuration
        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId);
            entity.Property(e => e.PhotoUrls).HasColumnType("jsonb");
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AquariumProject)
                .WithMany(p => p.MaintenanceRecords)
                .HasForeignKey(e => e.AquariumProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MaintenanceTask Configuration
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(e => e.TaskId);
            entity.HasOne(e => e.Maintenance)
                .WithMany(m => m.Tasks)
                .HasForeignKey(e => e.MaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AquariumProject Configuration
        modelBuilder.Entity<AquariumProject>(entity =>
        {
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.PhotoUrls).HasColumnType("jsonb");
            entity.Property(e => e.ConfigurationDetails).HasColumnType("jsonb");
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AquariumInventory Configuration
        modelBuilder.Entity<AquariumInventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Inventory)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Budget Configuration
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId);
            entity.HasIndex(e => e.BudgetNumber).IsUnique();
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BudgetItem Configuration
        modelBuilder.Entity<BudgetItem>(entity =>
        {
            entity.HasKey(e => e.BudgetItemId);
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Items)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

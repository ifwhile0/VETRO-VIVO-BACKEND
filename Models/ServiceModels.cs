namespace VetroVivo.API.Models;

public class Maintenance
{
    public Guid MaintenanceId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? AquariumProjectId { get; set; }
    public string Status { get; set; } = "scheduled"; // 'scheduled', 'in_progress', 'completed', 'cancelled'
    public string Type { get; set; } = string.Empty; // 'water_change', 'cleaning', 'inspection', 'emergency'
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? TechnicianNotes { get; set; }
    public List<string>? PhotoUrls { get; set; }
    public decimal? Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual AquariumProject? AquariumProject { get; set; }
    public virtual ICollection<MaintenanceTask> Tasks { get; set; } = new List<MaintenanceTask>();
}

public class MaintenanceTask
{
    public Guid TaskId { get; set; }
    public Guid MaintenanceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual Maintenance? Maintenance { get; set; }
}

public class AquariumProject
{
    public Guid ProjectId { get; set; }
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 'freshwater', 'saltwater', 'aquatic_plant'
    public string Status { get; set; } = "planning"; // 'planning', 'active', 'inactive', 'completed'
    public decimal? BudgetAmount { get; set; }
    public decimal? SpentAmount { get; set; }
    public string? Description { get; set; }
    public int? VolumeLiters { get; set; }
    public List<string>? PhotoUrls { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Dictionary<string, object>? ConfigurationDetails { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
    public virtual ICollection<AquariumInventory> Inventory { get; set; } = new List<AquariumInventory>();
}

public class AquariumInventory
{
    public Guid InventoryId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual AquariumProject? Project { get; set; }
    public virtual Product? Product { get; set; }
}

public class Budget
{
    public Guid BudgetId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = "draft"; // 'draft', 'sent', 'accepted', 'rejected', 'expired'
    public string Type { get; set; } = string.Empty; // 'project', 'maintenance'
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();
}

public class BudgetItem
{
    public Guid BudgetItemId { get; set; }
    public Guid BudgetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public virtual Budget? Budget { get; set; }
}

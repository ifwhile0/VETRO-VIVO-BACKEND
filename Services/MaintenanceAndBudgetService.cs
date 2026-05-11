using Microsoft.EntityFrameworkCore;
using VetroVivo.API.DTOs;
using VetroVivo.API.Models;

namespace VetroVivo.API.Services;

public interface IMaintenanceService
{
    Task<MaintenanceDto?> GetMaintenanceByIdAsync(Guid maintenanceId);
    Task<List<MaintenanceDto>> GetCustomerMaintenancesAsync(Guid customerId);
    Task<ApiResponse<MaintenanceDto>> CreateMaintenanceAsync(Guid customerId, CreateMaintenanceRequest request);
    Task<ApiResponse<bool>> CompleteMaintenanceAsync(Guid maintenanceId, string? notes);
}

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(AppDbContext context, ILogger<MaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaintenanceDto?> GetMaintenanceByIdAsync(Guid maintenanceId)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Tasks)
            .FirstOrDefaultAsync(m => m.MaintenanceId == maintenanceId);

        return maintenance != null ? MapToMaintenanceDto(maintenance) : null;
    }

    public async Task<List<MaintenanceDto>> GetCustomerMaintenancesAsync(Guid customerId)
    {
        var maintenances = await _context.Maintenances
            .Where(m => m.CustomerId == customerId)
            .Include(m => m.Tasks)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

        return maintenances.Select(MapToMaintenanceDto).ToList();
    }

    public async Task<ApiResponse<MaintenanceDto>> CreateMaintenanceAsync(Guid customerId, CreateMaintenanceRequest request)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<MaintenanceDto>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            var maintenance = new Maintenance
            {
                MaintenanceId = Guid.NewGuid(),
                CustomerId = customerId,
                AquariumProjectId = request.AquariumProjectId,
                Type = request.Type,
                Description = request.Description,
                Status = "scheduled",
                ScheduledDate = request.ScheduledDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var taskRequest in request.Tasks)
            {
                var task = new MaintenanceTask
                {
                    TaskId = Guid.NewGuid(),
                    MaintenanceId = maintenance.MaintenanceId,
                    Title = taskRequest.Title,
                    Description = taskRequest.Description,
                    IsCompleted = false
                };
                maintenance.Tasks.Add(task);
            }

            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Manutenção criada: {maintenance.MaintenanceId}");

            return new ApiResponse<MaintenanceDto>
            {
                Success = true,
                Message = "Manutenção agendada com sucesso",
                Data = await GetMaintenanceByIdAsync(maintenance.MaintenanceId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar manutenção: {ex.Message}");
            return new ApiResponse<MaintenanceDto>
            {
                Success = false,
                Message = "Erro ao agendar manutenção"
            };
        }
    }

    public async Task<ApiResponse<bool>> CompleteMaintenanceAsync(Guid maintenanceId, string? notes)
    {
        try
        {
            var maintenance = await _context.Maintenances
                .Include(m => m.Tasks)
                .FirstOrDefaultAsync(m => m.MaintenanceId == maintenanceId);

            if (maintenance == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Manutenção não encontrada",
                    Data = false
                };
            }

            maintenance.Status = "completed";
            maintenance.CompletedDate = DateTime.UtcNow;
            maintenance.TechnicianNotes = notes;

            // Marcar todas as tarefas como concluídas
            foreach (var task in maintenance.Tasks)
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.UtcNow;
            }

            maintenance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Manutenção {maintenanceId} concluída");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Manutenção concluída com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao concluir manutenção: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao concluir manutenção",
                Data = false
            };
        }
    }

    private MaintenanceDto MapToMaintenanceDto(Maintenance maintenance)
    {
        return new MaintenanceDto
        {
            MaintenanceId = maintenance.MaintenanceId,
            Status = maintenance.Status,
            Type = maintenance.Type,
            Description = maintenance.Description,
            ScheduledDate = maintenance.ScheduledDate,
            CompletedDate = maintenance.CompletedDate,
            Cost = maintenance.Cost,
            Tasks = maintenance.Tasks.Select(t => new MaintenanceTaskDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted
            }).ToList()
        };
    }
}

public interface IBudgetService
{
    Task<BudgetDto?> GetBudgetByIdAsync(Guid budgetId);
    Task<List<BudgetDto>> GetCustomerBudgetsAsync(Guid customerId);
    Task<ApiResponse<BudgetDto>> CreateBudgetAsync(Guid customerId, CreateBudgetRequest request);
    Task<ApiResponse<bool>> AcceptBudgetAsync(Guid budgetId);
    Task<ApiResponse<bool>> RejectBudgetAsync(Guid budgetId);
}

public class BudgetService : IBudgetService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(AppDbContext context, ILogger<BudgetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BudgetDto?> GetBudgetByIdAsync(Guid budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

        return budget != null ? MapToBudgetDto(budget) : null;
    }

    public async Task<List<BudgetDto>> GetCustomerBudgetsAsync(Guid customerId)
    {
        var budgets = await _context.Budgets
            .Where(b => b.CustomerId == customerId)
            .Include(b => b.Items)
            .OrderByDescending(b => b.CreatedDate)
            .ToListAsync();

        return budgets.Select(MapToBudgetDto).ToList();
    }

    public async Task<ApiResponse<BudgetDto>> CreateBudgetAsync(Guid customerId, CreateBudgetRequest request)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<BudgetDto>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            decimal totalAmount = 0;
            var budget = new Budget
            {
                BudgetId = Guid.NewGuid(),
                BudgetNumber = GenerateBudgetNumber(),
                CustomerId = customerId,
                Status = "draft",
                Type = request.Type,
                Description = request.Description,
                DiscountAmount = request.DiscountAmount ?? 0,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var itemRequest in request.Items)
            {
                var itemTotal = itemRequest.Quantity * itemRequest.UnitPrice;
                totalAmount += itemTotal;

                var budgetItem = new BudgetItem
                {
                    BudgetItemId = Guid.NewGuid(),
                    BudgetId = budget.BudgetId,
                    Description = itemRequest.Description,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    TotalPrice = itemTotal
                };
                budget.Items.Add(budgetItem);
            }

            budget.TotalAmount = totalAmount;
            budget.FinalAmount = totalAmount - (budget.DiscountAmount ?? 0);

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Orçamento criado: {budget.BudgetNumber}");

            return new ApiResponse<BudgetDto>
            {
                Success = true,
                Message = "Orçamento criado com sucesso",
                Data = await GetBudgetByIdAsync(budget.BudgetId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar orçamento: {ex.Message}");
            return new ApiResponse<BudgetDto>
            {
                Success = false,
                Message = "Erro ao criar orçamento"
            };
        }
    }

    public async Task<ApiResponse<bool>> AcceptBudgetAsync(Guid budgetId)
    {
        try
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Orçamento não encontrado",
                    Data = false
                };
            }

            budget.Status = "accepted";
            budget.AcceptedDate = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Orçamento {budgetId} aceito");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Orçamento aceito com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao aceitar orçamento: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao aceitar orçamento",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> RejectBudgetAsync(Guid budgetId)
    {
        try
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Orçamento não encontrado",
                    Data = false
                };
            }

            budget.Status = "rejected";
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Orçamento {budgetId} rejeitado");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Orçamento rejeitado",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao rejeitar orçamento: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao rejeitar orçamento",
                Data = false
            };
        }
    }

    private BudgetDto MapToBudgetDto(Budget budget)
    {
        return new BudgetDto
        {
            BudgetId = budget.BudgetId,
            BudgetNumber = budget.BudgetNumber,
            Status = budget.Status,
            Type = budget.Type,
            Description = budget.Description,
            TotalAmount = budget.TotalAmount,
            DiscountAmount = budget.DiscountAmount,
            FinalAmount = budget.FinalAmount,
            CreatedDate = budget.CreatedDate,
            ExpiryDate = budget.ExpiryDate,
            Items = budget.Items.Select(i => new BudgetItemDto
            {
                BudgetItemId = i.BudgetItemId,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }

    private string GenerateBudgetNumber()
    {
        return $"BUD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}

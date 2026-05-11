using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILogger<BudgetController> _logger;

    public BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
    {
        _budgetService = budgetService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BudgetDto>>> GetBudgetById(Guid id)
    {
        var budget = await _budgetService.GetBudgetByIdAsync(id);
        if (budget == null)
            return NotFound(new ApiResponse<BudgetDto> 
            { 
                Success = false, 
                Message = "Orçamento não encontrado" 
            });

        return Ok(new ApiResponse<BudgetDto>
        {
            Success = true,
            Data = budget
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<BudgetDto>>> GetMyBudgets()
    {
        var customerId = GetCustomerId();
        var budgets = await _budgetService.GetCustomerBudgetsAsync(customerId);
        
        return Ok(budgets);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetDto>>> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = GetCustomerId();
        var result = await _budgetService.CreateBudgetAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetBudgetById), new { id = result.Data?.BudgetId }, result);
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptBudget(Guid id)
    {
        var result = await _budgetService.AcceptBudgetAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectBudget(Guid id)
    {
        var result = await _budgetService.RejectBudgetAsync(id);

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

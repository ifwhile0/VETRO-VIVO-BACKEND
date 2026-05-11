using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(IMaintenanceService maintenanceService, ILogger<MaintenanceController> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MaintenanceDto>>> GetMaintenanceById(Guid id)
    {
        var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);
        if (maintenance == null)
            return NotFound(new ApiResponse<MaintenanceDto> 
            { 
                Success = false, 
                Message = "Manutenção não encontrada" 
            });

        return Ok(new ApiResponse<MaintenanceDto>
        {
            Success = true,
            Data = maintenance
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceDto>>> GetMyMaintenances()
    {
        var customerId = GetCustomerId();
        var maintenances = await _maintenanceService.GetCustomerMaintenancesAsync(customerId);
        
        return Ok(maintenances);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaintenanceDto>>> CreateMaintenance([FromBody] CreateMaintenanceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = GetCustomerId();
        var result = await _maintenanceService.CreateMaintenanceAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMaintenanceById), new { id = result.Data?.MaintenanceId }, result);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteMaintenance(Guid id, [FromBody] dynamic completeData)
    {
        var notes = completeData.notes as string;
        var result = await _maintenanceService.CompleteMaintenanceAsync(id, notes);

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

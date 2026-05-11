using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetProfile()
    {
        var customerId = GetCustomerId();
        
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return NotFound(new ApiResponse<CustomerDto> 
            { 
                Success = false, 
                Message = "Cliente não encontrado" 
            });

        return Ok(new ApiResponse<CustomerDto>
        {
            Success = true,
            Message = "Perfil recuperado com sucesso",
            Data = customer
        });
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateProfile([FromBody] UpdateCustomerRequest request)
    {
        var customerId = GetCustomerId();
        var result = await _customerService.UpdateCustomerAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var customerId = GetCustomerId();
        var result = await _customerService.ChangePasswordAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<ApiResponse<AddressDto>>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var customerId = GetCustomerId();
        var result = await _customerService.CreateAddressAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAddresses), result);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses()
    {
        var customerId = GetCustomerId();
        var addresses = await _customerService.GetAddressesAsync(customerId);

        return Ok(addresses);
    }

    [HttpDelete("addresses/{addressId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(Guid addressId)
    {
        var customerId = GetCustomerId();
        var result = await _customerService.DeleteAddressAsync(customerId, addressId);

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

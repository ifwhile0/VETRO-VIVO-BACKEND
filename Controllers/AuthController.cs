using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetroVivo.API.DTOs;
using VetroVivo.API.Services;

namespace VetroVivo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly Guid _storeId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"); // ID padrão da loja

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request, _storeId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request, _storeId);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var sessionIdClaim = User.FindFirst("SessionId");
        if (sessionIdClaim == null)
            return BadRequest(new ApiResponse<bool> { Success = false, Message = "Sessão não encontrada" });

        var success = await _authService.LogoutAsync(Guid.Parse(sessionIdClaim.Value));
        
        if (!success)
            return BadRequest(new ApiResponse<bool> { Success = false, Message = "Erro ao fazer logout" });

        return Ok(new ApiResponse<bool> 
        { 
            Success = true, 
            Message = "Logout realizado com sucesso",
            Data = true
        });
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<ApiResponse<string>> GetCurrentUser()
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Usuário autenticado",
            Data = email
        });
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VetroVivo.API.DTOs;
using VetroVivo.API.Models;

namespace VetroVivo.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, Guid storeId);
    Task<AuthResponse> LoginAsync(LoginRequest request, Guid storeId);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(Guid sessionId);
    string GenerateAccessToken(Customer customer, Guid storeId);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, AppDbContext context, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, Guid storeId)
    {
        try
        {
            // Validações
            if (request.Password != request.ConfirmPassword)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "As senhas não correspondem"
                };
            }

            // Verificar se email já existe
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email && c.StoreId == storeId);

            if (existingCustomer != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Este email já está cadastrado"
                };
            }

            // Criar novo cliente
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                StoreId = storeId,
                Email = request.Email.ToLower().Trim(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                DisplayName = $"{request.FirstName} {request.LastName}".Trim(),
                Phone = request.Phone,
                CPF = request.CPF,
                Salt = salt,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, salt),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailVerified = false,
                ReferralCode = GenerateReferralCode()
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Novo cliente registrado: {customer.CustomerId}");

            var accessToken = GenerateAccessToken(customer, storeId);
            var refreshToken = GenerateRefreshToken();

            // Criar sessão
            await CreateSessionAsync(customer.CustomerId, storeId, refreshToken);

            return new AuthResponse
            {
                Success = true,
                Message = "Cadastro realizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Customer = MapToCustomerDto(customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro no registro: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = "Erro ao registrar cliente"
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, Guid storeId)
    {
        try
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email.ToLower() && c.StoreId == storeId);

            if (customer == null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            if (!customer.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Conta desativada"
                };
            }

            if (customer.IsBlocked)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Conta bloqueada: {customer.BlockReason}"
                };
            }

            customer.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(customer, storeId);
            var refreshToken = GenerateRefreshToken();

            await CreateSessionAsync(customer.CustomerId, storeId, refreshToken);

            _logger.LogInformation($"Cliente autenticado: {customer.CustomerId}");

            return new AuthResponse
            {
                Success = true,
                Message = "Login realizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Customer = MapToCustomerDto(customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro no login: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = "Erro ao fazer login"
            };
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var session = await _context.CustomerSessions
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.IsActive);

            if (session == null || (session.ExpiresAt.HasValue && session.ExpiresAt < DateTime.UtcNow))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token de refresh inválido ou expirado"
                };
            }

            var customer = await _context.Customers.FindAsync(session.CustomerId);
            if (customer == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            var newAccessToken = GenerateAccessToken(customer, session.StoreId);
            var newRefreshToken = GenerateRefreshToken();

            session.RefreshToken = newRefreshToken;
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Token renovado com sucesso",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Customer = MapToCustomerDto(customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao renovar token: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = "Erro ao renovar token"
            };
        }
    }

    public async Task<bool> LogoutAsync(Guid sessionId)
    {
        try
        {
            var session = await _context.CustomerSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao fazer logout: {ex.Message}");
            return false;
        }
    }

    public string GenerateAccessToken(Customer customer, Guid storeId)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key não configurada");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim("StoreId", storeId.ToString()),
            new Claim(ClaimTypes.GivenName, customer.FirstName ?? ""),
            new Claim(ClaimTypes.Surname, customer.LastName ?? "")
        };

        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key não configurada");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        return principal;
    }

    private async Task CreateSessionAsync(Guid customerId, Guid storeId, string refreshToken)
    {
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        var session = new CustomerSession
        {
            SessionId = Guid.NewGuid(),
            CustomerId = customerId,
            StoreId = storeId,
            TokenHash = tokenHash,
            RefreshToken = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
            IsActive = true,
            LastActivityAt = DateTime.UtcNow
        };

        _context.CustomerSessions.Add(session);
        await _context.SaveChangesAsync();
    }

    private string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 10)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }

    private CustomerDto MapToCustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            DisplayName = customer.DisplayName,
            Phone = customer.Phone,
            AvatarUrl = customer.AvatarUrl,
            EmailVerified = customer.EmailVerified,
            CreatedAt = customer.CreatedAt,
            LastLoginAt = customer.LastLoginAt
        };
    }
}

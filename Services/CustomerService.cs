using Microsoft.EntityFrameworkCore;
using VetroVivo.API.DTOs;
using VetroVivo.API.Models;

namespace VetroVivo.API.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId);
    Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid customerId, ChangePasswordRequest request);
    Task<ApiResponse<AddressDto>> CreateAddressAsync(Guid customerId, CreateAddressRequest request);
    Task<List<AddressDto>> GetAddressesAsync(Guid customerId);
    Task<ApiResponse<bool>> DeleteAddressAsync(Guid customerId, Guid addressId);
}

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(AppDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return null;

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

    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<CustomerDto>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            if (!string.IsNullOrEmpty(request.FirstName))
                customer.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                customer.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.DisplayName))
                customer.DisplayName = request.DisplayName;

            if (!string.IsNullOrEmpty(request.Phone))
                customer.Phone = request.Phone;

            if (request.BirthDate.HasValue)
                customer.BirthDate = request.BirthDate;

            if (!string.IsNullOrEmpty(request.Gender))
                customer.Gender = request.Gender;

            if (request.NewsletterOptIn.HasValue)
                customer.NewsletterOptIn = request.NewsletterOptIn.Value;

            if (request.SMSOptIn.HasValue)
                customer.SMSOptIn = request.SMSOptIn.Value;

            if (request.PushOptIn.HasValue)
                customer.PushOptIn = request.PushOptIn.Value;

            if (!string.IsNullOrEmpty(request.PreferredLanguage))
                customer.PreferredLanguage = request.PreferredLanguage;

            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cliente {customerId} atualizado com sucesso");

            return new ApiResponse<CustomerDto>
            {
                Success = true,
                Message = "Perfil atualizado com sucesso",
                Data = await GetCustomerByIdAsync(customerId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar cliente: {ex.Message}");
            return new ApiResponse<CustomerDto>
            {
                Success = false,
                Message = "Erro ao atualizar perfil"
            };
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid customerId, ChangePasswordRequest request)
    {
        try
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "As senhas não correspondem",
                    Data = false
                };
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cliente não encontrado",
                    Data = false
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, customer.PasswordHash))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Senha atual incorreta",
                    Data = false
                };
            }

            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, customer.Salt);
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Senha do cliente {customerId} alterada");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Senha alterada com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao alterar senha: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao alterar senha",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<AddressDto>> CreateAddressAsync(Guid customerId, CreateAddressRequest request)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new ApiResponse<AddressDto>
                {
                    Success = false,
                    Message = "Cliente não encontrado"
                };
            }

            // Se for padrão, desmarcar outras
            if (request.IsDefault)
            {
                var defaultAddresses = await _context.Addresses
                    .Where(a => a.CustomerId == customerId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in defaultAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                CustomerId = customerId,
                Type = request.Type,
                FullName = request.FullName,
                Street = request.Street,
                Number = request.Number,
                Complement = request.Complement,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return new ApiResponse<AddressDto>
            {
                Success = true,
                Message = "Endereço criado com sucesso",
                Data = MapToAddressDto(address)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar endereço: {ex.Message}");
            return new ApiResponse<AddressDto>
            {
                Success = false,
                Message = "Erro ao criar endereço"
            };
        }
    }

    public async Task<List<AddressDto>> GetAddressesAsync(Guid customerId)
    {
        var addresses = await _context.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();

        return addresses.Select(MapToAddressDto).ToList();
    }

    public async Task<ApiResponse<bool>> DeleteAddressAsync(Guid customerId, Guid addressId)
    {
        try
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId && a.CustomerId == customerId);

            if (address == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Endereço não encontrado",
                    Data = false
                };
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Endereço deletado com sucesso",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao deletar endereço: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erro ao deletar endereço",
                Data = false
            };
        }
    }

    private AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            AddressId = address.AddressId,
            Type = address.Type,
            FullName = address.FullName,
            Street = address.Street,
            Number = address.Number,
            Complement = address.Complement,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            IsDefault = address.IsDefault
        };
    }
}

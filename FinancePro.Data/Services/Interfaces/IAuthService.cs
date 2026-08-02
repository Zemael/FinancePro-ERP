using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> AutenticarAsync(string email, string password);
}

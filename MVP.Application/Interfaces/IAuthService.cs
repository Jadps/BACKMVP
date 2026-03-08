using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface IAuthService
{
    Task<ApplicationResult<AuthResponseDTO>> LoginAsync(LoginDTO model);
    Task<ApplicationResult<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO model);
    Task<ApplicationResult> LogoutAsync(string? userEmail);
    Task<ApplicationResult> ForgotPasswordAsync(ForgotPasswordDTO model);
    Task<ApplicationResult> ResetPasswordAsync(ResetPasswordDTO model);
}

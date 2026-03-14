using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface IAuthService
{
    Task<ApplicationResult<AuthResponseDto>> LoginAsync(LoginDto model);
    Task<ApplicationResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto model);
    Task<ApplicationResult> LogoutAsync(string? userEmail);
    Task<ApplicationResult> ForgotPasswordAsync(ForgotPasswordDto model);
    Task<ApplicationResult> ResetPasswordAsync(ResetPasswordDto model);
}

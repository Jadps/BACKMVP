using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO?> LoginAsync(LoginDTO model);
    Task<AuthResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO model);
    Task LogoutAsync(string? userEmail);
}

using System;

namespace MVP.Application.DTOs;

public class AuthResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}

public class RefreshTokenRequestDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordDTO
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDTO
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

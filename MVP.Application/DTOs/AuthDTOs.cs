using System;

namespace MVP.Application.DTOs;

public record AuthResponseDTO(string AccessToken, string RefreshToken, DateTime Expiration);

public record RefreshTokenRequestDTO(string RefreshToken);

public record ForgotPasswordDTO(string Email);

public record ResetPasswordDTO(string Email, string Token, string NewPassword);

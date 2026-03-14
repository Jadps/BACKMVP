using System;

namespace MVP.Application.DTOs;

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, string RefreshToken, DateTime Expiration);

public record RefreshTokenRequestDto(string RefreshToken);

public record ForgotPasswordDto(string Email);

public record ResetPasswordDto(string Email, string Token, string NewPassword);

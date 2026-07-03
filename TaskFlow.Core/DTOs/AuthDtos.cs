namespace TaskFlow.Core.DTOs;

public record RegisterDto(string FullName, string Email, string Password);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(int UserId, string FullName, string Email, string Token, DateTime ExpiresAt);

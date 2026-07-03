using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}

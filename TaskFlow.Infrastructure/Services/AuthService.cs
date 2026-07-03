using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Exceptions;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new BadRequestException("Email ve şifre zorunludur.");

        if (dto.Password.Length < 6)
            throw new BadRequestException("Şifre en az 6 karakter olmalıdır.");

        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new BadRequestException("Bu email adresi zaten kayıtlı.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            // Şifre asla düz metin olarak saklanmaz — BCrypt ile hash'lenir
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponseDto(user.Id, user.FullName, user.Email, token, expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Kullanıcı yoksa veya şifre yanlışsa aynı hatayı veriyoruz;
        // "email bulunamadı" gibi ayrıntı vermek güvenlik açığıdır (user enumeration)
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new BadRequestException("Email veya şifre hatalı.");

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponseDto(user.Id, user.FullName, user.Email, token, expiresAt);
    }
}

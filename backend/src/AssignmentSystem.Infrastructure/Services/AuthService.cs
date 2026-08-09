using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IPasswordHasher hasher, IJwtService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // Deliberately generic message — do not reveal whether the email exists.
        if (user is null || !user.IsActive || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (token, expiresAt) = _jwt.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }
}

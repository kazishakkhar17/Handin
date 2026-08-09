using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AssignmentSystem.Infrastructure.Services;

// Wraps ASP.NET Core Identity's battle-tested PBKDF2 hasher so we don't hand-roll crypto.
// A throwaway "User" instance is fine here — the hasher doesn't inspect it.
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string plainTextPassword)
        => _inner.HashPassword(new object(), plainTextPassword);

    public bool Verify(string plainTextPassword, string hash)
    {
        var result = _inner.VerifyHashedPassword(new object(), hash, plainTextPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

using AssignmentSystem.Core.Entities;

namespace AssignmentSystem.Core.Interfaces;

public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string hash);
}

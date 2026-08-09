using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AssignmentSystem.Tests;

public class AuthServiceTests
{
    private static AuthService BuildSut(AssignmentSystem.Infrastructure.Data.AppDbContext db)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-test-secret-test-secret-1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        var hasher = new PasswordHasher();
        var jwt = new JwtService(config);
        return new AuthService(db, hasher, jwt);
    }

    [Fact]
    public async Task LoginAsync_ShouldSucceed_WithCorrectCredentials()
    {
        var db = TestDbFactory.Create();
        var hasher = new PasswordHasher();
        db.Users.Add(new User
        {
            FullName = "Jane Doe",
            Email = "jane@school.test",
            PasswordHash = hasher.Hash("CorrectPass1"),
            Role = UserRole.Teacher,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = BuildSut(db);
        var result = await sut.LoginAsync(new LoginRequest { Email = "jane@school.test", Password = "CorrectPass1" });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be("jane@school.test");
        result.User.Role.Should().Be(UserRole.Teacher);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WithWrongPassword()
    {
        var db = TestDbFactory.Create();
        var hasher = new PasswordHasher();
        db.Users.Add(new User
        {
            FullName = "Jane Doe",
            Email = "jane@school.test",
            PasswordHash = hasher.Hash("CorrectPass1"),
            Role = UserRole.Teacher,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = BuildSut(db);
        var act = () => sut.LoginAsync(new LoginRequest { Email = "jane@school.test", Password = "WrongPassword" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserIsDeactivated()
    {
        var db = TestDbFactory.Create();
        var hasher = new PasswordHasher();
        db.Users.Add(new User
        {
            FullName = "Inactive Guy",
            Email = "inactive@school.test",
            PasswordHash = hasher.Hash("CorrectPass1"),
            Role = UserRole.Student,
            IsActive = false
        });
        await db.SaveChangesAsync();

        var sut = BuildSut(db);
        var act = () => sut.LoginAsync(new LoginRequest { Email = "inactive@school.test", Password = "CorrectPass1" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenEmailDoesNotExist()
    {
        var db = TestDbFactory.Create();
        var sut = BuildSut(db);

        var act = () => sut.LoginAsync(new LoginRequest { Email = "nobody@school.test", Password = "whatever" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

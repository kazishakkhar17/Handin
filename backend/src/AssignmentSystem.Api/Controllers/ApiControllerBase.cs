using System.Security.Claims;
using AssignmentSystem.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User id claim missing from token."));

    protected UserRole CurrentUserRole =>
        Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("Role claim missing from token."));
}

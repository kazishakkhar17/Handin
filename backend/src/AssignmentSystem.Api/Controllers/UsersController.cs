using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Teacher: list the subject/class combinations you are assigned to teach.</summary>
    [HttpGet("me/teaching-assignments")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<List<TeachingAssignmentResponse>>> GetMyTeachingAssignments()
    {
        return Ok(await _userService.GetMyTeachingAssignmentsAsync(CurrentUserId));
    }

    /// <summary>Admin: create a new Admin, Teacher, or Student account.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetAll), new { }, user);
    }

    /// <summary>Admin: list users, optionally filtered by role.</summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserResponse>>> GetAll([FromQuery] UserRole? role)
    {
        return Ok(await _userService.GetAllUsersAsync(role));
    }

    /// <summary>Admin: update a user's name or active status.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UpdateUserRequest request)
    {
        return Ok(await _userService.UpdateUserAsync(id, request));
    }

    /// <summary>Admin: deactivate a user (soft delete — preserves history/audit trail).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _userService.DeactivateUserAsync(id);
        return NoContent();
    }

    /// <summary>Admin: assign a teacher to teach a subject for a specific class/course.</summary>
    [HttpPost("assign-teacher")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AssignTeacher([FromBody] AssignTeacherRequest request)
    {
        await _userService.AssignTeacherToSubjectAsync(request);
        return NoContent();
    }
}

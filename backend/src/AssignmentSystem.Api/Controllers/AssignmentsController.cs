using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ApiControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService) => _assignmentService = assignmentService;

    /// <summary>Teacher: create an assignment (draft or published) for a subject/class they teach.</summary>
    [HttpPost]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<AssignmentResponse>> Create([FromBody] CreateAssignmentRequest request)
    {
        var result = await _assignmentService.CreateAssignmentAsync(CurrentUserId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Teacher: update their own assignment (title, deadline, marks, draft/publish status, etc.).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<AssignmentResponse>> Update(int id, [FromBody] UpdateAssignmentRequest request)
    {
        return Ok(await _assignmentService.UpdateAssignmentAsync(CurrentUserId, id, request));
    }

    /// <summary>Teacher: delete their own assignment.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        await _assignmentService.DeleteAssignmentAsync(CurrentUserId, id);
        return NoContent();
    }

    /// <summary>Student: view all published assignments for their enrolled class/course.</summary>
    [HttpGet("my")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<List<AssignmentResponse>>> GetMyAssignments()
    {
        return Ok(await _assignmentService.GetAssignmentsForStudentAsync(CurrentUserId));
    }

    /// <summary>Teacher: view all assignments they have created (draft and published).</summary>
    [HttpGet("teaching")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<List<AssignmentResponse>>> GetTeachingAssignments()
    {
        return Ok(await _assignmentService.GetAssignmentsForTeacherAsync(CurrentUserId));
    }

    /// <summary>Get a single assignment's details, enforcing role-based visibility rules.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id)
    {
        return Ok(await _assignmentService.GetAssignmentByIdAsync(id, CurrentUserId, CurrentUserRole));
    }
}

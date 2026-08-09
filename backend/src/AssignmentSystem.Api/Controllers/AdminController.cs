using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ApiControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISubmissionService _submissionService;

    public AdminController(IAssignmentService assignmentService, ISubmissionService submissionService)
    {
        _assignmentService = assignmentService;
        _submissionService = submissionService;
    }

    /// <summary>Admin: view every assignment across all teachers, subjects, and classes.</summary>
    [HttpGet("assignments")]
    public async Task<ActionResult<List<AssignmentResponse>>> GetAllAssignments()
    {
        return Ok(await _assignmentService.GetAllAssignmentsAsync());
    }

    /// <summary>Admin: view every submission across all assignments.</summary>
    [HttpGet("submissions")]
    public async Task<ActionResult<List<SubmissionResponse>>> GetAllSubmissions()
    {
        return Ok(await _submissionService.GetAllSubmissionsAsync());
    }
}

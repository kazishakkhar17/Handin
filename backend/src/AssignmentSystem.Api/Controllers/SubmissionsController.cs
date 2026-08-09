using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Route("api/assignments/{assignmentId:int}/submissions")]
[Authorize]
public class SubmissionsController : ApiControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService) => _submissionService = submissionService;

    /// <summary>Student: submit an answer for a published assignment before its deadline.</summary>
    [HttpPost]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<SubmissionResponse>> Submit(int assignmentId, [FromBody] CreateSubmissionRequest request)
    {
        var result = await _submissionService.SubmitAsync(CurrentUserId, assignmentId, request);
        return CreatedAtAction(nameof(GetMine), new { assignmentId }, result);
    }

    /// <summary>Student: update their own submission before the deadline (if the teacher allows resubmission).</summary>
    [HttpPut]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<SubmissionResponse>> UpdateMine(int assignmentId, [FromBody] CreateSubmissionRequest request)
    {
        return Ok(await _submissionService.UpdateSubmissionAsync(CurrentUserId, assignmentId, request));
    }

    /// <summary>Student: view their own submission status, marks, and feedback for an assignment.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = "StudentOnly")]
    public async Task<ActionResult<SubmissionResponse?>> GetMine(int assignmentId)
    {
        var result = await _submissionService.GetMySubmissionAsync(CurrentUserId, assignmentId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Teacher: view all student submissions for one of their assignments.</summary>
    [HttpGet]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<List<SubmissionResponse>>> GetAll(int assignmentId)
    {
        return Ok(await _submissionService.GetSubmissionsForAssignmentAsync(CurrentUserId, assignmentId));
    }

    /// <summary>Teacher: assign marks and feedback to a student's submission.</summary>
    [HttpPost("{submissionId:int}/grade")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<SubmissionResponse>> Grade(int assignmentId, int submissionId, [FromBody] GradeSubmissionRequest request)
    {
        return Ok(await _submissionService.GradeSubmissionAsync(CurrentUserId, submissionId, request));
    }

    /// <summary>Teacher: change a submission's status (e.g. return for revision).</summary>
    [HttpPatch("{submissionId:int}/status")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult<SubmissionResponse>> UpdateStatus(int assignmentId, int submissionId, [FromBody] UpdateSubmissionStatusRequest request)
    {
        return Ok(await _submissionService.UpdateStatusAsync(CurrentUserId, submissionId, request));
    }
}

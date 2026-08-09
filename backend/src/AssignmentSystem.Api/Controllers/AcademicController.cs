using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Route("api/classes")]
[Authorize]
public class ClassesController : ApiControllerBase
{
    private readonly IAcademicService _academicService;

    public ClassesController(IAcademicService academicService) => _academicService = academicService;

    /// <summary>Admin: create a class/course.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ClassCourseResponse>> Create([FromBody] CreateClassCourseRequest request)
    {
        var result = await _academicService.CreateClassCourseAsync(request);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    /// <summary>Any authenticated user: list all classes/courses.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ClassCourseResponse>>> GetAll()
    {
        return Ok(await _academicService.GetAllClassCoursesAsync());
    }
}

[Route("api/subjects")]
[Authorize]
public class SubjectsController : ApiControllerBase
{
    private readonly IAcademicService _academicService;

    public SubjectsController(IAcademicService academicService) => _academicService = academicService;

    /// <summary>Admin: create a subject under a class/course.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SubjectResponse>> Create([FromBody] CreateSubjectRequest request)
    {
        var result = await _academicService.CreateSubjectAsync(request);
        return CreatedAtAction(nameof(GetByClass), new { classCourseId = result.ClassCourseId }, result);
    }

    /// <summary>Any authenticated user: list subjects for a given class/course.</summary>
    [HttpGet]
    public async Task<ActionResult<List<SubjectResponse>>> GetByClass([FromQuery] int classCourseId)
    {
        return Ok(await _academicService.GetSubjectsByClassAsync(classCourseId));
    }
}

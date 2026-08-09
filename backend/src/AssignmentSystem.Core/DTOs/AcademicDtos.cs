using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.DTOs;

public class CreateClassCourseRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ClassCourseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateSubjectRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    [Required]
    public int ClassCourseId { get; set; }
}

public class SubjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int ClassCourseId { get; set; }
    public string ClassCourseName { get; set; } = string.Empty;
}

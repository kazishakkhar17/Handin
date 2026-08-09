using System.ComponentModel.DataAnnotations;
using AssignmentSystem.Core.Entities;

namespace AssignmentSystem.Core.DTOs;

public class CreateUserRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    // Required when Role == Student: which class/course to enroll in
    public int? ClassCourseId { get; set; }
}

public class UpdateUserRequest
{
    [MaxLength(150)]
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
}

public class TeachingAssignmentResponse
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ClassCourseId { get; set; }
    public string ClassCourseName { get; set; } = string.Empty;
}

public class AssignTeacherRequest
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassCourseId { get; set; }
}

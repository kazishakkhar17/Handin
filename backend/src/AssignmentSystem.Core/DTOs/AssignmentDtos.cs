using System.ComponentModel.DataAnnotations;
using AssignmentSystem.Core.Entities;

namespace AssignmentSystem.Core.DTOs;

public class CreateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassCourseId { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Range(1, 1000)]
    public int MaxMarks { get; set; }

    public bool AllowResubmission { get; set; } = true;

    // Draft or Published — teachers choose at creation time
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
}

public class UpdateAssignmentRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public int? MaxMarks { get; set; }
    public bool? AllowResubmission { get; set; }
    public AssignmentStatus? Status { get; set; }
}

public class AssignmentResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ClassCourseId { get; set; }
    public string ClassCourseName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; }
    public bool AllowResubmission { get; set; }
    public DateTime CreatedAt { get; set; }

    // Populated only for the requesting student, if they've submitted
    public string? MySubmissionStatus { get; set; }
}

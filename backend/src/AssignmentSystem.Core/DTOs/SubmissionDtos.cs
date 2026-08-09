using System.ComponentModel.DataAnnotations;
using AssignmentSystem.Core.Entities;

namespace AssignmentSystem.Core.DTOs;

public class CreateSubmissionRequest
{
    [Required]
    public string AnswerText { get; set; } = string.Empty;

    public string? AttachmentUrl { get; set; }
}

public class GradeSubmissionRequest
{
    [Required, Range(0, 1000)]
    public int MarksAwarded { get; set; }

    public string? Feedback { get; set; }
}

public class UpdateSubmissionStatusRequest
{
    [Required]
    public SubmissionStatus Status { get; set; }
}

public class SubmissionResponse
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public int? MarksAwarded { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
}

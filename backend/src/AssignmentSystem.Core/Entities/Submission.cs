using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.Entities;

public class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    [Required]
    public string AnswerText { get; set; } = string.Empty;

    // Optional link to an uploaded file (stored path/URL); file upload itself is an optional extension.
    public string? AttachmentUrl { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    [Range(0, 1000)]
    public int? MarksAwarded { get; set; }

    public string? Feedback { get; set; }

    public int? GradedByTeacherId { get; set; }
    public DateTime? GradedAt { get; set; }
}

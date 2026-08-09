using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.Entities;

public class Assignment
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    [Required]
    public DateTime Deadline { get; set; }

    [Range(1, 1000)]
    public int MaxMarks { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    // Whether students may update a submission before the deadline
    public bool AllowResubmission { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

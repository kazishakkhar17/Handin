using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation: if Role == Teacher
    public ICollection<TeacherSubjectClass> TeachingAssignments { get; set; } = new List<TeacherSubjectClass>();

    // Navigation: if Role == Student
    public StudentEnrollment? Enrollment { get; set; }

    // Navigation: assignments created by this user (if teacher)
    public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();

    // Navigation: submissions made by this user (if student)
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

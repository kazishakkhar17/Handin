using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.Entities;

// Represents a "class" (school) or "course" (college) — e.g. "Grade 9-A" or "BSc CSE - 3rd Semester"
public class ClassCourse
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public ICollection<TeacherSubjectClass> TeacherAssignments { get; set; } = new List<TeacherSubjectClass>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

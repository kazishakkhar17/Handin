using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Core.Entities;

public class Subject
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Code { get; set; }

    // A subject belongs to one class/course in this simple model
    // (e.g. "Physics" for "Grade 9-A" is a distinct row from "Physics" for "Grade 10-A")
    public int ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public ICollection<TeacherSubjectClass> TeacherAssignments { get; set; } = new List<TeacherSubjectClass>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

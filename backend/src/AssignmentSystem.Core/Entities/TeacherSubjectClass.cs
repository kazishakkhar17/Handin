namespace AssignmentSystem.Core.Entities;

// Admin-managed link: which teacher teaches which subject in which class/course.
public class TeacherSubjectClass
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

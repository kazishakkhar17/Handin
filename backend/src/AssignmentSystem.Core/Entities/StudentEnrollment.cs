namespace AssignmentSystem.Core.Entities;

// A student belongs to exactly one class/course in this simplified model.
// (Documented assumption — see README "Assumptions" section.)
public class StudentEnrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}

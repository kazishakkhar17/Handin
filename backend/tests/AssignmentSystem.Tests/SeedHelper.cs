using AssignmentSystem.Core.Entities;
using AssignmentSystem.Infrastructure.Data;

namespace AssignmentSystem.Tests;

public class Fixture
{
    public int TeacherId;
    public int OtherTeacherId;
    public int StudentId;
    public int OtherStudentId;
    public int ClassCourseId;
    public int SubjectId;
}

public static class SeedHelper
{
    public static Fixture SeedStandardFixture(AppDbContext db)
    {
        var teacher = new User { FullName = "Teacher One", Email = "t1@test.com", PasswordHash = "x", Role = UserRole.Teacher };
        var otherTeacher = new User { FullName = "Teacher Two", Email = "t2@test.com", PasswordHash = "x", Role = UserRole.Teacher };
        var student = new User { FullName = "Student One", Email = "s1@test.com", PasswordHash = "x", Role = UserRole.Student };
        var otherStudent = new User { FullName = "Student Two", Email = "s2@test.com", PasswordHash = "x", Role = UserRole.Student };

        db.Users.AddRange(teacher, otherTeacher, student, otherStudent);
        db.SaveChanges();

        var classCourse = new ClassCourse { Name = "Grade 9-A" };
        db.ClassCourses.Add(classCourse);
        db.SaveChanges();

        var subject = new Subject { Name = "Physics", ClassCourseId = classCourse.Id };
        db.Subjects.Add(subject);
        db.SaveChanges();

        db.TeacherSubjectClasses.Add(new TeacherSubjectClass
        {
            TeacherId = teacher.Id,
            SubjectId = subject.Id,
            ClassCourseId = classCourse.Id
        });

        db.StudentEnrollments.Add(new StudentEnrollment
        {
            StudentId = student.Id,
            ClassCourseId = classCourse.Id
        });

        db.SaveChanges();

        return new Fixture
        {
            TeacherId = teacher.Id,
            OtherTeacherId = otherTeacher.Id,
            StudentId = student.Id,
            OtherStudentId = otherStudent.Id,
            ClassCourseId = classCourse.Id,
            SubjectId = subject.Id
        };
    }

    public static Assignment CreateAssignment(
        AppDbContext db,
        Fixture fixture,
        DateTime deadline,
        bool allowResubmission = true,
        AssignmentStatus status = AssignmentStatus.Published,
        int maxMarks = 100)
    {
        var assignment = new Assignment
        {
            Title = "Test Assignment",
            Description = "Description",
            SubjectId = fixture.SubjectId,
            ClassCourseId = fixture.ClassCourseId,
            TeacherId = fixture.TeacherId,
            Deadline = deadline,
            MaxMarks = maxMarks,
            AllowResubmission = allowResubmission,
            Status = status
        };

        db.Assignments.Add(assignment);
        db.SaveChanges();
        return assignment;
    }
}

using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Data;

// Seeds demo accounts (Admin / Teacher / Student) plus a bit of sample data so
// the evaluator can log in and see the app working immediately after setup.
public static class DbSeeder
{
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
            return; // already seeded

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@school.test",
            PasswordHash = hasher.Hash(DemoPassword),
            Role = UserRole.Admin
        };

        var teacher = new User
        {
            FullName = "Fahim Rahman",
            Email = "teacher@school.test",
            PasswordHash = hasher.Hash(DemoPassword),
            Role = UserRole.Teacher
        };

        var student = new User
        {
            FullName = "Ayesha Karim",
            Email = "student@school.test",
            PasswordHash = hasher.Hash(DemoPassword),
            Role = UserRole.Student
        };

        db.Users.AddRange(admin, teacher, student);
        await db.SaveChangesAsync();

        var classCourse = new ClassCourse { Name = "Grade 9 - Section A", Description = "Seeded demo class" };
        db.ClassCourses.Add(classCourse);
        await db.SaveChangesAsync();

        var subject = new Subject { Name = "Physics", Code = "PHY-101", ClassCourseId = classCourse.Id };
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();

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

        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            Title = "Newton's Laws of Motion — Problem Set 1",
            Description = "Solve problems 1-10 from chapter 4 and show your working.",
            SubjectId = subject.Id,
            ClassCourseId = classCourse.Id,
            TeacherId = teacher.Id,
            Deadline = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            AllowResubmission = true,
            Status = AssignmentStatus.Published
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
    }
}

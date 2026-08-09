using AssignmentSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClassCourse> ClassCourses => Set<ClassCourse>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubjectClass> TeacherSubjectClasses => Set<TeacherSubjectClass>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- User ----
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        // ---- ClassCourse / Subject ----
        modelBuilder.Entity<Subject>(e =>
        {
            e.HasOne(s => s.ClassCourse)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.ClassCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // A subject name must be unique within a given class/course
            e.HasIndex(s => new { s.ClassCourseId, s.Name }).IsUnique();
        });

        // ---- TeacherSubjectClass (many-to-many-ish join) ----
        modelBuilder.Entity<TeacherSubjectClass>(e =>
        {
            e.HasOne(t => t.Teacher)
                .WithMany(u => u.TeachingAssignments)
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Subject)
                .WithMany(s => s.TeacherAssignments)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.ClassCourse)
                .WithMany(c => c.TeacherAssignments)
                .HasForeignKey(t => t.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // A teacher can only be assigned once to the same subject+class
            e.HasIndex(t => new { t.TeacherId, t.SubjectId, t.ClassCourseId }).IsUnique();
        });

        // ---- StudentEnrollment (one student -> one class/course) ----
        modelBuilder.Entity<StudentEnrollment>(e =>
        {
            e.HasOne(se => se.Student)
                .WithOne(u => u.Enrollment)
                .HasForeignKey<StudentEnrollment>(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(se => se.ClassCourse)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(se => se.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(se => se.StudentId).IsUnique();
        });

        // ---- Assignment ----
        modelBuilder.Entity<Assignment>(e =>
        {
            e.HasOne(a => a.Subject)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.ClassCourse)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Teacher)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(a => a.Status).HasConversion<string>();
        });

        // ---- Submission ----
        modelBuilder.Entity<Submission>(e =>
        {
            e.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(s => s.Status).HasConversion<string>();

            // One submission per student per assignment (resubmission = update, not a new row)
            e.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
        });
    }
}

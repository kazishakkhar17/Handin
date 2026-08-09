using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUserAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var db = TestDbFactory.Create();
        var sut = new UserService(db, new PasswordHasher());

        await sut.CreateUserAsync(new CreateUserRequest
        {
            FullName = "First",
            Email = "dup@school.test",
            Password = "Password1",
            Role = UserRole.Teacher
        });

        var act = () => sut.CreateUserAsync(new CreateUserRequest
        {
            FullName = "Second",
            Email = "dup@school.test",
            Password = "Password1",
            Role = UserRole.Teacher
        });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrow_WhenStudentHasNoClassCourse()
    {
        var db = TestDbFactory.Create();
        var sut = new UserService(db, new PasswordHasher());

        var act = () => sut.CreateUserAsync(new CreateUserRequest
        {
            FullName = "New Student",
            Email = "newstudent@school.test",
            Password = "Password1",
            Role = UserRole.Student,
            ClassCourseId = null
        });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*class/course*");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldEnrollStudent_WhenClassCourseProvided()
    {
        var db = TestDbFactory.Create();
        var classCourse = new ClassCourse { Name = "Grade 8" };
        db.ClassCourses.Add(classCourse);
        await db.SaveChangesAsync();

        var sut = new UserService(db, new PasswordHasher());

        var result = await sut.CreateUserAsync(new CreateUserRequest
        {
            FullName = "New Student",
            Email = "newstudent2@school.test",
            Password = "Password1",
            Role = UserRole.Student,
            ClassCourseId = classCourse.Id
        });

        var enrollment = db.StudentEnrollments.FirstOrDefault(e => e.StudentId == result.Id);
        enrollment.Should().NotBeNull();
        enrollment!.ClassCourseId.Should().Be(classCourse.Id);
    }

    [Fact]
    public async Task AssignTeacherToSubjectAsync_ShouldThrow_OnDuplicateAssignment()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var sut = new UserService(db, new PasswordHasher());

        var request = new AssignTeacherRequest
        {
            TeacherId = fixture.TeacherId,
            SubjectId = fixture.SubjectId,
            ClassCourseId = fixture.ClassCourseId
        };

        // Fixture already assigns TeacherId to SubjectId/ClassCourseId — this should be a duplicate.
        var act = () => sut.AssignTeacherToSubjectAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already assigned*");
    }
}

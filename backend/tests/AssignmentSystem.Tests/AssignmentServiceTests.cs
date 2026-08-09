using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Tests;

public class AssignmentServiceTests
{
    [Fact]
    public async Task CreateAssignmentAsync_ShouldThrow_WhenTeacherNotAssignedToSubjectClass()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var sut = new AssignmentService(db);

        var request = new CreateAssignmentRequest
        {
            Title = "Unauthorized Attempt",
            Description = "desc",
            SubjectId = fixture.SubjectId,
            ClassCourseId = fixture.ClassCourseId,
            Deadline = DateTime.UtcNow.AddDays(3),
            MaxMarks = 100
        };

        // OtherTeacherId is not assigned to this subject/class combination.
        var act = () => sut.CreateAssignmentAsync(fixture.OtherTeacherId, request);

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldThrow_WhenDeadlineIsInThePast()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var sut = new AssignmentService(db);

        var request = new CreateAssignmentRequest
        {
            Title = "Past Deadline",
            Description = "desc",
            SubjectId = fixture.SubjectId,
            ClassCourseId = fixture.ClassCourseId,
            Deadline = DateTime.UtcNow.AddDays(-1),
            MaxMarks = 100
        };

        var act = () => sut.CreateAssignmentAsync(fixture.TeacherId, request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*future*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldSucceed_ForAssignedTeacher()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var sut = new AssignmentService(db);

        var request = new CreateAssignmentRequest
        {
            Title = "Valid Assignment",
            Description = "desc",
            SubjectId = fixture.SubjectId,
            ClassCourseId = fixture.ClassCourseId,
            Deadline = DateTime.UtcNow.AddDays(3),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };

        var result = await sut.CreateAssignmentAsync(fixture.TeacherId, request);

        result.Title.Should().Be("Valid Assignment");
        result.Status.Should().Be(AssignmentStatus.Published);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldThrow_WhenNotOwner()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(3));
        var sut = new AssignmentService(db);

        var act = () => sut.UpdateAssignmentAsync(fixture.OtherTeacherId, assignment.Id, new UpdateAssignmentRequest { Title = "Hijacked" });

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldThrow_WhenNotOwner()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(3));
        var sut = new AssignmentService(db);

        var act = () => sut.DeleteAssignmentAsync(fixture.OtherTeacherId, assignment.Id);

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task GetAssignmentsForStudentAsync_ShouldOnlyReturnPublishedAssignmentsInTheirClass()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), status: AssignmentStatus.Published);
        SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), status: AssignmentStatus.Draft);
        var sut = new AssignmentService(db);

        var result = await sut.GetAssignmentsForStudentAsync(fixture.StudentId);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(AssignmentStatus.Published);
    }

    [Fact]
    public async Task GetAssignmentsForStudentAsync_ShouldThrow_WhenStudentNotEnrolledAnywhere()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var sut = new AssignmentService(db);

        // OtherStudentId has no enrollment at all in the fixture.
        var act = () => sut.GetAssignmentsForStudentAsync(fixture.OtherStudentId);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}

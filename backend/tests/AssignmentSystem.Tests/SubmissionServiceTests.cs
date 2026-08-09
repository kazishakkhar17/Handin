using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Tests;

public class SubmissionServiceTests
{
    [Fact]
    public async Task SubmitAsync_ShouldSucceed_WhenPublishedAndBeforeDeadline()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1));
        var sut = new SubmissionService(db);

        var result = await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "My answer" });

        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.AnswerText.Should().Be("My answer");
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenDeadlineHasPassed()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddMinutes(-10));
        var sut = new SubmissionService(db);

        var act = () => sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Late answer" });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*deadline*");
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenStudentNotEnrolledInAssignmentClass()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1));
        var sut = new SubmissionService(db);

        // OtherStudentId was never enrolled in fixture.ClassCourseId
        var act = () => sut.SubmitAsync(fixture.OtherStudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenAssignmentIsUnpublishedDraft()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), status: AssignmentStatus.Draft);
        var sut = new SubmissionService(db);

        var act = () => sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_OnDuplicateSubmission()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1));
        var sut = new SubmissionService(db);

        await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "First" });
        var act = () => sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Second" });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already submitted*");
    }

    [Fact]
    public async Task UpdateSubmissionAsync_ShouldThrow_WhenResubmissionNotAllowed()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), allowResubmission: false);
        var sut = new SubmissionService(db);
        await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Original" });

        var act = () => sut.UpdateSubmissionAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Changed" });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Resubmission*");
    }

    [Fact]
    public async Task UpdateSubmissionAsync_ShouldThrow_AfterDeadline()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        // Deadline far enough in the future to submit, but we'll simulate lateness by using a past deadline
        // for the update check directly: create with a deadline slightly ahead, submit, then re-fetch and
        // move the deadline to the past to simulate time passing.
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddMinutes(5));
        var sut = new SubmissionService(db);
        await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Original" });

        assignment.Deadline = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var act = () => sut.UpdateSubmissionAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Changed" });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*deadline*");
    }

    [Fact]
    public async Task GradeSubmissionAsync_ShouldThrow_WhenMarksExceedMaxMarks()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), maxMarks: 50);
        var sut = new SubmissionService(db);
        var submission = await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });

        var act = () => sut.GradeSubmissionAsync(fixture.TeacherId, submission.Id, new GradeSubmissionRequest { MarksAwarded = 75 });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*cannot exceed*");
    }

    [Fact]
    public async Task GradeSubmissionAsync_ShouldThrow_WhenTeacherDoesNotOwnAssignment()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1));
        var sut = new SubmissionService(db);
        var submission = await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });

        var act = () => sut.GradeSubmissionAsync(fixture.OtherTeacherId, submission.Id, new GradeSubmissionRequest { MarksAwarded = 10 });

        await act.Should().ThrowAsync<ForbiddenActionException>();
    }

    [Fact]
    public async Task GradeSubmissionAsync_ShouldSucceed_AndMarkStatusGraded()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1), maxMarks: 100);
        var sut = new SubmissionService(db);
        var submission = await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });

        var graded = await sut.GradeSubmissionAsync(fixture.TeacherId, submission.Id, new GradeSubmissionRequest { MarksAwarded = 85, Feedback = "Well done" });

        graded.Status.Should().Be(SubmissionStatus.Graded);
        graded.MarksAwarded.Should().Be(85);
        graded.Feedback.Should().Be("Well done");
    }

    [Fact]
    public async Task UpdateSubmissionAsync_ShouldThrow_WhenAlreadyGraded()
    {
        var db = TestDbFactory.Create();
        var fixture = SeedHelper.SeedStandardFixture(db);
        var assignment = SeedHelper.CreateAssignment(db, fixture, DateTime.UtcNow.AddDays(1));
        var sut = new SubmissionService(db);
        var submission = await sut.SubmitAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Answer" });
        await sut.GradeSubmissionAsync(fixture.TeacherId, submission.Id, new GradeSubmissionRequest { MarksAwarded = 90 });

        var act = () => sut.UpdateSubmissionAsync(fixture.StudentId, assignment.Id, new CreateSubmissionRequest { AnswerText = "Trying to change after grading" });

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already been graded*");
    }
}

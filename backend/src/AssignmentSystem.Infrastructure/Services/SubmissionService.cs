using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Services;

public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _db;

    public SubmissionService(AppDbContext db) => _db = db;

    public async Task<SubmissionResponse> SubmitAsync(int studentId, int assignmentId, CreateSubmissionRequest request)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        await EnsureStudentCanAccessAssignmentAsync(studentId, assignment);

        var existing = await _db.Submissions.FirstOrDefaultAsync(s =>
            s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (existing is not null)
            throw new BusinessRuleException("You have already submitted this assignment. Use update instead.");

        var now = DateTime.UtcNow;
        if (now > assignment.Deadline)
            throw new BusinessRuleException("The deadline for this assignment has passed.");

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = request.AnswerText,
            AttachmentUrl = request.AttachmentUrl,
            SubmittedAt = now,
            Status = SubmissionStatus.Submitted
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();

        return await ToResponseAsync(submission);
    }

    public async Task<SubmissionResponse> UpdateSubmissionAsync(int studentId, int assignmentId, CreateSubmissionRequest request)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        await EnsureStudentCanAccessAssignmentAsync(studentId, assignment);

        var submission = await _db.Submissions.FirstOrDefaultAsync(s =>
            s.AssignmentId == assignmentId && s.StudentId == studentId)
            ?? throw new NotFoundException("No existing submission found to update.");

        // Business rule: updates are only allowed before the deadline, and only if the
        // teacher has enabled resubmission for this assignment.
        if (DateTime.UtcNow > assignment.Deadline)
            throw new BusinessRuleException("The deadline has passed; this submission can no longer be updated.");

        if (!assignment.AllowResubmission)
            throw new BusinessRuleException("Resubmission is not allowed for this assignment.");

        if (submission.Status == SubmissionStatus.Graded)
            throw new BusinessRuleException("This submission has already been graded and cannot be updated.");

        submission.AnswerText = request.AnswerText;
        submission.AttachmentUrl = request.AttachmentUrl;
        submission.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await ToResponseAsync(submission);
    }

    public async Task<List<SubmissionResponse>> GetSubmissionsForAssignmentAsync(int teacherId, int assignmentId)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenActionException("You may only view submissions for your own assignments.");

        var submissions = await _db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync();

        return submissions.Select(MapToResponse).ToList();
    }

    public async Task<SubmissionResponse> GradeSubmissionAsync(int teacherId, int submissionId, GradeSubmissionRequest request)
    {
        var submission = await _db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new NotFoundException("Submission not found.");

        if (submission.Assignment.TeacherId != teacherId)
            throw new ForbiddenActionException("You may only grade submissions for your own assignments.");

        if (request.MarksAwarded > submission.Assignment.MaxMarks)
            throw new BusinessRuleException($"Marks awarded cannot exceed the maximum of {submission.Assignment.MaxMarks}.");

        submission.MarksAwarded = request.MarksAwarded;
        submission.Feedback = request.Feedback;
        submission.Status = SubmissionStatus.Graded;
        submission.GradedByTeacherId = teacherId;
        submission.GradedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(submission);
    }

    public async Task<SubmissionResponse> UpdateStatusAsync(int teacherId, int submissionId, UpdateSubmissionStatusRequest request)
    {
        var submission = await _db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new NotFoundException("Submission not found.");

        if (submission.Assignment.TeacherId != teacherId)
            throw new ForbiddenActionException("You may only update submissions for your own assignments.");

        submission.Status = request.Status;
        await _db.SaveChangesAsync();

        return MapToResponse(submission);
    }

    public async Task<SubmissionResponse?> GetMySubmissionAsync(int studentId, int assignmentId)
    {
        var submission = await _db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        return submission is null ? null : MapToResponse(submission);
    }

    public async Task<List<SubmissionResponse>> GetAllSubmissionsAsync()
    {
        var submissions = await _db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return submissions.Select(MapToResponse).ToList();
    }

    private async Task EnsureStudentCanAccessAssignmentAsync(int studentId, Assignment assignment)
    {
        if (assignment.Status != AssignmentStatus.Published)
            throw new ForbiddenActionException("This assignment is not published.");

        var enrolled = await _db.StudentEnrollments.AnyAsync(e =>
            e.StudentId == studentId && e.ClassCourseId == assignment.ClassCourseId);

        if (!enrolled)
            throw new ForbiddenActionException("You are not enrolled in the class/course this assignment belongs to.");
    }

    private async Task<SubmissionResponse> ToResponseAsync(Submission submission)
    {
        await _db.Entry(submission).Reference(s => s.Student).LoadAsync();
        await _db.Entry(submission).Reference(s => s.Assignment).LoadAsync();
        return MapToResponse(submission);
    }

    private static SubmissionResponse MapToResponse(Submission s) => new()
    {
        Id = s.Id,
        AssignmentId = s.AssignmentId,
        AssignmentTitle = s.Assignment?.Title ?? string.Empty,
        StudentId = s.StudentId,
        StudentName = s.Student?.FullName ?? string.Empty,
        AnswerText = s.AnswerText,
        AttachmentUrl = s.AttachmentUrl,
        SubmittedAt = s.SubmittedAt,
        LastUpdatedAt = s.LastUpdatedAt,
        Status = s.Status,
        MarksAwarded = s.MarksAwarded,
        Feedback = s.Feedback,
        GradedAt = s.GradedAt
    };
}

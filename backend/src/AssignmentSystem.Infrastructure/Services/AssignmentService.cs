using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _db;

    public AssignmentService(AppDbContext db) => _db = db;

    public async Task<AssignmentResponse> CreateAssignmentAsync(int teacherId, CreateAssignmentRequest request)
    {
        // Business rule: a teacher may only create assignments for a subject/class they are actually assigned to.
        var isAssigned = await _db.TeacherSubjectClasses.AnyAsync(t =>
            t.TeacherId == teacherId &&
            t.SubjectId == request.SubjectId &&
            t.ClassCourseId == request.ClassCourseId);

        if (!isAssigned)
            throw new ForbiddenActionException("You are not assigned to teach this subject for this class/course.");

        if (request.Deadline <= DateTime.UtcNow)
            throw new BusinessRuleException("Deadline must be in the future.");

        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            SubjectId = request.SubjectId,
            ClassCourseId = request.ClassCourseId,
            TeacherId = teacherId,
            Deadline = request.Deadline,
            MaxMarks = request.MaxMarks,
            AllowResubmission = request.AllowResubmission,
            Status = request.Status
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        return await ToResponseAsync(assignment, requestingStudentId: null);
    }

    public async Task<AssignmentResponse> UpdateAssignmentAsync(int teacherId, int assignmentId, UpdateAssignmentRequest request)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenActionException("You may only modify your own assignments.");

        if (request.Title is not null) assignment.Title = request.Title;
        if (request.Description is not null) assignment.Description = request.Description;
        if (request.Deadline is not null) assignment.Deadline = request.Deadline.Value;
        if (request.MaxMarks is not null) assignment.MaxMarks = request.MaxMarks.Value;
        if (request.AllowResubmission is not null) assignment.AllowResubmission = request.AllowResubmission.Value;
        if (request.Status is not null) assignment.Status = request.Status.Value;

        assignment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await ToResponseAsync(assignment, requestingStudentId: null);
    }

    public async Task DeleteAssignmentAsync(int teacherId, int assignmentId)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        if (assignment.TeacherId != teacherId)
            throw new ForbiddenActionException("You may only delete your own assignments.");

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AssignmentResponse>> GetAssignmentsForStudentAsync(int studentId)
    {
        var enrollment = await _db.StudentEnrollments.FirstOrDefaultAsync(e => e.StudentId == studentId)
            ?? throw new BusinessRuleException("Student is not enrolled in any class/course.");

        var assignments = await _db.Assignments
            .Where(a => a.ClassCourseId == enrollment.ClassCourseId && a.Status == AssignmentStatus.Published)
            .Include(a => a.Subject)
            .Include(a => a.ClassCourse)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
            .OrderBy(a => a.Deadline)
            .ToListAsync();

        return assignments.Select(a => MapToResponse(a, a.Submissions.FirstOrDefault())).ToList();
    }

    public async Task<List<AssignmentResponse>> GetAssignmentsForTeacherAsync(int teacherId)
    {
        var assignments = await _db.Assignments
            .Where(a => a.TeacherId == teacherId)
            .Include(a => a.Subject)
            .Include(a => a.ClassCourse)
            .Include(a => a.Teacher)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assignments.Select(a => MapToResponse(a, null)).ToList();
    }

    public async Task<AssignmentResponse> GetAssignmentByIdAsync(int assignmentId, int requestingUserId, UserRole requestingUserRole)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Subject)
            .Include(a => a.ClassCourse)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        if (requestingUserRole == UserRole.Student)
        {
            var enrolled = await _db.StudentEnrollments.AnyAsync(e =>
                e.StudentId == requestingUserId && e.ClassCourseId == assignment.ClassCourseId);

            if (!enrolled || assignment.Status != AssignmentStatus.Published)
                throw new ForbiddenActionException("You do not have access to this assignment.");

            var mySubmission = await _db.Submissions.FirstOrDefaultAsync(s =>
                s.AssignmentId == assignmentId && s.StudentId == requestingUserId);

            return MapToResponse(assignment, mySubmission);
        }

        if (requestingUserRole == UserRole.Teacher && assignment.TeacherId != requestingUserId)
            throw new ForbiddenActionException("You may only view your own assignments.");

        return MapToResponse(assignment, null);
    }

    public async Task<List<AssignmentResponse>> GetAllAssignmentsAsync()
    {
        var assignments = await _db.Assignments
            .Include(a => a.Subject)
            .Include(a => a.ClassCourse)
            .Include(a => a.Teacher)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assignments.Select(a => MapToResponse(a, null)).ToList();
    }

    private async Task<AssignmentResponse> ToResponseAsync(Assignment assignment, int? requestingStudentId)
    {
        await _db.Entry(assignment).Reference(a => a.Subject).LoadAsync();
        await _db.Entry(assignment).Reference(a => a.ClassCourse).LoadAsync();
        await _db.Entry(assignment).Reference(a => a.Teacher).LoadAsync();
        return MapToResponse(assignment, null);
    }

    private static AssignmentResponse MapToResponse(Assignment a, Submission? mySubmission) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        SubjectId = a.SubjectId,
        SubjectName = a.Subject?.Name ?? string.Empty,
        ClassCourseId = a.ClassCourseId,
        ClassCourseName = a.ClassCourse?.Name ?? string.Empty,
        TeacherId = a.TeacherId,
        TeacherName = a.Teacher?.FullName ?? string.Empty,
        Deadline = a.Deadline,
        MaxMarks = a.MaxMarks,
        Status = a.Status,
        AllowResubmission = a.AllowResubmission,
        CreatedAt = a.CreatedAt,
        MySubmissionStatus = mySubmission?.Status.ToString()
    };
}

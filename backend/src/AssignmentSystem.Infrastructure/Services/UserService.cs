using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UserService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new BusinessRuleException("A user with this email already exists.");

        if (request.Role == UserRole.Student && request.ClassCourseId is null)
            throw new BusinessRuleException("A class/course must be specified when creating a student.");

        if (request.ClassCourseId is not null &&
            !await _db.ClassCourses.AnyAsync(c => c.Id == request.ClassCourseId))
            throw new NotFoundException("The specified class/course was not found.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (request.Role == UserRole.Student && request.ClassCourseId is not null)
        {
            _db.StudentEnrollments.Add(new StudentEnrollment
            {
                StudentId = user.Id,
                ClassCourseId = request.ClassCourseId.Value
            });
            await _db.SaveChangesAsync();
        }

        return ToResponse(user);
    }

    public async Task<List<UserResponse>> GetAllUsersAsync(UserRole? roleFilter)
    {
        var query = _db.Users.AsQueryable();
        if (roleFilter is not null)
            query = query.Where(u => u.Role == roleFilter);

        return await query
            .OrderBy(u => u.FullName)
            .Select(u => ToResponse(u))
            .ToListAsync();
    }

    public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.IsActive is not null) user.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task DeactivateUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        user.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task AssignTeacherToSubjectAsync(AssignTeacherRequest request)
    {
        var teacher = await _db.Users.FindAsync(request.TeacherId)
            ?? throw new NotFoundException("Teacher not found.");
        if (teacher.Role != UserRole.Teacher)
            throw new BusinessRuleException("The specified user is not a teacher.");

        var subject = await _db.Subjects.FindAsync(request.SubjectId)
            ?? throw new NotFoundException("Subject not found.");

        if (subject.ClassCourseId != request.ClassCourseId)
            throw new BusinessRuleException("The subject does not belong to the specified class/course.");

        var alreadyAssigned = await _db.TeacherSubjectClasses.AnyAsync(t =>
            t.TeacherId == request.TeacherId &&
            t.SubjectId == request.SubjectId &&
            t.ClassCourseId == request.ClassCourseId);

        if (alreadyAssigned)
            throw new BusinessRuleException("This teacher is already assigned to this subject/class.");

        _db.TeacherSubjectClasses.Add(new TeacherSubjectClass
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            ClassCourseId = request.ClassCourseId
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<TeachingAssignmentResponse>> GetMyTeachingAssignmentsAsync(int teacherId)
    {
        return await _db.TeacherSubjectClasses
            .Where(t => t.TeacherId == teacherId)
            .Include(t => t.Subject)
            .Include(t => t.ClassCourse)
            .OrderBy(t => t.ClassCourse.Name).ThenBy(t => t.Subject.Name)
            .Select(t => new TeachingAssignmentResponse
            {
                SubjectId = t.SubjectId,
                SubjectName = t.Subject.Name,
                ClassCourseId = t.ClassCourseId,
                ClassCourseName = t.ClassCourse.Name
            })
            .ToListAsync();
    }

    private static UserResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role,
        IsActive = u.IsActive
    };
}

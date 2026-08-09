using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;

namespace AssignmentSystem.Core.Interfaces;

// Thrown by services when a business rule is violated (deadline passed, resubmission not allowed, etc.)
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

// Thrown when the acting user is not allowed to perform the action (not "invalid credentials" —
// that's a 401; this is "you are who you say you are, but you may not do this" — a 403).
public class ForbiddenActionException : Exception
{
    public ForbiddenActionException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<List<UserResponse>> GetAllUsersAsync(UserRole? roleFilter);
    Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task DeactivateUserAsync(int userId);
    Task AssignTeacherToSubjectAsync(AssignTeacherRequest request);
    Task<List<TeachingAssignmentResponse>> GetMyTeachingAssignmentsAsync(int teacherId);
}

public interface IAcademicService
{
    Task<ClassCourseResponse> CreateClassCourseAsync(CreateClassCourseRequest request);
    Task<List<ClassCourseResponse>> GetAllClassCoursesAsync();
    Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request);
    Task<List<SubjectResponse>> GetSubjectsByClassAsync(int classCourseId);
}

public interface IAssignmentService
{
    Task<AssignmentResponse> CreateAssignmentAsync(int teacherId, CreateAssignmentRequest request);
    Task<AssignmentResponse> UpdateAssignmentAsync(int teacherId, int assignmentId, UpdateAssignmentRequest request);
    Task DeleteAssignmentAsync(int teacherId, int assignmentId);
    Task<List<AssignmentResponse>> GetAssignmentsForStudentAsync(int studentId);
    Task<List<AssignmentResponse>> GetAssignmentsForTeacherAsync(int teacherId);
    Task<AssignmentResponse> GetAssignmentByIdAsync(int assignmentId, int requestingUserId, UserRole requestingUserRole);
    Task<List<AssignmentResponse>> GetAllAssignmentsAsync();
}

public interface ISubmissionService
{
    Task<SubmissionResponse> SubmitAsync(int studentId, int assignmentId, CreateSubmissionRequest request);
    Task<SubmissionResponse> UpdateSubmissionAsync(int studentId, int assignmentId, CreateSubmissionRequest request);
    Task<List<SubmissionResponse>> GetSubmissionsForAssignmentAsync(int teacherId, int assignmentId);
    Task<SubmissionResponse> GradeSubmissionAsync(int teacherId, int submissionId, GradeSubmissionRequest request);
    Task<SubmissionResponse> UpdateStatusAsync(int teacherId, int submissionId, UpdateSubmissionStatusRequest request);
    Task<SubmissionResponse?> GetMySubmissionAsync(int studentId, int assignmentId);
    Task<List<SubmissionResponse>> GetAllSubmissionsAsync();
}

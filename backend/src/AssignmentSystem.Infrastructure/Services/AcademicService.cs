using AssignmentSystem.Core.DTOs;
using AssignmentSystem.Core.Entities;
using AssignmentSystem.Core.Interfaces;
using AssignmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Services;

public class AcademicService : IAcademicService
{
    private readonly AppDbContext _db;

    public AcademicService(AppDbContext db) => _db = db;

    public async Task<ClassCourseResponse> CreateClassCourseAsync(CreateClassCourseRequest request)
    {
        var entity = new ClassCourse { Name = request.Name, Description = request.Description };
        _db.ClassCourses.Add(entity);
        await _db.SaveChangesAsync();

        return new ClassCourseResponse { Id = entity.Id, Name = entity.Name, Description = entity.Description };
    }

    public async Task<List<ClassCourseResponse>> GetAllClassCoursesAsync()
    {
        return await _db.ClassCourses
            .OrderBy(c => c.Name)
            .Select(c => new ClassCourseResponse { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();
    }

    public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request)
    {
        var classCourse = await _db.ClassCourses.FindAsync(request.ClassCourseId)
            ?? throw new NotFoundException("Class/course not found.");

        if (await _db.Subjects.AnyAsync(s => s.ClassCourseId == request.ClassCourseId && s.Name == request.Name))
            throw new BusinessRuleException("This subject already exists for the given class/course.");

        var subject = new Subject
        {
            Name = request.Name,
            Code = request.Code,
            ClassCourseId = request.ClassCourseId
        };

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        return new SubjectResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            ClassCourseId = subject.ClassCourseId,
            ClassCourseName = classCourse.Name
        };
    }

    public async Task<List<SubjectResponse>> GetSubjectsByClassAsync(int classCourseId)
    {
        return await _db.Subjects
            .Where(s => s.ClassCourseId == classCourseId)
            .Include(s => s.ClassCourse)
            .OrderBy(s => s.Name)
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                ClassCourseId = s.ClassCourseId,
                ClassCourseName = s.ClassCourse.Name
            })
            .ToListAsync();
    }
}

using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Assemblers;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Assembly;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed class CoursePersistenceService : ICoursePersistenceService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _uow;

    public CoursePersistenceService(
        ICourseRepository courseRepository,
        IUserProfileRepository userProfileRepository,
        IQuizRepository quizRepository,
        IUnitOfWork uow)
    {
        ArgumentNullException.ThrowIfNull(courseRepository);
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(quizRepository);
        ArgumentNullException.ThrowIfNull(uow);

        _courseRepository = courseRepository;
        _userProfileRepository = userProfileRepository;
        _quizRepository = quizRepository;
        _uow = uow;
    }

    public async Task<Guid> PersistAsync(Guid userId, string topic, List<ChapterAssemblyModel> assemblyData, CancellationToken ct = default)
    {
        var chapters = CourseAssembler.BuildChapters(assemblyData);
        var course = Course.Create(topic, chapters);

        foreach (var chapter in course.Chapters)
            chapter.SetCourseId(course.Id);

        var profile = await _userProfileRepository.GetCurrentUserProfile(userId, ct)
                      ?? throw new UserProfileNotFoundException($"Profilul cu id-ul {userId} nu a fost gasit");

        profile.EnrollInCourse(course);

        await _uow.BeginTransactionAsync(ct);

        try
        {
            await _courseRepository.AddAsync(course, ct);
            _userProfileRepository.Update(profile);
            await _uow.SaveChangesAsync(ct);

            var quizQuestions = CourseAssembler.BuildQuizQuestions(assemblyData, course);
            await _quizRepository.AddRangeAsync(quizQuestions, ct);
            await _uow.SaveChangesAsync(ct);

            await _uow.CommitAsync(ct);

            return course.Id;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}

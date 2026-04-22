using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using JetBrains.Annotations;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class UserProfile : Entity
{
    private readonly List<Course> _courses = [];
    private readonly List<string> _finishedChapters = [];
    private readonly List<UserType> _userTypes = [];

    [UsedImplicitly]
    private UserProfile() //For Ef core
    {
    }

    private UserProfile(Guid accountId, LearningType learningType,
        List<UserType> userTypes, string? domain = "Student")
    {
        if (accountId == Guid.Empty)
            throw new InvalidFieldException("User id este invalid");
        if (learningType is null)
            throw new InvalidFieldException("Learning type nu poate fi null");
        if (userTypes is null || userTypes.Count == 0)
            throw new InvalidFieldException("User types este invalid");
        if (string.IsNullOrWhiteSpace(learningType.Value))
            throw new InvalidFieldException("User learning type este invalid");

        AccountId = accountId;
        LearningType = learningType;
        Domain = domain ?? "Student";
        CorrectAnswersForQuiz = 0;
        IncorrectAnswersForQuiz = 0;
        Experience = 0;

        _userTypes.AddRange(userTypes);
    }

    public Guid AccountId { get; private set; }

    public Account? Account
    {
        get;
        init
        {
            field = value;
            if (value is not null)
                AccountId = value.Id;
        }
    }

    public string Domain { get; private set; } = string.Empty;
    public int CorrectAnswersForQuiz { get; private set; }
    public int IncorrectAnswersForQuiz { get; private set; }
    public int Experience { get; private set; }
    public LearningType LearningType { get; private set; } = LearningType.ReadWrite;

    public IReadOnlyCollection<UserType> UserTypes => _userTypes.AsReadOnly();
    public IReadOnlyCollection<Course> Courses => _courses.AsReadOnly();
    public IReadOnlyCollection<string> FinishedChapters => _finishedChapters.AsReadOnly();

    public static UserProfile Create(Guid accountId, LearningType learningType, List<UserType> userTypes,
        string? domain = "Student") =>
        new(accountId, learningType, userTypes, domain);

    public void Update(LearningType newLearningType, List<UserType> newUserTypes, string newDomain)
    {
        if (newLearningType is null)
            throw new InvalidFieldException("Learning type nu poate fi null");
        if (newUserTypes is null || newUserTypes.Count == 0)
            throw new InvalidFieldException("User types este invalid");
        if (string.IsNullOrWhiteSpace(newLearningType.Value))
            throw new InvalidFieldException("User learning type este invalid");
        if (string.IsNullOrWhiteSpace(newDomain))
            throw new InvalidFieldException("Domain nu poate fi null");

        LearningType = newLearningType;
        _userTypes.Clear();
        _userTypes.AddRange(newUserTypes);
        Domain = newDomain;
    }

    public void EnrollInCourse(Course course)
    {
        if (course is null)
            throw new InvalidFieldException("Course nu poate fi null");
        if (_courses.Any(c => c.Id == course.Id))
            throw new InvalidFieldException("Deja inscris in acest curs");

        _courses.Add(course);
    }

    public void FinishChapter(string chapterId)
    {
        if (_finishedChapters.Contains(chapterId))
            throw new InvalidFieldException("Capitolul a fost deja finalizat");

        _finishedChapters.Add(chapterId);
        Experience += 10;
    }
}

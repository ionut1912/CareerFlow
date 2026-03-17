using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class UserProfile : Entity
{
    private readonly List<UserType> _userTypes = [];

    private UserProfile() //for EF core
    {
    }

    public UserProfile(Guid accountId, LearningType learningType, List<UserType> userTypes, string? domain = "Student")
    {
        if (accountId == Guid.Empty) throw new InvalidFieldException("User id este invalid");

        if (string.IsNullOrWhiteSpace(learningType.Value))
            throw new InvalidFieldException("User learning type este invalid");

        if (userTypes.Count == 0) throw new InvalidFieldException("User types este invalid");

        AccountId = accountId;
        LearningType = learningType;
        Domain = domain ?? "student";
        CorrectAnswersForQuiz = 0;
        IncorrectAnswersForQuiz = 0;
        Experience = 0;
        _userTypes.AddRange(userTypes);
    }

    public Guid AccountId { get; private set; }
    public Account Account { get; private set; }
    public string Domain { get; private set; } = string.Empty;
    public int CorrectAnswersForQuiz { get; private set; }
    public int IncorrectAnswersForQuiz { get; private set; }
    public int Experience { get; private set; }
    public LearningType LearningType { get; private set; } = LearningType.Visual;
    public IReadOnlyCollection<UserType> UserTypes => _userTypes;

    public static UserProfile Create(Guid accountId, LearningType learningType, List<UserType> userTypes,
        string? domain = "Student")
    {
        return new UserProfile(accountId, learningType, userTypes, domain);
    }

    public void Update(LearningType newLearningType, List<UserType> newUserTypes, string newDomain)
    {
        if (newLearningType == LearningType)
            throw new LearningTypeAlreadyExistsException($"LearningType {newLearningType} is already in use");

        var alreadyExists = newUserTypes.FirstOrDefault(t => _userTypes.Contains(t));
        if (alreadyExists != null)
            throw new UserTypeAlreadyExistsException($"UserType {alreadyExists} is already in use");
        if (Domain == newDomain)
            throw new DomainAlreadyExistsException($"Domain {Domain} is already in use");
        LearningType = newLearningType;
        _userTypes.Clear();
        _userTypes.AddRange(newUserTypes);
        Domain = newDomain;
    }
}
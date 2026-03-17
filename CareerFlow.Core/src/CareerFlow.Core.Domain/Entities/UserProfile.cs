using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class UserProfile : Entity
{
    private readonly List<UserType> _userTypes = [];

    private UserProfile()
    {
    }

    public UserProfile(Guid accountId, LearningType learningType, List<UserType> userTypes, string? domain = "Student")
    {
        if (accountId == Guid.Empty) throw new InvalidFieldException("User id este invalid");
        if (learningType is null) throw new InvalidFieldException("Learning type nu poate fi null");
        if (userTypes is null) throw new InvalidFieldException("User types nu poate fi null");
        if (string.IsNullOrWhiteSpace(learningType.Value))
            throw new InvalidFieldException("User learning type este invalid");
        if (userTypes.Count == 0) throw new InvalidFieldException("User types este invalid");

        AccountId = accountId;
        LearningType = learningType;
        Domain = domain ?? "Student";
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
    public IReadOnlyCollection<UserType> UserTypes => _userTypes.AsReadOnly();

    public static UserProfile Create(Guid accountId, LearningType learningType, List<UserType> userTypes,
        string? domain = "Student")
    {
        return new UserProfile(accountId, learningType, userTypes, domain);
    }

    public void Update(LearningType newLearningType, List<UserType> newUserTypes, string newDomain)
    {
        if (newLearningType is null) throw new InvalidFieldException("Learning type nu poate fi null");
        if (newUserTypes is null) throw new InvalidFieldException("User types nu poate fi null");
        if (string.IsNullOrWhiteSpace(newLearningType.Value))
            throw new InvalidFieldException("User learning type este invalid");
        if (newUserTypes.Count == 0) throw new InvalidFieldException("User types este invalid");
        if (string.IsNullOrWhiteSpace(newDomain)) throw new InvalidFieldException("Domain nu poate fi null");

        LearningType = newLearningType;
        _userTypes.Clear();
        _userTypes.AddRange(newUserTypes);
        Domain = newDomain;
    }
}
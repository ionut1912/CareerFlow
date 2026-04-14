using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Tests.Entities;

public class UserProfileEntityTests
{
    private static readonly LearningType ValidLearning = LearningType.Visual;
    private static readonly List<UserType> ValidTypes = [UserType.Student];

    private static UserProfile Profile(Guid? id = null)
    {
        return UserProfile.Create(id ?? Guid.NewGuid(), ValidLearning, ValidTypes);
    }


    [Fact]
    public void Create_ValidParameters_ReturnsProfile()
    {
        var id = Guid.NewGuid();

        var profile = UserProfile.Create(id, ValidLearning, ValidTypes);

        profile.ShouldNotBeNull();
        profile.AccountId.ShouldBe(id);
        profile.Domain.ShouldBe("Student");
        profile.Experience.ShouldBe(0);
        profile.CorrectAnswersForQuiz.ShouldBe(0);
        profile.IncorrectAnswersForQuiz.ShouldBe(0);
    }

    [Fact]
    public void Create_NullDomain_DefaultsToStudent()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), ValidLearning, ValidTypes, null);

        profile.Domain.ShouldBe("Student");
    }

    [Fact]
    public void Create_EmptyAccountId_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.Empty, ValidLearning, ValidTypes));
    }

    [Fact]
    public void Create_NullLearningType_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), null!, ValidTypes));
    }

    [Fact]
    public void Create_NullUserTypes_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), ValidLearning, null!));
    }

    [Fact]
    public void Create_EmptyUserTypes_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), ValidLearning, []));
    }

    [Fact]
    public void Update_ValidParameters_UpdatesProfile()
    {
        var profile = Profile();

        profile.Update(LearningType.Auditory, [UserType.JobSearcher], "Medicine");

        profile.Domain.ShouldBe("Medicine");
        profile.UserTypes.ShouldHaveSingleItem();
    }

    [Fact]
    public void Update_NullLearningType_ThrowsInvalidFieldException()
    {
        var profile = Profile();

        Should.Throw<InvalidFieldException>(() =>
            profile.Update(null!, ValidTypes, "Student"));
    }

    [Fact]
    public void Update_NullUserTypes_ThrowsInvalidFieldException()
    {
        var profile = Profile();

        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, null!, "Student"));
    }

    [Fact]
    public void Update_EmptyUserTypes_ThrowsInvalidFieldException()
    {
        var profile = Profile();

        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, [], "Student"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_InvalidDomain_ThrowsInvalidFieldException(string? domain)
    {
        var profile = Profile();

        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, ValidTypes, domain!));
    }

    [Fact]
    public void Update_ReplacesUserTypes()
    {
        var profile = Profile();
        var newTypes = new List<UserType> { UserType.JobSearcher, UserType.HobbyLearner };

        profile.Update(ValidLearning, newTypes, "Student");

        profile.UserTypes.Count.ShouldBe(2);
        profile.UserTypes.ShouldNotContain(UserType.Student);
    }
}
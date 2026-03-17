using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Shouldly;

namespace CareerFlow.Core.Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void Create_ValidParams_CreatesUserProfile()
    {
        //Arrange
        var accountId=Guid.NewGuid();
        var learningType = LearningType.Visual;
        var userTypes = UserType.HobbyLearner;
        var domain = "english";
        
        //Act
        var userProfile=UserProfile.Create(accountId, learningType, [userTypes],domain);
        
        //Assert
        userProfile.AccountId.ShouldBe(accountId);
        userProfile.LearningType.ShouldBe(learningType);
        userProfile.UserTypes.ToList()[0].ShouldBe(userTypes);
        userProfile.Domain.ShouldBe(domain);
    }
    
}
using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class GetUserProfileByIdQueryHandlerTests:BaseHandlerTest<GetUserProfileByIdQueryHandler>
{
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
    private readonly GetUserProfileByIdQueryHandler _handler;

    public GetUserProfileByIdQueryHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler=new GetUserProfileByIdQueryHandler( _userProfileRepositoryMock.Object,
            _loggerMock.Object);
        
    }

    [Fact]
    public async Task Handle_UserProfileFound_ReturnsUserProfile()
    {
        //Arrange
        var query = new GetUserProfileByIdQuery(Guid.NewGuid());
        var userProfileToReturn =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.HobbyLearner], "testDomain");
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, Ct))
            .ReturnsAsync(userProfileToReturn);
        
        //Act
        var result = await _handler.Handle(query, Ct);
        
        //Assert
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(userProfileToReturn.AccountId);
        result.LearningType.ShouldBe(userProfileToReturn.LearningType.Value);
        result.UserTypes[0].ShouldBe(userProfileToReturn.UserTypes.ToList()[0].Value);
        result.Domain.ShouldBe(userProfileToReturn.Domain);
    }
    
    [Fact]
    public async Task Handle_UserProfileNotFound_ThrowsException()
    {
        //Arrnage
        var query = new GetUserProfileByIdQuery(Guid.NewGuid());
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, Ct))
            .ReturnsAsync((UserProfile?)null);
        
        //Act
        var exception =await Should.ThrowAsync<UserProfileNotFoundException>(()=>_handler.Handle(query, Ct));
        
        //Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {query.Id} nu a fost gasit");
        _loggerMock.VerifyLogError(query.Id.ToString(),Times.Once());
    }
}
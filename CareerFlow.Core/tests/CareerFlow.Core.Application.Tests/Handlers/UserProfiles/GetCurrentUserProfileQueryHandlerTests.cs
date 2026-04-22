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

public class GetCurrentUserProfileQueryHandlerTests : BaseHandlerTest<GetCurrentUserProfileQueryHandler>
{
    private readonly GetCurrentUserProfileQueryHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;

    public GetCurrentUserProfileQueryHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler = new GetCurrentUserProfileQueryHandler(
            _userProfileRepositoryMock.Object,
            LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_GetCurrentUserProfileQuery_FinishSuccessfully()
    {
        //Arrange
        var query = new GetCurrentUserProfileQuery(Guid.NewGuid());
        var userProfileToReturn =
            UserProfile.Create(query.AccountId, LearningType.Auditory, [UserType.HobbyLearner], "testDomain");
        _userProfileRepositoryMock
            .Setup(x => x.GetCurrentUserProfile(query.AccountId, Ct))
            .ReturnsAsync(userProfileToReturn);

        //Act
        var result = await _handler.Handle(query, Ct);
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(query.AccountId);
        result.LearningType.ShouldBe(userProfileToReturn.LearningType.Value);
        result.UserTypes[0].ShouldBe(userProfileToReturn.UserTypes.ToList()[0].Value);
    }

    [Fact]
    public async Task Handle_UserProfileNotFound_ThrowsException()
    {
        //Arrange
        var query = new GetCurrentUserProfileQuery(Guid.NewGuid());
        _userProfileRepositoryMock
            .Setup(x => x.GetCurrentUserProfile(query.AccountId, Ct))
            .ReturnsAsync((UserProfile?)null);

        //Act
        var exception = await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(query, Ct));

        //Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {query.AccountId} nu a fost gasit");
        LoggerMock.VerifyLogError(query.AccountId.ToString(), Times.Once());
    }
}
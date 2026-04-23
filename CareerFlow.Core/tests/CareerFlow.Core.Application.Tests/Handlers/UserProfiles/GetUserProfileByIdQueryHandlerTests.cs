using System.Linq.Expressions;

using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;

using Moq;

using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class GetUserProfileByIdQueryHandlerTests : BaseHandlerTest<GetUserProfileByIdQueryHandler>
{
    private readonly GetUserProfileByIdQueryHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;

    public GetUserProfileByIdQueryHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler = new GetUserProfileByIdQueryHandler(_userProfileRepositoryMock.Object,
            LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserProfileFound_ReturnsUserProfile()
    {
        // Arrange
        var userProfileToReturn = UserProfile.Create(
            Guid.NewGuid(),
            LearningType.Auditory,
            [UserType.HobbyLearner],
            "testDomain");

        var query = new GetUserProfileByIdQuery(userProfileToReturn.Id);

        // Use It.IsAny to match the params/expressions argument used in the handler
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(
                query.Id,
                Ct,
                It.IsAny<Expression<Func<UserProfile, object>>[]>()))
            .ReturnsAsync(userProfileToReturn);

        // Act
        UserProfileDto result = await _handler.Handle(query, Ct);

        // Assert
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
        UserProfileNotFoundException exception =
            await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(query, Ct));

        //Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {query.Id} nu a fost gasit");
        LoggerMock.VerifyLogError(query.Id.ToString(), Times.Once());
    }
}

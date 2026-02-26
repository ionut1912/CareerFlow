using CareerFlow.Core.Application.CQRS.Accounts.Handler;
using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class GetCurrentAccountQueryHandlerTests : BaseHandlerTest<GetCurrentAccountQueryHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly GetCurrentAccountQueryHandler _handler;

    public GetCurrentAccountQueryHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _handler = new GetCurrentAccountQueryHandler(_accountRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var account = TestDataFactory.CreateAccount();
        var query = new GetCurrentAccountQuery(account.Id);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, Ct)).ReturnsAsync(account);

        // Act
        var result = await _handler.Handle(query, Ct);

        // Assert
        result.Id.ShouldBe(account.Id);
        result.Email.ShouldBe(account.Email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var query = new GetCurrentAccountQuery(Guid.NewGuid());
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(query.AccountId, Ct)).ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(query, Ct));

        // Assert
        _loggerMock.VerifyLogError(query.AccountId.ToString(), Times.Once());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isRepoNull, bool isLoggerNull)
    {
        //Arrange
        var query = new GetCurrentAccountQuery(Guid.NewGuid());

        //Act&Assert
        await Should.ThrowAsync<ArgumentNullException>(() => new GetCurrentAccountQueryHandler(
            isRepoNull ? null : _accountRepositoryMock.Object,
            isLoggerNull ? null : _loggerMock.Object).Handle(query, Ct));
    }
}
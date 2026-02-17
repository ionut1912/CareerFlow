namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

public sealed class TestUserContext(Guid accountId)
{
    private Guid _accountId = accountId;

    public Guid AccountId => _accountId;

    public void SetAccountId(Guid accountId) => _accountId = accountId;
}
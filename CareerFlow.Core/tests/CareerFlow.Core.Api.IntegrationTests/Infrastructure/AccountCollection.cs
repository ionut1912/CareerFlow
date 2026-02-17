using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

[CollectionDefinition("Account")]
public sealed class AccountCollection : ICollectionFixture<SharedContainerFixture> { }

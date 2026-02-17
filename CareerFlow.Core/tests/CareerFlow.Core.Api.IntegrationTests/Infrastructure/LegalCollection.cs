using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

[CollectionDefinition("Legal")]
public sealed class LegalCollection : ICollectionFixture<SharedContainerFixture> { }
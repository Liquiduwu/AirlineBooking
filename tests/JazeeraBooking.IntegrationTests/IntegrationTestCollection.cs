using Xunit;

namespace JazeeraBooking.IntegrationTests;

/// <summary>
/// All integration test classes share one CustomWebApplicationFactory (one SQL Server
/// container) via this collection, instead of each class spinning up its own - starting a
/// container per class would multiply test run time for no benefit. xUnit already runs test
/// classes within the same collection sequentially against each other (only classes in
/// *different* collections run in parallel), which is what we want here since tests share
/// database state seeded once at container startup.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

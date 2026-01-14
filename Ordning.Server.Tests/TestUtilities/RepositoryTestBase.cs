using EasyReasy.Database;
using EasyReasy.Database.Testing;

namespace Ordning.Server.Tests.TestUtilities
{
    /// <summary>
    /// Base class for repository integration tests that provides async initialization of the test database manager.
    /// </summary>
    public abstract class RepositoryTestBase : IAsyncLifetime
    {
        /// <summary>
        /// Gets the test database manager instance.
        /// </summary>
        protected TestDatabaseManager TestDatabaseManager { get; private set; } = null!;

        /// <summary>
        /// Gets the session factory for creating database sessions.
        /// </summary>
        protected IDbSessionFactory SessionFactory { get; private set; } = null!;

        /// <summary>
        /// Initializes the test database manager asynchronously before tests run.
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            TestDatabaseManager = await TestDatabaseManagerFixture.GetInstanceAsync();
            SessionFactory = new DbSessionFactory(TestDatabaseManager.DataSource);
        }

        /// <summary>
        /// Cleanup after tests complete.
        /// </summary>
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

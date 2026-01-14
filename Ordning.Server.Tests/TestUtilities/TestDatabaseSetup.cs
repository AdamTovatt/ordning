using System.Data.Common;
using EasyReasy.Database.Testing;

namespace Ordning.Server.Tests.TestUtilities
{
    /// <summary>
    /// Test database setup implementation for integration tests.
    /// </summary>
    public class TestDatabaseSetup : ITestDatabaseSetup
    {
        /// <summary>
        /// Resets the database. Empty implementation as migrations handle schema creation.
        /// </summary>
        public Task ResetDatabaseAsync(DbConnection connection)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets up the database. Empty implementation as empty migrated database is sufficient.
        /// </summary>
        public void SetupDatabase()
        {
        }
    }
}

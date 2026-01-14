using EasyReasy.Database;
using EasyReasy.Database.Testing;
using EasyReasy.EnvironmentVariables;
using Microsoft.Extensions.Logging;
using Ordning.Server.Database;

namespace Ordning.Server.Tests.TestUtilities
{
    /// <summary>
    /// Assembly-level fixture that sets up the test database manager and runs migrations once per test run.
    /// </summary>
    public static class TestDatabaseManagerFixture
    {
        private static readonly Lazy<Task<TestDatabaseManager>> LazyInstance = new Lazy<Task<TestDatabaseManager>>(
            async () => await InitializeAsync());

        /// <summary>
        /// Gets the test database manager instance for use in repository tests.
        /// This should be accessed after async initialization is complete.
        /// </summary>
        public static async Task<TestDatabaseManager> GetInstanceAsync()
        {
            return await LazyInstance.Value;
        }

        /// <summary>
        /// Initializes the test database manager and runs migrations asynchronously.
        /// </summary>
        private static async Task<TestDatabaseManager> InitializeAsync()
        {
            // Load environment variables from file
            string environmentVariablesFilePath = Path.Combine("..", "..", "EnvironmentVariables.txt");

            if (!File.Exists(environmentVariablesFilePath))
            {
                string exampleContent = EnvironmentVariableHelper.GetExampleContent(
                    "DATABASE_CONNECTION_STRING",
                    "Host=localhost;Port=5432;Database=ordning_test;Username=postgres;Password=postgres");

                File.WriteAllText(environmentVariablesFilePath, exampleContent);
            }

            EnvironmentVariableHelper.LoadVariablesFromFile(environmentVariablesFilePath);

            // Validate environment variables
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariables));

            // Get connection string
            string connectionString = EnvironmentVariables.DatabaseConnectionString.GetValue();

            // Create data source factory
            IDataSourceFactory dataSourceFactory = new NpgsqlDataSourceFactory();

            // Create test database manager
            TestDatabaseManager testDatabaseManager = new TestDatabaseManager(
                dataSourceFactory,
                () => connectionString);

            // Setup database (empty implementation, migrations handle schema)
            ITestDatabaseSetup testDatabaseSetup = new TestDatabaseSetup();
            await testDatabaseManager.EnsureCleanDatabaseSetupAsync(testDatabaseSetup);

            // Run migrations using the migrator from the main project
            ILoggerFactory loggerFactory = LoggerFactory.Create(config => config.AddConsole().SetMinimumLevel(LogLevel.Information));
            ILogger logger = loggerFactory.CreateLogger(typeof(TestDatabaseManagerFixture));
            DatabaseMigrator.RunMigrations(logger);

            return testDatabaseManager;
        }
    }
}

using System.Reflection;
using DbUp;
using DbUp.Engine;
using EasyReasy.EnvironmentVariables;
using Npgsql;

namespace Ordning.Server.Database
{
    /// <summary>
    /// Handles database migrations using DbUp.
    /// </summary>
    public static class DatabaseMigrator
    {
        /// <summary>
        /// Runs database migrations from embedded SQL scripts.
        /// </summary>
        /// <param name="logger">Logger for migration output.</param>
        /// <exception cref="Exception">Thrown if migrations fail.</exception>
        public static void RunMigrations(ILogger logger)
        {
            string connectionString = EnvironmentVariables.DatabaseConnectionString.GetValue();

            UpgradeEngine upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    (string scriptName) =>
                    {
                        return scriptName.Contains("Migrations") && scriptName.Split('.').Last() == "sql";
                    })
                .WithTransaction()
                .LogToConsole()
                .Build();

            DatabaseUpgradeResult result = upgrader.PerformUpgrade();

            if (result.Error != null && result.Error is PostgresException postgresException)
            {
                if (postgresException.SqlState.Trim() == "28P01")
                {
                    throw new Exception("Cannot connect to database, please check your connection string.");
                }
            }

            if (!result.Successful)
            {
                throw new Exception($"Error when performing database upgrade, failing on script: {result.ErrorScript?.Name ?? "(null)"} with error {result.Error}");
            }
            else
            {
                logger.LogInformation("Database is up to date.");
            }
        }
    }
}

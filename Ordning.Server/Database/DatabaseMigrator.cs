using System.Reflection;
using DbUp;
using DbUp.Engine;
using EasyReasy.EnvironmentVariables;

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
        /// <returns>True if migrations succeeded, false otherwise.</returns>
        public static bool RunMigrations(ILogger logger)
        {
            string connectionString = EnvironmentVariables.DatabaseConnectionString.GetValue();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            UpgradeEngine upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, script => script.Contains("Migrations"))
                .WithTransaction()
                .LogToConsole()
                .Build();
            
            DatabaseUpgradeResult result = upgrader.PerformUpgrade();
            
            if (!result.Successful)
            {
                logger.LogError(result.Error, "Database migration failed");
                return false;
            }
            
            if (result.ScriptsExecuted.Count > 0)
            {
                logger.LogInformation("Executed {Count} database migration(s)", result.ScriptsExecuted.Count);
                foreach (SqlScript script in result.ScriptsExecuted)
                {
                    logger.LogInformation("  - {ScriptName}", script.Name);
                }
            }
            else
            {
                logger.LogInformation("Database is up to date - no migrations executed");
            }
            
            return true;
        }
    }
}

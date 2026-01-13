using EasyReasy.EnvironmentVariables;

namespace Ordning.Server
{
    /// <summary>
    /// Container for environment variable names used by the application.
    /// </summary>
    [EnvironmentVariableNameContainer]
    public static class EnvironmentVariables
    {
        /// <summary>
        /// Secret key used for signing JWT tokens. Should be a secret value with a minimum length of 32 characters.
        /// </summary>
        [EnvironmentVariableName(minLength: 32)]
        public static readonly VariableName JwtSecret = new VariableName("JWT_SECRET");

        /// <summary>
        /// PostgreSQL database connection string.
        /// </summary>
        [EnvironmentVariableName(minLength: 10)]
        public static readonly VariableName DatabaseConnectionString = new VariableName("DATABASE_CONNECTION_STRING");
    }
}

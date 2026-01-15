using System.Data.Common;
using System.Text.Json;
using Dapper;
using EasyReasy.Database;

namespace Ordning.Server.Users.Repositories
{
    /// <summary>
    /// Repository implementation for user data access operations.
    /// </summary>
    public class UserRepository : RepositoryBase, IUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dataSource">The database data source.</param>
        /// <param name="sessionFactory">The session factory for creating database sessions.</param>
        public UserRepository(DbDataSource dataSource, IDbSessionFactory sessionFactory)
            : base(dataSource, sessionFactory)
        {
        }

        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        public async Task<UserDbModel?> GetByIdAsync(Guid id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        username,
                        email,
                        password_hash AS PasswordHash,
                        roles::text AS RolesJson
                    FROM auth_user
                    WHERE id = @{nameof(id)}";

                UserDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<UserDbModel>(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets a user by email address (case-insensitive).
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        public async Task<UserDbModel?> GetByEmailAsync(string email, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        username,
                        email,
                        password_hash AS PasswordHash,
                        roles::text AS RolesJson
                    FROM auth_user
                    WHERE LOWER(email) = LOWER(@{nameof(email)})";

                UserDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<UserDbModel>(
                    query,
                    new { email },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets a user by username (case-insensitive).
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        public async Task<UserDbModel?> GetByUsernameAsync(string username, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        username,
                        email,
                        password_hash AS PasswordHash,
                        roles::text AS RolesJson
                    FROM auth_user
                    WHERE LOWER(username) = LOWER(@{nameof(username)})";

                UserDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<UserDbModel>(
                    query,
                    new { username },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="email">The email address for the user.</param>
        /// <param name="passwordHash">The hashed password for the user.</param>
        /// <param name="roles">The collection of roles for the user. Defaults to an empty collection.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The created user database model.</returns>
        public async Task<UserDbModel> CreateAsync(string username, string email, string passwordHash, IEnumerable<string>? roles = null, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string rolesJson = JsonSerializer.Serialize(roles ?? Array.Empty<string>());

                string query = $@"
                    INSERT INTO auth_user (username, email, password_hash, roles)
                    VALUES (@{nameof(username)}, @{nameof(email)}, @{nameof(passwordHash)}, @rolesJson::jsonb)
                    RETURNING 
                        id,
                        username,
                        email,
                        password_hash AS PasswordHash,
                        roles::text AS RolesJson";

                UserDbModel result = await dbSession.Connection.QuerySingleAsync<UserDbModel>(
                    query,
                    new { username, email, passwordHash, rolesJson },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Updates the password hash for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="passwordHash">The new hashed password.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the user was found and updated; otherwise, false.</returns>
        public async Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    UPDATE auth_user
                    SET password_hash = @{nameof(passwordHash)},
                        updated_at = NOW()
                    WHERE id = @{nameof(userId)}";

                int rowsAffected = await dbSession.Connection.ExecuteAsync(
                    query,
                    new { userId, passwordHash },
                    transaction: dbSession.Transaction);

                return rowsAffected > 0;
            }, session);
        }

        /// <summary>
        /// Gets the total count of users in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The total count of users.</returns>
        public async Task<int> GetCountAsync(IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM auth_user";

                int count = await dbSession.Connection.QuerySingleAsync<int>(
                    query,
                    transaction: dbSession.Transaction);

                return count;
            }, session);
        }
    }
}

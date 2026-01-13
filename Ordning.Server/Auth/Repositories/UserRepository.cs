using System.Data.Common;
using Dapper;
using EasyReasy.Database;

namespace Ordning.Server.Auth.Repositories
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
        /// Gets a user by email address.
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
                    WHERE email = @{nameof(email)}";

                UserDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<UserDbModel>(
                    query,
                    new { email },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }
    }
}

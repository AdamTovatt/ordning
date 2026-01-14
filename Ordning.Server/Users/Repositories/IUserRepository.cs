using EasyReasy.Database;

namespace Ordning.Server.Users.Repositories
{
    /// <summary>
    /// Repository interface for user data access operations.
    /// </summary>
    public interface IUserRepository : IRepository
    {
        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        Task<UserDbModel?> GetByIdAsync(Guid id, IDbSession? session = null);

        /// <summary>
        /// Gets a user by email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        Task<UserDbModel?> GetByEmailAsync(string email, IDbSession? session = null);

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="email">The email address for the user.</param>
        /// <param name="passwordHash">The hashed password for the user.</param>
        /// <param name="roles">The collection of roles for the user. Defaults to an empty collection.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The created user database model.</returns>
        Task<UserDbModel> CreateAsync(string username, string email, string passwordHash, IEnumerable<string>? roles = null, IDbSession? session = null);

        /// <summary>
        /// Updates the password hash for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="passwordHash">The new hashed password.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the user was found and updated; otherwise, false.</returns>
        Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash, IDbSession? session = null);

        /// <summary>
        /// Gets the total count of users in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The total count of users.</returns>
        Task<int> GetCountAsync(IDbSession? session = null);
    }
}

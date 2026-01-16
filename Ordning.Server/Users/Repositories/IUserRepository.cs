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
        /// Gets a user by username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        Task<UserDbModel?> GetByUsernameAsync(string username, IDbSession? session = null);

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
        /// Gets all users in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of all user database models. Password hashes are excluded for security.</returns>
        Task<IEnumerable<UserDbModel>> GetAllAsync(IDbSession? session = null);

        /// <summary>
        /// Updates all roles for a user, replacing the existing roles.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roles">The collection of roles to set for the user.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the user was found and roles were updated; otherwise, false.</returns>
        Task<bool> UpdateRolesAsync(Guid userId, IEnumerable<string> roles, IDbSession? session = null);

        /// <summary>
        /// Adds a role to a user if it doesn't already exist.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The role to add.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the user was found and role was added (or already existed); otherwise, false.</returns>
        Task<bool> AddRoleAsync(Guid userId, string role, IDbSession? session = null);

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The role to remove.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the user was found and role was removed (or didn't exist); otherwise, false.</returns>
        Task<bool> RemoveRoleAsync(Guid userId, string role, IDbSession? session = null);

        /// <summary>
        /// Gets the total count of users in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The total count of users.</returns>
        Task<int> GetCountAsync(IDbSession? session = null);
    }
}

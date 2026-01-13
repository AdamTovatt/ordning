using EasyReasy.Database;

namespace Ordning.Server.Auth.Repositories
{
    /// <summary>
    /// Repository interface for user data access operations.
    /// </summary>
    public interface IUserRepository : IRepository
    {
        /// <summary>
        /// Gets a user by email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The user database model if found; otherwise, null.</returns>
        Task<UserDbModel?> GetByEmailAsync(string email, IDbSession? session = null);
    }
}

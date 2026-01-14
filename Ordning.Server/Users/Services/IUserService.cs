using Ordning.Server.Users.Models;

namespace Ordning.Server.Users.Services
{
    /// <summary>
    /// Provides functionality for validating user credentials.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Validates the provided email and password credentials.
        /// </summary>
        /// <param name="email">The email address to validate (passed as username from the login request).</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object if the credentials are valid; otherwise, <c>null</c>.</returns>
        Task<User?> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="email">The email address for the user.</param>
        /// <param name="password">The plain text password for the user (will be hashed).</param>
        /// <param name="roles">The collection of roles for the user. Defaults to an empty collection.</param>
        /// <returns>The created user domain model.</returns>
        Task<User> CreateUserAsync(string username, string email, string password, IEnumerable<string>? roles = null);

        /// <summary>
        /// Updates the password for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="newPassword">The new plain text password (will be hashed).</param>
        /// <param name="username">The username of the user (used as salt for password hashing).</param>
        /// <returns>True if the user was found and password was updated; otherwise, false.</returns>
        Task<bool> UpdatePasswordAsync(string userId, string newPassword, string username);
    }
}

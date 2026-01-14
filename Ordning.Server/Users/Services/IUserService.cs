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
    }
}

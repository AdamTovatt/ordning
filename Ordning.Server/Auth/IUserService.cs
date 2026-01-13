namespace Ordning.Server.Auth
{
    /// <summary>
    /// Provides functionality for validating user credentials.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Validates the provided username and password credentials.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object if the credentials are valid; otherwise, <c>null</c>.</returns>
        Task<User?> ValidateCredentialsAsync(string username, string password);
    }
}

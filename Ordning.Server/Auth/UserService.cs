namespace Ordning.Server.Auth
{
    /// <summary>
    /// Implementation of <see cref="IUserService"/> that validates user credentials.
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Validates the provided username and password credentials.
        /// Currently supports a hardcoded test user with username "test@test.com" and password "test".
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object with admin role if the credentials match the test user; otherwise, <c>null</c>.</returns>
        public Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            if (username == "test@test.com" && password == "test")
            {
                User user = new User
                {
                    Id = "test-user-id",
                    Username = username,
                    Roles = new[] { "admin" },
                };
                return Task.FromResult<User?>(user);
            }

            return Task.FromResult<User?>(null);
        }
    }
}

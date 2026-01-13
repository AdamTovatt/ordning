namespace Ordning.Server.Auth
{
    /// <summary>
    /// Implementation of <see cref="IUserService"/> that validates user credentials.
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Validates the provided email and password credentials.
        /// Currently supports a hardcoded test user with email "test@test.com" and password "test".
        /// </summary>
        /// <param name="email">The email address to validate (passed as username from the login request).</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object with admin role if the credentials match the test user; otherwise, <c>null</c>.</returns>
        public Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            if (email == "test@test.com" && password == "test")
            {
                User user = new User(
                    id: "test-user-id",
                    username: "testuser",
                    email: "test@test.com",
                    roles: new[] { "admin" });
                return Task.FromResult<User?>(user);
            }

            return Task.FromResult<User?>(null);
        }
    }
}

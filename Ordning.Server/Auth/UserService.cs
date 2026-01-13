using EasyReasy.Auth;
using Ordning.Server.Auth.Repositories;

namespace Ordning.Server.Auth
{
    /// <summary>
    /// Implementation of <see cref="IUserService"/> that validates user credentials.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository for database access.</param>
        /// <param name="passwordHasher">The password hasher for verifying passwords.</param>
        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Validates the provided email and password credentials.
        /// </summary>
        /// <param name="email">The email address to validate (passed as username from the login request).</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object if the credentials are valid; otherwise, <c>null</c>.</returns>
        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            UserDbModel? userDbModel = await _userRepository.GetByEmailAsync(email);
            if (userDbModel == null)
            {
                return null;
            }

            bool isPasswordValid = await _passwordHasher.VerifyPasswordAsync(password, userDbModel.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            return userDbModel.ToDomainUser();
        }
    }
}

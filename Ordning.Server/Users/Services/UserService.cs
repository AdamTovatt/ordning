using EasyReasy.Auth;
using Ordning.Server.Users.Models;
using Ordning.Server.Users.Repositories;

namespace Ordning.Server.Users.Services
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

            bool isPasswordValid = _passwordHasher.ValidatePassword(password, userDbModel.PasswordHash, userDbModel.Email);
            if (!isPasswordValid)
            {
                return null;
            }

            return userDbModel.ToDomainUser();
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="email">The email address for the user.</param>
        /// <param name="password">The plain text password for the user (will be hashed).</param>
        /// <param name="roles">The collection of roles for the user. Defaults to an empty collection.</param>
        /// <returns>The created user domain model.</returns>
        public async Task<User> CreateUserAsync(string username, string email, string password, IEnumerable<string>? roles = null)
        {
            string passwordHash = _passwordHasher.HashPassword(password, email);
            
            UserDbModel userDbModel = await _userRepository.CreateAsync(
                username: username,
                email: email,
                passwordHash: passwordHash,
                roles: roles);
            
            return userDbModel.ToDomainUser();
        }

        /// <summary>
        /// Updates the password for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="newPassword">The new plain text password (will be hashed).</param>
        /// <param name="email">The email address of the user (used as salt for password hashing).</param>
        /// <returns>True if the user was found and password was updated; otherwise, false.</returns>
        public async Task<bool> UpdatePasswordAsync(string userId, string newPassword, string email)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return false;
            }

            string passwordHash = _passwordHasher.HashPassword(newPassword, email);
            
            bool updated = await _userRepository.UpdatePasswordAsync(
                userId: userGuid,
                passwordHash: passwordHash);
            
            return updated;
        }

        /// <summary>
        /// Gets the total count of users in the system.
        /// </summary>
        /// <returns>The total count of users.</returns>
        public async Task<int> GetUserCountAsync()
        {
            return await _userRepository.GetCountAsync();
        }
    }
}

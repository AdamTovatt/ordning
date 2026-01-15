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
        /// Validates the provided email/username and password credentials.
        /// </summary>
        /// <param name="email">The email address or username to validate (passed as username from the login request). First checks email, then username if email lookup fails.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>A <see cref="User"/> object if the credentials are valid; otherwise, <c>null</c>.</returns>
        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            UserDbModel? userDbModel = await _userRepository.GetByEmailAsync(email);
            if (userDbModel == null)
            {
                userDbModel = await _userRepository.GetByUsernameAsync(email);
                if (userDbModel == null)
                {
                    return null;
                }
            }

            bool isPasswordValid = _passwordHasher.ValidatePassword(password, userDbModel.PasswordHash, userDbModel.Email);
            if (!isPasswordValid)
            {
                return null;
            }

            return userDbModel.ToDomainUser();
        }

        /// <summary>
        /// Gets a user by unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return null;
            }

            UserDbModel? userDbModel = await _userRepository.GetByIdAsync(userGuid);
            if (userDbModel == null)
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
        /// <returns>True if the user was found and password was updated; otherwise, false.</returns>
        public async Task<bool> UpdatePasswordAsync(string userId, string newPassword)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return false;
            }

            UserDbModel? userDbModel = await _userRepository.GetByIdAsync(userGuid);
            if (userDbModel == null)
            {
                return false;
            }

            string passwordHash = _passwordHasher.HashPassword(newPassword, userDbModel.Email);
            
            bool updated = await _userRepository.UpdatePasswordAsync(
                userId: userGuid,
                passwordHash: passwordHash);
            
            return updated;
        }

        /// <summary>
        /// Gets all users in the system.
        /// </summary>
        /// <returns>A collection of all users.</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            IEnumerable<UserDbModel> userDbModels = await _userRepository.GetAllAsync();
            return userDbModels.Select(u => u.ToDomainUser());
        }

        /// <summary>
        /// Updates all roles for a user, replacing the existing roles.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roles">The collection of roles to set for the user. Only "write" and "admin" are allowed.</param>
        /// <returns>True if the user was found and roles were updated; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid role is provided.</exception>
        public async Task<bool> UpdateRolesAsync(string userId, IEnumerable<string> roles)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return false;
            }

            IEnumerable<string> rolesList = (roles ?? Array.Empty<string>()).ToList();
            foreach (string role in rolesList)
            {
                ValidateRole(role);
            }

            return await _userRepository.UpdateRolesAsync(userGuid, rolesList);
        }

        /// <summary>
        /// Adds a role to a user if it doesn't already exist.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The role to add. Only "write" and "admin" are allowed.</param>
        /// <returns>True if the user was found and role was added (or already existed); otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid role is provided.</exception>
        public async Task<bool> AddRoleAsync(string userId, string role)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return false;
            }

            ValidateRole(role);

            return await _userRepository.AddRoleAsync(userGuid, role);
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The role to remove.</param>
        /// <returns>True if the user was found and role was removed (or didn't exist); otherwise, false.</returns>
        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return false;
            }

            return await _userRepository.RemoveRoleAsync(userGuid, role);
        }

        /// <summary>
        /// Gets the total count of users in the system.
        /// </summary>
        /// <returns>The total count of users.</returns>
        public async Task<int> GetUserCountAsync()
        {
            return await _userRepository.GetCountAsync();
        }

        /// <summary>
        /// Validates that a role is one of the allowed values.
        /// </summary>
        /// <param name="role">The role to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the role is not "write" or "admin".</exception>
        private static void ValidateRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or empty.", nameof(role));
            }

            if (role != "write" && role != "admin")
            {
                throw new ArgumentException($"Invalid role '{role}'. Only 'write' and 'admin' are allowed.", nameof(role));
            }
        }
    }
}

namespace Ordning.Server.Users.Models
{
    /// <summary>
    /// Request model for creating a user.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Gets or sets the username for the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address for the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the user.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of roles for the user.
        /// </summary>
        public IEnumerable<string>? Roles { get; set; }
    }

    /// <summary>
    /// Request model for updating a user's password.
    /// </summary>
    public class UpdatePasswordRequest
    {
        /// <summary>
        /// Gets or sets the new password for the user.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a user's roles.
    /// </summary>
    public class UpdateRolesRequest
    {
        /// <summary>
        /// Gets or sets the collection of roles for the user. Only "write" and "admin" are allowed.
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Request model for adding a role to a user.
    /// </summary>
    public class AddRoleRequest
    {
        /// <summary>
        /// Gets or sets the role to add. Only "write" and "admin" are allowed.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}

using Ordning.Server.Users.Models;
using System.Text.Json;

namespace Ordning.Server.Users.Repositories
{
    /// <summary>
    /// Database model representing a user in the database.
    /// </summary>
    public class UserDbModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the username for the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address for the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password for the user.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the roles as a JSONB string (stored as JSON array in database).
        /// </summary>
        public string RolesJson { get; set; } = "[]";

        /// <summary>
        /// Converts the database model to a domain User model.
        /// </summary>
        /// <returns>A User domain model.</returns>
        public User ToDomainUser()
        {
            IEnumerable<string> roles = Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(RolesJson))
            {
                try
                {
                    string[]? rolesArray = JsonSerializer.Deserialize<string[]>(RolesJson);
                    roles = rolesArray ?? Array.Empty<string>();
                }
                catch
                {
                    roles = Array.Empty<string>();
                }
            }

            return new User(
                id: Id.ToString(),
                username: Username,
                email: Email,
                roles: roles);
        }
    }
}

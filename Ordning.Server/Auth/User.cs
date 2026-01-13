namespace Ordning.Server.Auth
{
    /// <summary>
    /// Represents an authenticated user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets the unique identifier for the user.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the username for the user.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the email address for the user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the collection of roles assigned to the user.
        /// </summary>
        public IEnumerable<string> Roles { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the user.</param>
        /// <param name="username">The username for the user.</param>
        /// <param name="email">The email address for the user.</param>
        /// <param name="roles">The collection of roles assigned to the user. Defaults to an empty collection.</param>
        public User(string id, string username, string email, IEnumerable<string>? roles = null)
        {
            Id = id;
            Username = username;
            Email = email;
            Roles = roles ?? Array.Empty<string>();
        }
    }
}

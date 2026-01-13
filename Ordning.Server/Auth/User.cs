namespace Ordning.Server.Auth
{
    /// <summary>
    /// Represents an authenticated user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username (typically an email address) for the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of roles assigned to the user.
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }
}

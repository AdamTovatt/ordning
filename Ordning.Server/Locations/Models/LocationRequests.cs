namespace Ordning.Server.Locations.Models
{
    /// <summary>
    /// Request model for creating a location.
    /// </summary>
    public class CreateLocationRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the location.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the parent location identifier.
        /// </summary>
        public string? ParentLocationId { get; set; }
    }

    /// <summary>
    /// Request model for updating a location.
    /// </summary>
    public class UpdateLocationRequest
    {
        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the location.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the parent location identifier.
        /// </summary>
        public string? ParentLocationId { get; set; }
    }
}

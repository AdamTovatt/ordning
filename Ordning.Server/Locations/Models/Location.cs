namespace Ordning.Server.Locations.Models
{
    /// <summary>
    /// Represents a location in the system.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets the unique identifier for the location.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the location.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the location, if any.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the parent location identifier, if any.
        /// </summary>
        public string? ParentLocationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="description">The description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The parent location identifier. Defaults to null.</param>
        public Location(string id, string name, string? description = null, string? parentLocationId = null)
        {
            Id = id;
            Name = name;
            Description = description;
            ParentLocationId = parentLocationId;
        }
    }
}

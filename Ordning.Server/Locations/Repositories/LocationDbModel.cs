using Ordning.Server.Locations.Models;

namespace Ordning.Server.Locations.Repositories
{
    /// <summary>
    /// Database model representing a location in the database.
    /// </summary>
    public class LocationDbModel
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

        /// <summary>
        /// Gets or sets the UTC timestamp when the location was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the location was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Converts the database model to a domain Location model.
        /// </summary>
        /// <returns>A Location domain model.</returns>
        public Location ToDomainLocation()
        {
            return new Location(
                id: Id,
                name: Name,
                description: Description,
                parentLocationId: ParentLocationId,
                createdAt: CreatedAt,
                updatedAt: UpdatedAt);
        }
    }
}

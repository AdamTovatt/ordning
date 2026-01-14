using Ordning.Server.Locations.Models;

namespace Ordning.Server.Locations.Services
{
    /// <summary>
    /// Service interface for location business logic operations.
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <returns>The location if found; otherwise, null.</returns>
        Task<Location?> GetLocationByIdAsync(string id);

        /// <summary>
        /// Gets all locations in the system.
        /// </summary>
        /// <returns>A collection of all locations.</returns>
        Task<IEnumerable<Location>> GetAllLocationsAsync();

        /// <summary>
        /// Gets all direct children of a parent location.
        /// </summary>
        /// <param name="parentId">The unique identifier of the parent location.</param>
        /// <returns>A collection of child locations.</returns>
        Task<IEnumerable<Location>> GetChildrenAsync(string parentId);

        /// <summary>
        /// Creates a new location in the system.
        /// </summary>
        /// <param name="id">The unique identifier for the location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="description">The description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The parent location identifier. Defaults to null.</param>
        /// <returns>The created location.</returns>
        /// <exception cref="ArgumentException">Thrown when the location ID already exists or when a circular reference would be created.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the parent location does not exist.</exception>
        Task<Location> CreateLocationAsync(string id, string name, string? description = null, string? parentLocationId = null);

        /// <summary>
        /// Updates an existing location in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the location to update.</param>
        /// <param name="name">The new name of the location.</param>
        /// <param name="description">The new description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The new parent location identifier. Defaults to null.</param>
        /// <returns>The updated location.</returns>
        /// <exception cref="ArgumentException">Thrown when the location does not exist or when a circular reference would be created.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the parent location does not exist.</exception>
        Task<Location> UpdateLocationAsync(string id, string name, string? description = null, string? parentLocationId = null);

        /// <summary>
        /// Deletes a location from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <returns>True if the location was found and deleted; otherwise, false.</returns>
        Task<bool> DeleteLocationAsync(string id);
    }
}

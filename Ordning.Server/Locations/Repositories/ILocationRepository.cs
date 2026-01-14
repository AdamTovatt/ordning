using EasyReasy.Database;

namespace Ordning.Server.Locations.Repositories
{
    /// <summary>
    /// Repository interface for location data access operations.
    /// </summary>
    public interface ILocationRepository : IRepository
    {
        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The location database model if found; otherwise, null.</returns>
        Task<LocationDbModel?> GetByIdAsync(string id, IDbSession? session = null);

        /// <summary>
        /// Gets all locations in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of all location database models.</returns>
        Task<IEnumerable<LocationDbModel>> GetAllAsync(IDbSession? session = null);

        /// <summary>
        /// Gets all direct children of a parent location.
        /// </summary>
        /// <param name="parentId">The unique identifier of the parent location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of child location database models.</returns>
        Task<IEnumerable<LocationDbModel>> GetChildrenAsync(string parentId, IDbSession? session = null);

        /// <summary>
        /// Creates a new location in the database.
        /// </summary>
        /// <param name="id">The unique identifier for the location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="description">The description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The parent location identifier. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The created location database model.</returns>
        Task<LocationDbModel> CreateAsync(string id, string name, string? description = null, string? parentLocationId = null, IDbSession? session = null);

        /// <summary>
        /// Updates an existing location in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location to update.</param>
        /// <param name="name">The new name of the location.</param>
        /// <param name="description">The new description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The new parent location identifier. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location was found and updated; otherwise, false.</returns>
        Task<bool> UpdateAsync(string id, string name, string? description = null, string? parentLocationId = null, IDbSession? session = null);

        /// <summary>
        /// Deletes a location from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location was found and deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(string id, IDbSession? session = null);

        /// <summary>
        /// Checks if a location exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(string id, IDbSession? session = null);
    }
}

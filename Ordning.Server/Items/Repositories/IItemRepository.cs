using EasyReasy.Database;

namespace Ordning.Server.Items.Repositories
{
    /// <summary>
    /// Repository interface for item data access operations.
    /// </summary>
    public interface IItemRepository : IRepository
    {
        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The item database model if found; otherwise, null.</returns>
        Task<ItemDbModel?> GetByIdAsync(Guid id, IDbSession? session = null);

        /// <summary>
        /// Gets all items in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of all item database models.</returns>
        Task<IEnumerable<ItemDbModel>> GetAllAsync(IDbSession? session = null);

        /// <summary>
        /// Gets all items in a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of item database models in the specified location.</returns>
        Task<IEnumerable<ItemDbModel>> GetByLocationIdAsync(string locationId, IDbSession? session = null);

        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name="id">The unique identifier for the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="description">The description of the item. Defaults to null.</param>
        /// <param name="locationId">The location identifier where the item is stored.</param>
        /// <param name="properties">The optional properties of the item as key/value pairs. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The created item database model.</returns>
        Task<ItemDbModel> CreateAsync(Guid id, string name, string? description, string locationId, Dictionary<string, string>? properties = null, IDbSession? session = null);

        /// <summary>
        /// Updates an existing item in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="name">The new name of the item.</param>
        /// <param name="description">The new description of the item. Defaults to null.</param>
        /// <param name="properties">The new properties of the item as key/value pairs. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item was found and updated; otherwise, false.</returns>
        Task<bool> UpdateAsync(Guid id, string name, string? description = null, Dictionary<string, string>? properties = null, IDbSession? session = null);

        /// <summary>
        /// Deletes an item from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item was found and deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(Guid id, IDbSession? session = null);

        /// <summary>
        /// Checks if an item exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id, IDbSession? session = null);

        /// <summary>
        /// Moves one or more items to a new location.
        /// </summary>
        /// <param name="itemIds">The unique identifiers of the items to move.</param>
        /// <param name="newLocationId">The unique identifier of the new location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The number of items that were moved.</returns>
        Task<int> MoveItemsAsync(IEnumerable<Guid> itemIds, string newLocationId, IDbSession? session = null);
    }
}

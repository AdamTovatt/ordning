using Ordning.Server.Items.Models;

namespace Ordning.Server.Items.Services
{
    /// <summary>
    /// Service interface for item business logic operations.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>The item if found; otherwise, null.</returns>
        Task<Item?> GetItemByIdAsync(Guid id);

        /// <summary>
        /// Gets all items in the system.
        /// </summary>
        /// <returns>A collection of all items.</returns>
        Task<IEnumerable<Item>> GetAllItemsAsync();

        /// <summary>
        /// Gets all items in a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location.</param>
        /// <returns>A collection of items in the specified location.</returns>
        Task<IEnumerable<Item>> GetItemsByLocationIdAsync(string locationId);

        /// <summary>
        /// Creates a new item in the system.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="locationId">The location identifier where the item is stored.</param>
        /// <param name="description">The description of the item. Defaults to null.</param>
        /// <param name="properties">The optional properties of the item as key/value pairs. Defaults to null.</param>
        /// <returns>The created item.</returns>
        /// <exception cref="ArgumentException">Thrown when the location does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the location does not exist.</exception>
        Task<Item> CreateItemAsync(string name, string locationId, string? description = null, Dictionary<string, string>? properties = null);

        /// <summary>
        /// Updates an existing item in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="name">The new name of the item.</param>
        /// <param name="description">The new description of the item. Defaults to null.</param>
        /// <param name="properties">The new properties of the item as key/value pairs. Defaults to null.</param>
        /// <returns>The updated item.</returns>
        /// <exception cref="ArgumentException">Thrown when the item does not exist.</exception>
        Task<Item> UpdateItemAsync(Guid id, string name, string? description = null, Dictionary<string, string>? properties = null);

        /// <summary>
        /// Deletes an item from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <returns>True if the item was found and deleted; otherwise, false.</returns>
        Task<bool> DeleteItemAsync(Guid id);

        /// <summary>
        /// Moves one or more items to a new location.
        /// </summary>
        /// <param name="itemIds">The unique identifiers of the items to move.</param>
        /// <param name="newLocationId">The unique identifier of the new location.</param>
        /// <returns>The number of items that were moved.</returns>
        /// <exception cref="ArgumentException">Thrown when any item does not exist or when the new location does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the new location does not exist.</exception>
        Task<int> MoveItemsAsync(IEnumerable<Guid> itemIds, string newLocationId);
    }
}

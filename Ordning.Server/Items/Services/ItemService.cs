using System.Collections.Generic;
using System.Linq;
using EasyReasy.Database;
using Ordning.Server.Items.Models;
using Ordning.Server.Items.Repositories;
using Ordning.Server.Locations.Repositories;

namespace Ordning.Server.Items.Services
{
    /// <summary>
    /// Implementation of <see cref="IItemService"/> that provides item business logic operations.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ILocationRepository _locationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class.
        /// </summary>
        /// <param name="itemRepository">The item repository for database access.</param>
        /// <param name="locationRepository">The location repository for validation.</param>
        public ItemService(IItemRepository itemRepository, ILocationRepository locationRepository)
        {
            _itemRepository = itemRepository;
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>The item if found; otherwise, null.</returns>
        public async Task<Item?> GetItemByIdAsync(Guid id)
        {
            ItemDbModel? itemDbModel = await _itemRepository.GetByIdAsync(id);
            if (itemDbModel == null)
            {
                return null;
            }

            return itemDbModel.ToDomainItem();
        }

        /// <summary>
        /// Gets all items in the system.
        /// </summary>
        /// <returns>A collection of all items.</returns>
        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            IEnumerable<ItemDbModel> itemDbModels = await _itemRepository.GetAllAsync();
            return itemDbModels.Select(i => i.ToDomainItem());
        }

        /// <summary>
        /// Gets all items in a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location.</param>
        /// <returns>A collection of items in the specified location.</returns>
        public async Task<IEnumerable<Item>> GetItemsByLocationIdAsync(string locationId)
        {
            IEnumerable<ItemDbModel> itemDbModels = await _itemRepository.GetByLocationIdAsync(locationId);
            return itemDbModels.Select(i => i.ToDomainItem());
        }

        /// <summary>
        /// Creates a new item in the system.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="locationId">The location identifier where the item is stored.</param>
        /// <param name="description">The description of the item. Defaults to null.</param>
        /// <param name="properties">The optional properties of the item as key/value pairs. Defaults to null.</param>
        /// <returns>The created item.</returns>
        /// <exception cref="ArgumentException">Thrown when the location does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the location does not exist or when the location has child locations.</exception>
        public async Task<Item> CreateItemAsync(string name, string locationId, string? description = null, Dictionary<string, string>? properties = null)
        {
            bool locationExists = await _locationRepository.ExistsAsync(locationId);
            if (!locationExists)
            {
                throw new InvalidOperationException($"Location with ID '{locationId}' does not exist.");
            }

            bool hasChildren = await _locationRepository.HasChildrenAsync(locationId);
            if (hasChildren)
            {
                throw new InvalidOperationException("Items cannot be added to the selected location because it has child locations. Please select a more specific location.");
            }

            Guid itemId = Guid.NewGuid();
            ItemDbModel itemDbModel = await _itemRepository.CreateAsync(
                id: itemId,
                name: name,
                description: description,
                locationId: locationId,
                properties: properties);

            return itemDbModel.ToDomainItem();
        }

        /// <summary>
        /// Updates an existing item in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="name">The new name of the item.</param>
        /// <param name="description">The new description of the item. Defaults to null.</param>
        /// <param name="properties">The new properties of the item as key/value pairs. Defaults to null.</param>
        /// <returns>The updated item.</returns>
        /// <exception cref="ArgumentException">Thrown when the item does not exist.</exception>
        public async Task<Item> UpdateItemAsync(Guid id, string name, string? description = null, Dictionary<string, string>? properties = null)
        {
            ItemDbModel? existingItem = await _itemRepository.GetByIdAsync(id);
            if (existingItem == null)
            {
                throw new ArgumentException($"Item with ID '{id}' does not exist.", nameof(id));
            }

            bool updated = await _itemRepository.UpdateAsync(
                id: id,
                name: name,
                description: description,
                properties: properties);

            if (!updated)
            {
                throw new ArgumentException($"Failed to update item with ID '{id}'.", nameof(id));
            }

            ItemDbModel? updatedItem = await _itemRepository.GetByIdAsync(id);
            if (updatedItem == null)
            {
                throw new InvalidOperationException($"Item with ID '{id}' was updated but could not be retrieved.");
            }

            return updatedItem.ToDomainItem();
        }

        /// <summary>
        /// Deletes an item from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <returns>True if the item was found and deleted; otherwise, false.</returns>
        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await _itemRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Moves one or more items to a new location.
        /// </summary>
        /// <param name="itemIds">The unique identifiers of the items to move.</param>
        /// <param name="newLocationId">The unique identifier of the new location.</param>
        /// <returns>The number of items that were moved.</returns>
        /// <exception cref="ArgumentException">Thrown when any item does not exist or when the new location does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the new location does not exist or when the location has child locations.</exception>
        public async Task<int> MoveItemsAsync(IEnumerable<Guid> itemIds, string newLocationId)
        {
            Guid[] itemIdsArray = itemIds.ToArray();
            if (itemIdsArray.Length == 0)
            {
                throw new ArgumentException("At least one item ID must be provided.", nameof(itemIds));
            }

            bool locationExists = await _locationRepository.ExistsAsync(newLocationId);
            if (!locationExists)
            {
                throw new InvalidOperationException($"Location with ID '{newLocationId}' does not exist.");
            }

            bool hasChildren = await _locationRepository.HasChildrenAsync(newLocationId);
            if (hasChildren)
            {
                throw new InvalidOperationException("Items cannot be added to the selected location because it has child locations. Please select a more specific location.");
            }

            foreach (Guid itemId in itemIdsArray)
            {
                bool itemExists = await _itemRepository.ExistsAsync(itemId);
                if (!itemExists)
                {
                    throw new ArgumentException($"Item with ID '{itemId}' does not exist.", nameof(itemIds));
                }
            }

            int movedCount = await _itemRepository.MoveItemsAsync(itemIdsArray, newLocationId);
            return movedCount;
        }

        /// <summary>
        /// Searches items using full-text search with relevance ranking.
        /// If the search term is empty or whitespace, returns all items with pagination.
        /// </summary>
        /// <param name="searchTerm">The search term to match against item names, descriptions, and properties.</param>
        /// <param name="offset">The number of results to skip for pagination.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <returns>A tuple containing the matching items and the total count of matches.</returns>
        /// <exception cref="ArgumentException">Thrown when pagination parameters are invalid.</exception>
        public async Task<(IEnumerable<Item> Results, int TotalCount)> SearchItemsAsync(string searchTerm, int offset, int limit)
        {
            if (offset < 0)
            {
                throw new ArgumentException("Offset must be greater than or equal to zero.", nameof(offset));
            }

            if (limit <= 0)
            {
                throw new ArgumentException("Limit must be greater than zero.", nameof(limit));
            }

            if (limit > 100)
            {
                throw new ArgumentException("Limit cannot exceed 100.", nameof(limit));
            }

            // If search term is empty or whitespace, return all items with pagination
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                IEnumerable<ItemDbModel> allItems = await _itemRepository.GetAllAsync();
                List<ItemDbModel> allItemsList = allItems.ToList();
                int allItemsCount = allItemsList.Count;
                IEnumerable<ItemDbModel> paginatedItems = allItemsList.Skip(offset).Take(limit);
                IEnumerable<Item> allItemsResult = paginatedItems.Select(i => i.ToDomainItem());

                return (allItemsResult, allItemsCount);
            }

            (IEnumerable<ItemDbModel> results, int totalCount) = await _itemRepository.SearchAsync(searchTerm, offset, limit);
            IEnumerable<Item> items = results.Select(i => i.ToDomainItem());

            return (items, totalCount);
        }
    }
}

using Ordning.Server.Locations.Models;
using Ordning.Server.Locations.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Ordning.Server.Locations.Services
{
    /// <summary>
    /// Implementation of <see cref="ILocationService"/> that provides location business logic operations.
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class.
        /// </summary>
        /// <param name="locationRepository">The location repository for database access.</param>
        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <returns>The location if found; otherwise, null.</returns>
        public async Task<Location?> GetLocationByIdAsync(string id)
        {
            LocationDbModel? locationDbModel = await _locationRepository.GetByIdAsync(id);
            if (locationDbModel == null)
            {
                return null;
            }

            return locationDbModel.ToDomainLocation();
        }

        /// <summary>
        /// Gets all locations in the system.
        /// </summary>
        /// <returns>A collection of all locations.</returns>
        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            IEnumerable<LocationDbModel> locationDbModels = await _locationRepository.GetAllAsync();
            return locationDbModels.Select(l => l.ToDomainLocation());
        }

        /// <summary>
        /// Gets all direct children of a parent location.
        /// </summary>
        /// <param name="parentId">The unique identifier of the parent location.</param>
        /// <returns>A collection of child locations.</returns>
        public async Task<IEnumerable<Location>> GetChildrenAsync(string parentId)
        {
            IEnumerable<LocationDbModel> locationDbModels = await _locationRepository.GetChildrenAsync(parentId);
            return locationDbModels.Select(l => l.ToDomainLocation());
        }

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
        public async Task<Location> CreateLocationAsync(string id, string name, string? description = null, string? parentLocationId = null)
        {
            bool exists = await _locationRepository.ExistsAsync(id);
            if (exists)
            {
                throw new ArgumentException($"A location with ID '{id}' already exists.", nameof(id));
            }

            if (!string.IsNullOrWhiteSpace(parentLocationId))
            {
                bool parentExists = await _locationRepository.ExistsAsync(parentLocationId);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent location with ID '{parentLocationId}' does not exist.");
                }

                await ValidateNoCircularReferenceAsync(id, parentLocationId);
            }

            LocationDbModel locationDbModel = await _locationRepository.CreateAsync(
                id: id,
                name: name,
                description: description,
                parentLocationId: parentLocationId);

            return locationDbModel.ToDomainLocation();
        }

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
        public async Task<Location> UpdateLocationAsync(string id, string name, string? description = null, string? parentLocationId = null)
        {
            LocationDbModel? existingLocation = await _locationRepository.GetByIdAsync(id);
            if (existingLocation == null)
            {
                throw new ArgumentException($"Location with ID '{id}' does not exist.", nameof(id));
            }

            if (!string.IsNullOrWhiteSpace(parentLocationId))
            {
                if (parentLocationId == id)
                {
                    throw new ArgumentException("A location cannot be its own parent.", nameof(parentLocationId));
                }

                bool parentExists = await _locationRepository.ExistsAsync(parentLocationId);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent location with ID '{parentLocationId}' does not exist.");
                }

                await ValidateNoCircularReferenceAsync(id, parentLocationId);
            }

            bool updated = await _locationRepository.UpdateAsync(
                id: id,
                name: name,
                description: description,
                parentLocationId: parentLocationId);

            if (!updated)
            {
                throw new ArgumentException($"Failed to update location with ID '{id}'.", nameof(id));
            }

            LocationDbModel? updatedLocation = await _locationRepository.GetByIdAsync(id);
            if (updatedLocation == null)
            {
                throw new InvalidOperationException($"Location with ID '{id}' was updated but could not be retrieved.");
            }

            return updatedLocation.ToDomainLocation();
        }

        /// <summary>
        /// Deletes a location from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <returns>True if the location was found and deleted; otherwise, false.</returns>
        public async Task<bool> DeleteLocationAsync(string id)
        {
            return await _locationRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Validates that setting the specified parent would not create a circular reference.
        /// </summary>
        /// <param name="locationId">The ID of the location being updated.</param>
        /// <param name="newParentId">The ID of the potential new parent.</param>
        /// <exception cref="ArgumentException">Thrown when a circular reference would be created.</exception>
        private async Task ValidateNoCircularReferenceAsync(string locationId, string newParentId)
        {
            HashSet<string> visited = new HashSet<string>();
            string? currentParentId = newParentId;

            while (!string.IsNullOrWhiteSpace(currentParentId))
            {
                if (currentParentId == locationId)
                {
                    throw new ArgumentException($"Setting '{newParentId}' as parent would create a circular reference.", nameof(newParentId));
                }

                if (visited.Contains(currentParentId))
                {
                    break;
                }

                visited.Add(currentParentId);

                LocationDbModel? parent = await _locationRepository.GetByIdAsync(currentParentId);
                if (parent == null)
                {
                    break;
                }

                currentParentId = parent.ParentLocationId;
            }
        }

        /// <summary>
        /// Searches locations using full-text search with relevance ranking.
        /// </summary>
        /// <param name="searchTerm">The search term to match against location names and descriptions.</param>
        /// <param name="offset">The number of results to skip for pagination.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <returns>A tuple containing the matching locations and the total count of matches.</returns>
        /// <exception cref="ArgumentException">Thrown when the search term is null, empty, or whitespace-only, or when pagination parameters are invalid.</exception>
        public async Task<(IEnumerable<Location> Results, int TotalCount)> SearchLocationsAsync(string searchTerm, int offset, int limit)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null, empty, or whitespace-only.", nameof(searchTerm));
            }

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

            (IEnumerable<LocationDbModel> results, int totalCount) = await _locationRepository.SearchAsync(searchTerm, offset, limit);
            IEnumerable<Location> locations = results.Select(l => l.ToDomainLocation());

            return (locations, totalCount);
        }

        /// <summary>
        /// Gets all locations organized in a hierarchical tree structure.
        /// </summary>
        /// <returns>A collection of root location tree nodes (locations without a parent).</returns>
        public async Task<IEnumerable<LocationTreeNode>> GetLocationTreeAsync()
        {
            IEnumerable<LocationDbModel> allLocationDbModels = await _locationRepository.GetAllAsync();
            IEnumerable<Location> allLocations = allLocationDbModels.Select(l => l.ToDomainLocation()).ToList();

            Dictionary<string, LocationTreeNode> nodeMap = new Dictionary<string, LocationTreeNode>();
            List<LocationTreeNode> rootNodes = new List<LocationTreeNode>();

            foreach (Location location in allLocations)
            {
                LocationTreeNode node = new LocationTreeNode
                {
                    Location = location,
                    Children = new List<LocationTreeNode>(),
                };
                nodeMap[location.Id] = node;
            }

            foreach (Location location in allLocations)
            {
                LocationTreeNode node = nodeMap[location.Id];

                if (string.IsNullOrWhiteSpace(location.ParentLocationId))
                {
                    rootNodes.Add(node);
                }
                else
                {
                    if (nodeMap.TryGetValue(location.ParentLocationId, out LocationTreeNode? parentNode))
                    {
                        parentNode.Children.Add(node);
                    }
                }
            }

            foreach (LocationTreeNode rootNode in rootNodes)
            {
                SortChildrenRecursively(rootNode);
            }

            return rootNodes.OrderBy(n => n.Location.Name);
        }

        /// <summary>
        /// Recursively sorts children of a location tree node by name.
        /// </summary>
        /// <param name="node">The node whose children should be sorted.</param>
        private static void SortChildrenRecursively(LocationTreeNode node)
        {
            node.Children = node.Children.OrderBy(c => c.Location.Name).ToList();
            foreach (LocationTreeNode child in node.Children)
            {
                SortChildrenRecursively(child);
            }
        }
    }
}

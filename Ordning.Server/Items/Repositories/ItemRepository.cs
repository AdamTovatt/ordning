using System.Data.Common;
using System.Text.Json;
using Dapper;
using EasyReasy.Database;

namespace Ordning.Server.Items.Repositories
{
    /// <summary>
    /// Repository implementation for item data access operations.
    /// </summary>
    public class ItemRepository : RepositoryBase, IItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRepository"/> class.
        /// </summary>
        /// <param name="dataSource">The database data source.</param>
        /// <param name="sessionFactory">The session factory for creating database sessions.</param>
        public ItemRepository(DbDataSource dataSource, IDbSessionFactory sessionFactory)
            : base(dataSource, sessionFactory)
        {
        }

        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The item database model if found; otherwise, null.</returns>
        public async Task<ItemDbModel?> GetByIdAsync(Guid id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        name,
                        description,
                        location_id AS LocationId,
                        properties::text AS PropertiesJson
                    FROM items
                    WHERE id = @{nameof(id)}";

                ItemDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<ItemDbModel>(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets all items in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of all item database models.</returns>
        public async Task<IEnumerable<ItemDbModel>> GetAllAsync(IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = @"
                    SELECT 
                        id,
                        name,
                        description,
                        location_id AS LocationId,
                        properties::text AS PropertiesJson
                    FROM items
                    ORDER BY name";

                IEnumerable<ItemDbModel> result = await dbSession.Connection.QueryAsync<ItemDbModel>(
                    query,
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets all items in a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of item database models in the specified location.</returns>
        public async Task<IEnumerable<ItemDbModel>> GetByLocationIdAsync(string locationId, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        name,
                        description,
                        location_id AS LocationId,
                        properties::text AS PropertiesJson
                    FROM items
                    WHERE location_id = @{nameof(locationId)}
                    ORDER BY name";

                IEnumerable<ItemDbModel> result = await dbSession.Connection.QueryAsync<ItemDbModel>(
                    query,
                    new { locationId },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

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
        public async Task<ItemDbModel> CreateAsync(Guid id, string name, string? description, string locationId, Dictionary<string, string>? properties = null, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string propertiesJson = JsonSerializer.Serialize(properties ?? new Dictionary<string, string>());

                string query = $@"
                    INSERT INTO items (id, name, description, location_id, properties)
                    VALUES (@{nameof(id)}, @{nameof(name)}, @{nameof(description)}, @{nameof(locationId)}, @propertiesJson::jsonb)
                    RETURNING 
                        id,
                        name,
                        description,
                        location_id AS LocationId,
                        properties::text AS PropertiesJson";

                ItemDbModel result = await dbSession.Connection.QuerySingleAsync<ItemDbModel>(
                    query,
                    new { id, name, description, locationId, propertiesJson },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Updates an existing item in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="name">The new name of the item.</param>
        /// <param name="description">The new description of the item. Defaults to null.</param>
        /// <param name="properties">The new properties of the item as key/value pairs. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item was found and updated; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(Guid id, string name, string? description = null, Dictionary<string, string>? properties = null, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string propertiesJson = JsonSerializer.Serialize(properties ?? new Dictionary<string, string>());

                string query = $@"
                    UPDATE items
                    SET name = @{nameof(name)},
                        description = @{nameof(description)},
                        properties = @propertiesJson::jsonb,
                        updated_at = NOW()
                    WHERE id = @{nameof(id)}";

                int rowsAffected = await dbSession.Connection.ExecuteAsync(
                    query,
                    new { id, name, description, propertiesJson },
                    transaction: dbSession.Transaction);

                return rowsAffected > 0;
            }, session);
        }

        /// <summary>
        /// Deletes an item from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item was found and deleted; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(Guid id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    DELETE FROM items
                    WHERE id = @{nameof(id)}";

                int rowsAffected = await dbSession.Connection.ExecuteAsync(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return rowsAffected > 0;
            }, session);
        }

        /// <summary>
        /// Checks if an item exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the item exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM items
                    WHERE id = @{nameof(id)}";

                int count = await dbSession.Connection.QuerySingleAsync<int>(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return count > 0;
            }, session);
        }

        /// <summary>
        /// Moves one or more items to a new location.
        /// </summary>
        /// <param name="itemIds">The unique identifiers of the items to move.</param>
        /// <param name="newLocationId">The unique identifier of the new location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The number of items that were moved.</returns>
        public async Task<int> MoveItemsAsync(IEnumerable<Guid> itemIds, string newLocationId, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                Guid[] itemIdsArray = itemIds.ToArray();

                string query = $@"
                    UPDATE items
                    SET location_id = @{nameof(newLocationId)},
                        updated_at = NOW()
                    WHERE id = ANY(@{nameof(itemIdsArray)})";

                int rowsAffected = await dbSession.Connection.ExecuteAsync(
                    query,
                    new { newLocationId, itemIdsArray },
                    transaction: dbSession.Transaction);

                return rowsAffected;
            }, session);
        }
    }
}

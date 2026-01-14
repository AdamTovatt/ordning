using System.Data.Common;
using System.Text.Json;
using Dapper;
using EasyReasy.Database;
using Npgsql;
using Ordning.Server.Database;

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
            try
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
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                string constraintName = ex.ConstraintName ?? string.Empty;
                if (constraintName.Contains("fk_items_location"))
                {
                    throw new DatabaseConstraintViolationException("Location does not exist.", ex);
                }

                throw new DatabaseConstraintViolationException("A database constraint violation occurred.", ex);
            }
        }

        /// <summary>
        /// Updates an existing item in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="name">The new name of the item.</param>
        /// <param name="description">The new description of the item. Defaults to null.</param>
        /// <param name="properties">The new properties of the item as key/value pairs. Defaults to null. When null, properties are cleared to an empty dictionary (serialized as <c>{}</c>).</param>
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
            try
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
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                string constraintName = ex.ConstraintName ?? string.Empty;
                if (constraintName.Contains("fk_items_location"))
                {
                    throw new DatabaseConstraintViolationException("Location does not exist.", ex);
                }

                throw new DatabaseConstraintViolationException("A database constraint violation occurred.", ex);
            }
        }

        /// <summary>
        /// Searches items using full-text search with relevance ranking.
        /// </summary>
        /// <param name="searchTerm">The search term to match against item names, descriptions, and properties.</param>
        /// <param name="offset">The number of results to skip for pagination.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A tuple containing the matching items and the total count of matches.</returns>
        public async Task<(IEnumerable<ItemDbModel> Results, int TotalCount)> SearchAsync(string searchTerm, int offset, int limit, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                // Sanitize and prepare search terms
                string sanitizedTerm = SanitizeSearchTerm(searchTerm);
                string[] words = sanitizedTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string phraseQuery = sanitizedTerm;
                string andQuery = string.Join(" & ", words);
                string orQuery = string.Join(" | ", words);

                // If single word, use same query for all
                if (words.Length == 1)
                {
                    andQuery = words[0];
                    orQuery = words[0];
                }

                // Build the search query with relevance ranking
                string searchQuery = @"
                    SELECT 
                        id,
                        name,
                        description,
                        location_id AS LocationId,
                        properties::text AS PropertiesJson
                    FROM (
                        SELECT 
                            id,
                            name,
                            description,
                            location_id,
                            properties,
                            (
                                -- Phrase match score (highest weight)
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B') ||
                                    setweight(to_tsvector('english', COALESCE(properties::text, '')), 'C'),
                                    phraseto_tsquery('english', @phraseQuery)
                                ), 0) * 3.0 +
                                -- Both words match score
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B') ||
                                    setweight(to_tsvector('english', COALESCE(properties::text, '')), 'C'),
                                    to_tsquery('english', @andQuery)
                                ), 0) * 2.0 +
                                -- Either word match score
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B') ||
                                    setweight(to_tsvector('english', COALESCE(properties::text, '')), 'C'),
                                    to_tsquery('english', @orQuery)
                                ), 0)
                            ) AS relevance_score
                        FROM items
                        WHERE 
                            to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                            @@ phraseto_tsquery('english', @phraseQuery)
                            OR to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                            @@ to_tsquery('english', @andQuery)
                            OR to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                            @@ to_tsquery('english', @orQuery)
                    ) AS ranked_items
                    ORDER BY relevance_score DESC, name ASC
                    LIMIT @limit OFFSET @offset";

                IEnumerable<ItemDbModel> results = await dbSession.Connection.QueryAsync<ItemDbModel>(
                    searchQuery,
                    new { phraseQuery, andQuery, orQuery, limit, offset },
                    transaction: dbSession.Transaction);

                // Get total count
                string countQuery = @"
                    SELECT COUNT(*)
                    FROM items
                    WHERE 
                        to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                        @@ phraseto_tsquery('english', @phraseQuery)
                        OR to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                        @@ to_tsquery('english', @andQuery)
                        OR to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(properties::text, '')) 
                        @@ to_tsquery('english', @orQuery)";

                int totalCount = await dbSession.Connection.QuerySingleAsync<int>(
                    countQuery,
                    new { phraseQuery, andQuery, orQuery },
                    transaction: dbSession.Transaction);

                return (results, totalCount);
            }, session);
        }

        /// <summary>
        /// Sanitizes a search term for use in PostgreSQL full-text search queries.
        /// Escapes special characters that have meaning in tsquery.
        /// </summary>
        /// <param name="searchTerm">The search term to sanitize.</param>
        /// <returns>The sanitized search term.</returns>
        private static string SanitizeSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return string.Empty;
            }

            // Trim whitespace
            string sanitized = searchTerm.Trim();

            // Escape special characters for tsquery: & | ! : ' ( )
            // Replace with space, then clean up multiple spaces
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                @"[&|!:()']",
                " ");

            // Clean up multiple spaces
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                @"\s+",
                " ");

            return sanitized.Trim();
        }
    }
}

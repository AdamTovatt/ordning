using System.Data.Common;
using Dapper;
using EasyReasy.Database;
using Npgsql;
using Ordning.Server.Database;

namespace Ordning.Server.Locations.Repositories
{
    /// <summary>
    /// Repository implementation for location data access operations.
    /// </summary>
    public class LocationRepository : RepositoryBase, ILocationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationRepository"/> class.
        /// </summary>
        /// <param name="dataSource">The database data source.</param>
        /// <param name="sessionFactory">The session factory for creating database sessions.</param>
        public LocationRepository(DbDataSource dataSource, IDbSessionFactory sessionFactory)
            : base(dataSource, sessionFactory)
        {
        }

        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The location database model if found; otherwise, null.</returns>
        public async Task<LocationDbModel?> GetByIdAsync(string id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        name,
                        description,
                        parent_location_id AS ParentLocationId,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM locations
                    WHERE id = @{nameof(id)}";

                LocationDbModel? result = await dbSession.Connection.QuerySingleOrDefaultAsync<LocationDbModel>(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets all locations in the database.
        /// </summary>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of all location database models.</returns>
        public async Task<IEnumerable<LocationDbModel>> GetAllAsync(IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = @"
                    SELECT 
                        id,
                        name,
                        description,
                        parent_location_id AS ParentLocationId,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM locations
                    ORDER BY name";

                IEnumerable<LocationDbModel> result = await dbSession.Connection.QueryAsync<LocationDbModel>(
                    query,
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Gets all direct children of a parent location.
        /// </summary>
        /// <param name="parentId">The unique identifier of the parent location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A collection of child location database models.</returns>
        public async Task<IEnumerable<LocationDbModel>> GetChildrenAsync(string parentId, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT 
                        id,
                        name,
                        description,
                        parent_location_id AS ParentLocationId,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM locations
                    WHERE parent_location_id = @{nameof(parentId)}
                    ORDER BY name";

                IEnumerable<LocationDbModel> result = await dbSession.Connection.QueryAsync<LocationDbModel>(
                    query,
                    new { parentId },
                    transaction: dbSession.Transaction);

                return result;
            }, session);
        }

        /// <summary>
        /// Creates a new location in the database.
        /// </summary>
        /// <param name="id">The unique identifier for the location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="description">The description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The parent location identifier. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>The created location database model.</returns>
        public async Task<LocationDbModel> CreateAsync(string id, string name, string? description = null, string? parentLocationId = null, IDbSession? session = null)
        {
            try
            {
                return await UseSessionAsync(async (dbSession) =>
                {
                    string query = $@"
                        INSERT INTO locations (id, name, description, parent_location_id)
                        VALUES (@{nameof(id)}, @{nameof(name)}, @{nameof(description)}, @{nameof(parentLocationId)})
                        RETURNING 
                            id,
                            name,
                            description,
                            parent_location_id AS ParentLocationId,
                            created_at AS CreatedAt,
                            updated_at AS UpdatedAt";

                    LocationDbModel result = await dbSession.Connection.QuerySingleAsync<LocationDbModel>(
                        query,
                        new { id, name, description, parentLocationId },
                        transaction: dbSession.Transaction);

                    return result;
                }, session);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                string constraintName = ex.ConstraintName ?? string.Empty;
                if (constraintName.Contains("fk_locations_parent_location"))
                {
                    throw new DatabaseConstraintViolationException("Parent location does not exist.", ex);
                }

                throw new DatabaseConstraintViolationException("A database constraint violation occurred.", ex);
            }
        }

        /// <summary>
        /// Updates an existing location in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location to update.</param>
        /// <param name="name">The new name of the location.</param>
        /// <param name="description">The new description of the location. Defaults to null.</param>
        /// <param name="parentLocationId">The new parent location identifier. Defaults to null.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location was found and updated; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(string id, string name, string? description = null, string? parentLocationId = null, IDbSession? session = null)
        {
            try
            {
                return await UseSessionAsync(async (dbSession) =>
                {
                    string query = $@"
                        UPDATE locations
                        SET name = @{nameof(name)},
                            description = @{nameof(description)},
                            parent_location_id = @{nameof(parentLocationId)},
                            updated_at = NOW()
                        WHERE id = @{nameof(id)}";

                    int rowsAffected = await dbSession.Connection.ExecuteAsync(
                        query,
                        new { id, name, description, parentLocationId },
                        transaction: dbSession.Transaction);

                    return rowsAffected > 0;
                }, session);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                string constraintName = ex.ConstraintName ?? string.Empty;
                if (constraintName.Contains("fk_locations_parent_location"))
                {
                    throw new DatabaseConstraintViolationException("Parent location does not exist.", ex);
                }

                throw new DatabaseConstraintViolationException("A database constraint violation occurred.", ex);
            }
        }

        /// <summary>
        /// Deletes a location from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location was found and deleted; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(string id, IDbSession? session = null)
        {
            try
            {
                return await UseSessionAsync(async (dbSession) =>
                {
                    string query = $@"
                        DELETE FROM locations
                        WHERE id = @{nameof(id)}";

                    int rowsAffected = await dbSession.Connection.ExecuteAsync(
                        query,
                        new { id },
                        transaction: dbSession.Transaction);

                    return rowsAffected > 0;
                }, session);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                string constraintName = ex.ConstraintName ?? string.Empty;
                if (constraintName.Contains("fk_locations_parent_location"))
                {
                    throw new DatabaseConstraintViolationException("Cannot delete location because it has child locations.", ex);
                }

                if (constraintName.Contains("fk_items_location"))
                {
                    throw new DatabaseConstraintViolationException("Cannot delete location because it contains items.", ex);
                }

                throw new DatabaseConstraintViolationException("Cannot delete location because it is referenced by other records.", ex);
            }
        }

        /// <summary>
        /// Checks if a location exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>True if the location exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(string id, IDbSession? session = null)
        {
            return await UseSessionAsync(async (dbSession) =>
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM locations
                    WHERE id = @{nameof(id)}";

                int count = await dbSession.Connection.QuerySingleAsync<int>(
                    query,
                    new { id },
                    transaction: dbSession.Transaction);

                return count > 0;
            }, session);
        }

        /// <summary>
        /// Searches locations using full-text search with relevance ranking.
        /// </summary>
        /// <param name="searchTerm">The search term to match against location names and descriptions.</param>
        /// <param name="offset">The number of results to skip for pagination.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <param name="session">Optional database session. If not provided, a new session will be created.</param>
        /// <returns>A tuple containing the matching locations and the total count of matches.</returns>
        public async Task<(IEnumerable<LocationDbModel> Results, int TotalCount)> SearchAsync(string searchTerm, int offset, int limit, IDbSession? session = null)
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
                        parent_location_id AS ParentLocationId,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM (
                        SELECT 
                            id,
                            name,
                            description,
                            parent_location_id,
                            created_at,
                            updated_at,
                            (
                                -- Phrase match score (highest weight)
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B'),
                                    phraseto_tsquery('english', @phraseQuery)
                                ), 0) * 3.0 +
                                -- Both words match score
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B'),
                                    to_tsquery('english', @andQuery)
                                ), 0) * 2.0 +
                                -- Either word match score
                                COALESCE(ts_rank_cd(
                                    setweight(to_tsvector('english', name), 'A') ||
                                    setweight(to_tsvector('english', COALESCE(description, '')), 'B'),
                                    to_tsquery('english', @orQuery)
                                ), 0)
                            ) AS relevance_score
                        FROM locations
                        WHERE 
                            to_tsvector('english', name || ' ' || COALESCE(description, '')) 
                            @@ phraseto_tsquery('english', @phraseQuery)
                            OR to_tsvector('english', name || ' ' || COALESCE(description, '')) 
                            @@ to_tsquery('english', @andQuery)
                            OR to_tsvector('english', name || ' ' || COALESCE(description, '')) 
                            @@ to_tsquery('english', @orQuery)
                    ) AS ranked_locations
                    ORDER BY relevance_score DESC, name ASC
                    LIMIT @limit OFFSET @offset";

                IEnumerable<LocationDbModel> results = await dbSession.Connection.QueryAsync<LocationDbModel>(
                    searchQuery,
                    new { phraseQuery, andQuery, orQuery, limit, offset },
                    transaction: dbSession.Transaction);

                // Get total count
                string countQuery = @"
                    SELECT COUNT(*)
                    FROM locations
                    WHERE 
                        to_tsvector('english', name || ' ' || COALESCE(description, '')) 
                        @@ phraseto_tsquery('english', @phraseQuery)
                        OR to_tsvector('english', name || ' ' || COALESCE(description, '')) 
                        @@ to_tsquery('english', @andQuery)
                        OR to_tsvector('english', name || ' ' || COALESCE(description, '')) 
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

namespace Ordning.Server.Locations.Models
{
    /// <summary>
    /// Request model for creating a location.
    /// </summary>
    public class CreateLocationRequest
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
    }

    /// <summary>
    /// Request model for updating a location.
    /// </summary>
    public class UpdateLocationRequest
    {
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
    }

    /// <summary>
    /// Response model for search results with pagination metadata.
    /// </summary>
    public class SearchResponse<T>
    {
        /// <summary>
        /// Gets or sets the search results.
        /// </summary>
        public IEnumerable<T> Results { get; set; } = Array.Empty<T>();

        /// <summary>
        /// Gets or sets the total count of matching results.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the pagination offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the pagination limit.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are more results available.
        /// </summary>
        public bool HasMore { get; set; }
    }
}

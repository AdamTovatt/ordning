namespace Ordning.Server.Items.Models
{
    /// <summary>
    /// Request model for creating an item.
    /// </summary>
    public class CreateItemRequest
    {
        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the item.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the location identifier where the item is stored.
        /// </summary>
        public string LocationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional properties of the item as key/value pairs.
        /// </summary>
        public Dictionary<string, string>? Properties { get; set; }
    }

    /// <summary>
    /// Request model for updating an item.
    /// </summary>
    public class UpdateItemRequest
    {
        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the item.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the optional properties of the item as key/value pairs.
        /// </summary>
        public Dictionary<string, string>? Properties { get; set; }
    }

    /// <summary>
    /// Request model for moving items.
    /// </summary>
    public class MoveItemsRequest
    {
        /// <summary>
        /// Gets or sets the unique identifiers of the items to move.
        /// </summary>
        public IEnumerable<Guid> ItemIds { get; set; } = Array.Empty<Guid>();

        /// <summary>
        /// Gets or sets the unique identifier of the new location.
        /// </summary>
        public string NewLocationId { get; set; } = string.Empty;
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

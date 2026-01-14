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
}

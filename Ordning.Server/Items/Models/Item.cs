namespace Ordning.Server.Items.Models
{
    /// <summary>
    /// Represents an item in the system.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets the unique identifier for the item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the item, if any.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the location identifier where the item is stored.
        /// </summary>
        public string LocationId { get; }

        /// <summary>
        /// Gets the optional properties of the item as key/value pairs.
        /// </summary>
        public IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary>
        /// Gets the UTC timestamp when the item was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the UTC timestamp when the item was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="description">The description of the item. Defaults to null.</param>
        /// <param name="locationId">The location identifier where the item is stored.</param>
        /// <param name="properties">The optional properties of the item as key/value pairs. Defaults to empty dictionary.</param>
        /// <param name="createdAt">The UTC timestamp when the item was created.</param>
        /// <param name="updatedAt">The UTC timestamp when the item was last updated.</param>
        public Item(Guid id, string name, string? description, string locationId, IReadOnlyDictionary<string, string>? properties = null, DateTimeOffset createdAt = default, DateTimeOffset updatedAt = default)
        {
            Id = id;
            Name = name;
            Description = description;
            LocationId = locationId;
            Properties = properties ?? new Dictionary<string, string>();
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}

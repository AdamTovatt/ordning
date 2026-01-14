using Ordning.Server.Items.Models;
using System.Text.Json;

namespace Ordning.Server.Items.Repositories
{
    /// <summary>
    /// Database model representing an item in the database.
    /// </summary>
    public class ItemDbModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the item.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Gets or sets the properties as a JSONB string (stored as JSON object in database).
        /// </summary>
        public string PropertiesJson { get; set; } = "{}";

        /// <summary>
        /// Converts the database model to a domain Item model.
        /// </summary>
        /// <returns>An Item domain model.</returns>
        public Item ToDomainItem()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(PropertiesJson))
            {
                try
                {
                    Dictionary<string, string>? propertiesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(PropertiesJson);
                    properties = propertiesDict ?? new Dictionary<string, string>();
                }
                catch
                {
                    properties = new Dictionary<string, string>();
                }
            }

            return new Item(
                id: Id,
                name: Name,
                description: Description,
                locationId: LocationId,
                properties: properties);
        }
    }
}

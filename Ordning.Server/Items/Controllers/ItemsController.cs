using Microsoft.AspNetCore.Mvc;
using Ordning.Server.Items.Models;
using Ordning.Server.Items.Services;

namespace Ordning.Server.Items.Controllers
{
    /// <summary>
    /// Controller for managing items.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsController"/> class.
        /// </summary>
        /// <param name="itemService">The item service.</param>
        /// <param name="logger">The logger.</param>
        public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
        {
            _itemService = itemService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <returns>A collection of all items.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Item>), 200)]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllItems()
        {
            IEnumerable<Item> items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>The item if found; otherwise, 404 Not Found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Item), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Item>> GetItemById(Guid id)
        {
            Item? item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound($"Item with ID '{id}' not found.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Gets all items in a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location.</param>
        /// <returns>A collection of items in the specified location.</returns>
        [HttpGet("location/{locationId}")]
        [ProducesResponseType(typeof(IEnumerable<Item>), 200)]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsByLocation(string locationId)
        {
            IEnumerable<Item> items = await _itemService.GetItemsByLocationIdAsync(locationId);
            return Ok(items);
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="request">The item creation request.</param>
        /// <returns>The created item.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Item), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Item>> CreateItem([FromBody] CreateItemRequest request)
        {
            Item item = await _itemService.CreateItemAsync(
                name: request.Name,
                locationId: request.LocationId,
                description: request.Description,
                properties: request.Properties);

            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
        }

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="request">The item update request.</param>
        /// <returns>The updated item.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Item), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Item>> UpdateItem(Guid id, [FromBody] UpdateItemRequest request)
        {
            Item item = await _itemService.UpdateItemAsync(
                id: id,
                name: request.Name,
                description: request.Description,
                properties: request.Properties);

            return Ok(item);
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <returns>204 No Content if deleted; otherwise, 404 Not Found.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            bool deleted = await _itemService.DeleteItemAsync(id);
            if (!deleted)
            {
                return NotFound($"Item with ID '{id}' not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Moves one or more items to a new location.
        /// </summary>
        /// <param name="request">The move items request.</param>
        /// <returns>The number of items that were moved.</returns>
        [HttpPost("move")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> MoveItems([FromBody] MoveItemsRequest request)
        {
            int movedCount = await _itemService.MoveItemsAsync(
                itemIds: request.ItemIds,
                newLocationId: request.NewLocationId);

            return Ok(movedCount);
        }
    }
}

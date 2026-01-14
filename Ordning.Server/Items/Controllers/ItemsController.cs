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
            try
            {
                IEnumerable<Item> items = await _itemService.GetAllItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch items");
                return StatusCode(500, "Failed to fetch items");
            }
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
            try
            {
                Item? item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound($"Item with ID '{id}' not found.");
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch item {ItemId}", id);
                return StatusCode(500, "Failed to fetch item");
            }
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
            try
            {
                IEnumerable<Item> items = await _itemService.GetItemsByLocationIdAsync(locationId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch items for location {LocationId}", locationId);
                return StatusCode(500, "Failed to fetch items");
            }
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
            try
            {
                Item item = await _itemService.CreateItemAsync(
                    name: request.Name,
                    locationId: request.LocationId,
                    description: request.Description,
                    properties: request.Properties);

                return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating item");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating item");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create item");
                return StatusCode(500, "Failed to create item");
            }
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
            try
            {
                Item item = await _itemService.UpdateItemAsync(
                    id: id,
                    name: request.Name,
                    description: request.Description,
                    properties: request.Properties);

                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating item {ItemId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update item {ItemId}", id);
                return StatusCode(500, "Failed to update item");
            }
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
            try
            {
                bool deleted = await _itemService.DeleteItemAsync(id);
                if (!deleted)
                {
                    return NotFound($"Item with ID '{id}' not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId}", id);
                return StatusCode(500, "Failed to delete item");
            }
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
            try
            {
                int movedCount = await _itemService.MoveItemsAsync(
                    itemIds: request.ItemIds,
                    newLocationId: request.NewLocationId);

                return Ok(movedCount);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when moving items");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when moving items");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move items");
                return StatusCode(500, "Failed to move items");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ordning.Server.Items.Models;
using Ordning.Server.Items.Services;
using Ordning.Server.RateLimiting;

namespace Ordning.Server.Items.Controllers
{
    /// <summary>
    /// Controller for managing items.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(RateLimitPolicies.Lenient)]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemController"/> class.
        /// </summary>
        /// <param name="itemService">The item service.</param>
        /// <param name="logger">The logger.</param>
        public ItemController(IItemService itemService, ILogger<ItemController> logger)
        {
            _itemService = itemService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <returns>A collection of all items.</returns>
        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [Authorize(Roles = "write,admin")]
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
        [Authorize(Roles = "write,admin")]
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
        [Authorize(Roles = "write,admin")]
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
        [Authorize(Roles = "write,admin")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> MoveItems([FromBody] MoveItemsRequest request)
        {
            int movedCount = await _itemService.MoveItemsAsync(
                itemIds: request.ItemIds,
                newLocationId: request.NewLocationId);

            return Ok(movedCount);
        }

        /// <summary>
        /// Searches items using full-text search with relevance ranking.
        /// If the search query is empty or whitespace, returns all items.
        /// </summary>
        /// <param name="q">The search term to match against item names, descriptions, and properties.</param>
        /// <param name="offset">The number of results to skip for pagination. Defaults to 0.</param>
        /// <param name="limit">The maximum number of results to return. Defaults to 20, maximum 100.</param>
        /// <returns>Search results with pagination metadata.</returns>
        [HttpGet("search")]
        [Authorize]
        [ProducesResponseType(typeof(SearchResponse<Item>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SearchResponse<Item>>> SearchItems([FromQuery] string? q = null, [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {
            try
            {
                (IEnumerable<Item> results, int totalCount) = await _itemService.SearchItemsAsync(q ?? string.Empty, offset, limit);

                SearchResponse<Item> response = new SearchResponse<Item>
                {
                    Results = results,
                    TotalCount = totalCount,
                    Offset = offset,
                    Limit = limit,
                    HasMore = offset + limit < totalCount
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

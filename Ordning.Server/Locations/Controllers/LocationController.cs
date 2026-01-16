using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ordning.Server.Locations.Models;
using Ordning.Server.Locations.Services;
using Ordning.Server.RateLimiting;

namespace Ordning.Server.Locations.Controllers
{
    /// <summary>
    /// Controller for managing locations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(RateLimitPolicies.Lenient)]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationController"/> class.
        /// </summary>
        /// <param name="locationService">The location service.</param>
        /// <param name="logger">The logger.</param>
        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all locations.
        /// </summary>
        /// <returns>A collection of all locations.</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Location>), 200)]
        public async Task<ActionResult<IEnumerable<Location>>> GetAllLocations()
        {
            IEnumerable<Location> locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }

        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <returns>The location if found; otherwise, 404 Not Found.</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(Location), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Location>> GetLocationById(string id)
        {
            Location? location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound($"Location with ID '{id}' not found.");
            }

            return Ok(location);
        }

        /// <summary>
        /// Gets all direct children of a location.
        /// </summary>
        /// <param name="id">The unique identifier of the parent location.</param>
        /// <returns>A collection of child locations.</returns>
        [HttpGet("{id}/children")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Location>), 200)]
        public async Task<ActionResult<IEnumerable<Location>>> GetChildren(string id)
        {
            IEnumerable<Location> children = await _locationService.GetChildrenAsync(id);
            return Ok(children);
        }

        /// <summary>
        /// Creates a new location.
        /// </summary>
        /// <param name="request">The location creation request.</param>
        /// <returns>The created location.</returns>
        [HttpPost]
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [Authorize(Roles = "write,admin")]
        [ProducesResponseType(typeof(Location), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Location>> CreateLocation([FromBody] CreateLocationRequest request)
        {
            Location location = await _locationService.CreateLocationAsync(
                id: request.Id,
                name: request.Name,
                description: request.Description,
                parentLocationId: request.ParentLocationId);

            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, location);
        }

        /// <summary>
        /// Updates an existing location.
        /// </summary>
        /// <param name="id">The unique identifier of the location to update.</param>
        /// <param name="request">The location update request.</param>
        /// <returns>The updated location.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "write,admin")]
        [ProducesResponseType(typeof(Location), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Location>> UpdateLocation(string id, [FromBody] UpdateLocationRequest request)
        {
            Location location = await _locationService.UpdateLocationAsync(
                id: id,
                name: request.Name,
                description: request.Description,
                parentLocationId: request.ParentLocationId);

            return Ok(location);
        }

        /// <summary>
        /// Deletes a location.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <returns>204 No Content if deleted; otherwise, 404 Not Found.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "write,admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteLocation(string id)
        {
            bool deleted = await _locationService.DeleteLocationAsync(id);
            if (!deleted)
            {
                return NotFound($"Location with ID '{id}' not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Searches locations using full-text search with relevance ranking.
        /// If the search query is empty or whitespace, returns all locations.
        /// </summary>
        /// <param name="q">The search term to match against location IDs, names, and descriptions.</param>
        /// <param name="offset">The number of results to skip for pagination. Defaults to 0.</param>
        /// <param name="limit">The maximum number of results to return. Defaults to 20, maximum 100.</param>
        /// <returns>Search results with pagination metadata.</returns>
        [HttpGet("search")]
        [Authorize]
        [ProducesResponseType(typeof(SearchResponse<Location>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SearchResponse<Location>>> SearchLocations([FromQuery] string? q = null, [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {
            try
            {
                (IEnumerable<Location> results, int totalCount) = await _locationService.SearchLocationsAsync(q ?? string.Empty, offset, limit);

                SearchResponse<Location> response = new SearchResponse<Location>
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

        /// <summary>
        /// Gets all locations organized in a hierarchical tree structure.
        /// </summary>
        /// <returns>A collection of root location tree nodes (locations without a parent).</returns>
        [HttpGet("tree")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<LocationTreeNode>), 200)]
        public async Task<ActionResult<IEnumerable<LocationTreeNode>>> GetLocationTree()
        {
            IEnumerable<LocationTreeNode> tree = await _locationService.GetLocationTreeAsync();
            return Ok(tree);
        }

        /// <summary>
        /// Gets the full path from root to the specified location.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <returns>A collection of locations representing the path from root to the location, ordered from root to the target location. Returns empty collection if location not found.</returns>
        [HttpGet("{id}/path")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Location>), 200)]
        public async Task<ActionResult<IEnumerable<Location>>> GetFullPath(string id)
        {
            IEnumerable<Location> path = await _locationService.GetFullPathAsync(id);
            return Ok(path);
        }
    }
}

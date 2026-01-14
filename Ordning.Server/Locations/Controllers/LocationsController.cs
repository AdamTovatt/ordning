using Microsoft.AspNetCore.Mvc;
using Ordning.Server.Locations.Models;
using Ordning.Server.Locations.Services;

namespace Ordning.Server.Locations.Controllers
{
    /// <summary>
    /// Controller for managing locations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationsController"/> class.
        /// </summary>
        /// <param name="locationService">The location service.</param>
        /// <param name="logger">The logger.</param>
        public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all locations.
        /// </summary>
        /// <returns>A collection of all locations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Location>), 200)]
        public async Task<ActionResult<IEnumerable<Location>>> GetAllLocations()
        {
            try
            {
                IEnumerable<Location> locations = await _locationService.GetAllLocationsAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch locations");
                return StatusCode(500, "Failed to fetch locations");
            }
        }

        /// <summary>
        /// Gets a location by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location.</param>
        /// <returns>The location if found; otherwise, 404 Not Found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Location), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Location>> GetLocationById(string id)
        {
            try
            {
                Location? location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                {
                    return NotFound($"Location with ID '{id}' not found.");
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch location {LocationId}", id);
                return StatusCode(500, "Failed to fetch location");
            }
        }

        /// <summary>
        /// Gets all direct children of a location.
        /// </summary>
        /// <param name="id">The unique identifier of the parent location.</param>
        /// <returns>A collection of child locations.</returns>
        [HttpGet("{id}/children")]
        [ProducesResponseType(typeof(IEnumerable<Location>), 200)]
        public async Task<ActionResult<IEnumerable<Location>>> GetChildren(string id)
        {
            try
            {
                IEnumerable<Location> children = await _locationService.GetChildrenAsync(id);
                return Ok(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch children for location {LocationId}", id);
                return StatusCode(500, "Failed to fetch children");
            }
        }

        /// <summary>
        /// Creates a new location.
        /// </summary>
        /// <param name="request">The location creation request.</param>
        /// <returns>The created location.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Location), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Location>> CreateLocation([FromBody] CreateLocationRequest request)
        {
            try
            {
                Location location = await _locationService.CreateLocationAsync(
                    id: request.Id,
                    name: request.Name,
                    description: request.Description,
                    parentLocationId: request.ParentLocationId);

                return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, location);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating location");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating location");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create location");
                return StatusCode(500, "Failed to create location");
            }
        }

        /// <summary>
        /// Updates an existing location.
        /// </summary>
        /// <param name="id">The unique identifier of the location to update.</param>
        /// <param name="request">The location update request.</param>
        /// <returns>The updated location.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Location), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Location>> UpdateLocation(string id, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                Location location = await _locationService.UpdateLocationAsync(
                    id: id,
                    name: request.Name,
                    description: request.Description,
                    parentLocationId: request.ParentLocationId);

                return Ok(location);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating location {LocationId}", id);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating location {LocationId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update location {LocationId}", id);
                return StatusCode(500, "Failed to update location");
            }
        }

        /// <summary>
        /// Deletes a location.
        /// </summary>
        /// <param name="id">The unique identifier of the location to delete.</param>
        /// <returns>204 No Content if deleted; otherwise, 404 Not Found.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteLocation(string id)
        {
            try
            {
                bool deleted = await _locationService.DeleteLocationAsync(id);
                if (!deleted)
                {
                    return NotFound($"Location with ID '{id}' not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete location {LocationId}", id);
                return StatusCode(500, "Failed to delete location");
            }
        }
    }

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
}

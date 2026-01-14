using EasyReasy.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ordning.Server.RateLimiting;
using Ordning.Server.Users.Models;
using Ordning.Server.Users.Services;

namespace Ordning.Server.Users.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(RateLimitPolicies.Default)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="logger">The logger.</param>
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the total count of users in the system.
        /// </summary>
        /// <returns>The total count of users.</returns>
        [HttpGet("count")]
        [Authorize]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> GetUserCount()
        {
            int count = await _userService.GetUserCountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="request">The user creation request.</param>
        /// <returns>The created user.</returns>
        [HttpPost]
        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
        {
            User user = await _userService.CreateUserAsync(
                username: request.Username,
                email: request.Email,
                password: request.Password,
                roles: request.Roles);

            return Created($"/api/users/{user.Id}", user);
        }

        /// <summary>
        /// Updates the password for a user.
        /// Users can only update their own password unless they are an admin.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="request">The password update request.</param>
        /// <returns>204 No Content if updated; otherwise, 403 Forbidden or 404 Not Found.</returns>
        [HttpPut("{id}/password")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePassword(string id, [FromBody] UpdatePasswordRequest request)
        {
            string? currentUserId = HttpContext.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            IEnumerable<string> roles = HttpContext.GetRoles();
            bool isAdmin = roles.Contains("admin", StringComparer.OrdinalIgnoreCase);

            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(403, "You can only update your own password.");
            }

            bool updated = await _userService.UpdatePasswordAsync(id, request.NewPassword);
            if (!updated)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            return NoContent();
        }
    }
}

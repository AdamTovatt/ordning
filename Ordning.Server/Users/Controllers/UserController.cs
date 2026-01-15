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
    [EnableRateLimiting(RateLimitPolicies.Lenient)]
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
        /// Gets all users in the system.
        /// </summary>
        /// <returns>A collection of all users.</returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            IEnumerable<User> users = await _userService.GetAllUsersAsync();
            return Ok(users);
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
        /// Checks if the current authenticated user is an admin.
        /// </summary>
        /// <returns>True if the user is an admin; otherwise, false.</returns>
        [HttpGet("is-admin")]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        public ActionResult<bool> IsAdmin()
        {
            IEnumerable<string> roles = HttpContext.GetRoles();
            bool isAdmin = roles.Contains("admin", StringComparer.OrdinalIgnoreCase);
            return Ok(isAdmin);
        }

        /// <summary>
        /// Gets information about the current authenticated user.
        /// </summary>
        /// <returns>The current user's information.</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> GetMe()
        {
            string? currentUserId = HttpContext.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            User? user = await _userService.GetUserByIdAsync(currentUserId);
            if (user == null)
            {
                return NotFound($"User with ID '{currentUserId}' not found.");
            }

            return Ok(user);
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

        /// <summary>
        /// Updates all roles for a user, replacing the existing roles.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="request">The roles update request.</param>
        /// <returns>204 No Content if updated; otherwise, 400 Bad Request or 404 Not Found.</returns>
        [HttpPut("{id}/roles")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] UpdateRolesRequest request)
        {
            try
            {
                bool updated = await _userService.UpdateRolesAsync(id, request.Roles);
                if (!updated)
                {
                    return NotFound($"User with ID '{id}' not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a role to a user if it doesn't already exist.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="request">The add role request.</param>
        /// <returns>204 No Content if added; otherwise, 400 Bad Request or 404 Not Found.</returns>
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddRole(string id, [FromBody] AddRoleRequest request)
        {
            try
            {
                bool added = await _userService.AddRoleAsync(id, request.Role);
                if (!added)
                {
                    return NotFound($"User with ID '{id}' not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="role">The role to remove.</param>
        /// <returns>204 No Content if removed; otherwise, 404 Not Found.</returns>
        [HttpDelete("{id}/roles/{role}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            bool removed = await _userService.RemoveRoleAsync(id, role);
            if (!removed)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            return NoContent();
        }
    }
}

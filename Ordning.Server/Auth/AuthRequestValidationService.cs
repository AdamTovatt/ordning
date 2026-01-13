using EasyReasy.Auth;
using System.Security.Claims;

namespace Ordning.Server.Auth
{
    /// <summary>
    /// Implementation of <see cref="IAuthRequestValidationService"/> that validates authentication requests
    /// and issues JWT tokens for authenticated users.
    /// </summary>
    public class AuthRequestValidationService : IAuthRequestValidationService
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRequestValidationService"/> class.
        /// </summary>
        /// <param name="userService">The user service used to validate credentials.</param>
        public AuthRequestValidationService(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Validates an API key authentication request.
        /// This implementation does not support API key authentication and always returns <c>null</c>.
        /// </summary>
        /// <param name="request">The API key authentication request.</param>
        /// <param name="jwtTokenService">The JWT token service for creating tokens.</param>
        /// <param name="httpContext">The HTTP context, if available.</param>
        /// <returns>Always returns <c>null</c> as API key authentication is not supported.</returns>
        public Task<AuthResponse?> ValidateApiKeyRequestAsync(
            ApiKeyAuthRequest request,
            IJwtTokenService jwtTokenService,
            HttpContext? httpContext = null)
        {
            return Task.FromResult<AuthResponse?>(null);
        }

        /// <summary>
        /// Validates a username/password login request and issues a JWT token if the credentials are valid.
        /// </summary>
        /// <param name="request">The login authentication request containing username and password.</param>
        /// <param name="jwtTokenService">The JWT token service for creating tokens.</param>
        /// <param name="httpContext">The HTTP context, if available.</param>
        /// <returns>An <see cref="AuthResponse"/> containing the JWT token and expiration time if credentials are valid; otherwise, <c>null</c>.</returns>
        public async Task<AuthResponse?> ValidateLoginRequestAsync(
            LoginAuthRequest request,
            IJwtTokenService jwtTokenService,
            HttpContext? httpContext = null)
        {
            User? user = await _userService.ValidateCredentialsAsync(request.Username, request.Password);
            if (user == null)
                return null;

            DateTime expiresAt = DateTime.UtcNow.AddHours(1);
            string token = jwtTokenService.CreateToken(
                subject: user.Id,
                authType: "user",
                additionalClaims: Array.Empty<Claim>(),
                roles: user.Roles.ToArray(),
                expiresAt: expiresAt);

            return new AuthResponse(token, expiresAt.ToString("o"));
        }
    }
}

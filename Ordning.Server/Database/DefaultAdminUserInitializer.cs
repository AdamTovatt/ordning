using Ordning.Server.Users.Services;

namespace Ordning.Server.Database
{
    /// <summary>
    /// Background service that initializes a default admin user if no users exist in the database.
    /// </summary>
    public class DefaultAdminUserInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultAdminUserInitializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAdminUserInitializer"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for creating scoped services.</param>
        /// <param name="logger">The logger for logging initialization events.</param>
        public DefaultAdminUserInitializer(
            IServiceProvider serviceProvider,
            ILogger<DefaultAdminUserInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Starts the background service and initializes the default admin user if needed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    int userCount = await userService.GetUserCountAsync();
                    
                    if (userCount == 0)
                    {
                        _logger.LogInformation("No users found in database. Creating default admin user...");
                        
                        await userService.CreateUserAsync(
                            username: "admin",
                            email: "admin@admin.com",
                            password: "admin",
                            roles: new[] { "admin" });
                        
                        _logger.LogInformation("Default admin user created successfully. Username: admin, Email: admin@admin.com");
                    }
                    else
                    {
                        _logger.LogInformation("Database already contains {UserCount} user(s). Skipping default admin user creation.", userCount);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize default admin user");
                throw;
            }
        }

        /// <summary>
        /// Stops the background service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

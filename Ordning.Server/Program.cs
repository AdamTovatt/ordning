using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using Ordning.Server.Auth;
using Ordning.Server.Database;
using Ordning.Server.Items.Repositories;
using Ordning.Server.Items.Services;
using Ordning.Server.Locations.Repositories;
using Ordning.Server.Locations.Services;
using Ordning.Server.Middleware;
using Ordning.Server.RateLimiting;
using Ordning.Server.Users.Repositories;
using Ordning.Server.Users.Services;

namespace Ordning.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariables));

            // Run database migrations before building the app
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            // Create a temporary logger for migrations (before app is built)
            using ILoggerFactory loggerFactory = LoggerFactory.Create(config => config.AddConsole().SetMinimumLevel(LogLevel.Information));
            ILogger<Program> migrationLogger = loggerFactory.CreateLogger<Program>();
            
            DatabaseMigrator.RunMigrations(migrationLogger);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure database services
            DatabaseConfiguration.AddDatabaseServices(builder.Services);

            // Configure EasyReasy.Auth
            string jwtSecret = EnvironmentVariables.JwtSecret.GetValue();
            builder.Services.AddEasyReasyAuth(jwtSecret, issuer: "ordning");
            builder.Services.AddSingleton<IPasswordHasher, SecurePasswordHasher>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthRequestValidationService, AuthRequestValidationService>();
            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddScoped<IItemService, ItemService>();

            // Configure Rate Limiting
            builder.Services.AddRateLimiting();

            // Register background service for default admin user initialization
            builder.Services.AddHostedService<Database.DefaultAdminUserInitializer>();

            WebApplication app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseRouting();

            app.UseRateLimiter();

            app.UseEasyReasyAuth();

            app.UseAuthorization();

            // Add auth endpoints
            using (IServiceScope scope = app.Services.CreateScope())
            {
                IAuthRequestValidationService authRequestValidationService = scope.ServiceProvider.GetRequiredService<IAuthRequestValidationService>();
                app.AddAuthEndpoints(
                    authRequestValidationService,
                    allowUsernamePassword: true,
                    allowApiKeys: false);
            }

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

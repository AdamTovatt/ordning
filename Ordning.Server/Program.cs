using EasyReasy;
using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using Ordning.Server.Auth;
using Ordning.Server.Database;
using Ordning.Server.Auth.Repositories;

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
            
            bool migrationsSucceeded = DatabaseMigrator.RunMigrations(migrationLogger);
            if (!migrationsSucceeded)
            {
                throw new InvalidOperationException("Database migrations failed. Application cannot start.");
            }

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

            app.UseRouting();

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

using System.Data.Common;
using EasyReasy.Database;
using EasyReasy.EnvironmentVariables;

namespace Ordning.Server.Database
{
    /// <summary>
    /// Configures database services for dependency injection.
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Adds database services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        public static void AddDatabaseServices(IServiceCollection services)
        {
            string connectionString = EnvironmentVariables.DatabaseConnectionString.GetValue();
            
            IDataSourceFactory dataSourceFactory = new NpgsqlDataSourceFactory();
            DbDataSource dataSource = dataSourceFactory.CreateDataSource(connectionString);
            
            services.AddSingleton(dataSource);
            services.AddScoped<IDbSessionFactory, DbSessionFactory>();
        }
    }
}

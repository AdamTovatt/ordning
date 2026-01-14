using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Ordning.Server.RateLimiting
{
    /// <summary>
    /// Extension methods for configuring rate limiting services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures rate limiting services to the service collection.
        /// Configures default, strict, very strict, and lenient policies, along with a global limiter partitioned by user identity.
        /// </summary>
        /// <param name="services">The service collection to add rate limiting to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Default policy: 60 requests per minute per user
                options.AddFixedWindowLimiter(RateLimitPolicies.Default, opt =>
                {
                    opt.PermitLimit = 60;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Strict policy: 30 requests per minute per user
                options.AddFixedWindowLimiter(RateLimitPolicies.Strict, opt =>
                {
                    opt.PermitLimit = 30;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Very strict policy: 10 requests per minute per user
                options.AddFixedWindowLimiter(RateLimitPolicies.VeryStrict, opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Lenient policy: 120 requests per minute per user
                options.AddFixedWindowLimiter(RateLimitPolicies.Lenient, opt =>
                {
                    opt.PermitLimit = 120;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Custom rejection handler
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                };

                // Global limiter partitioned by user identity
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}

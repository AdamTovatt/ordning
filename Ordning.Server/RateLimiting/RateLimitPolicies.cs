namespace Ordning.Server.RateLimiting
{
    /// <summary>
    /// Constants for rate limiting policy names used throughout the application.
    /// </summary>
    public static class RateLimitPolicies
    {
        /// <summary>
        /// Default rate limiting policy: 60 requests per minute per user.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// Strict rate limiting policy: 30 requests per minute per user.
        /// </summary>
        public const string Strict = "strict";

        /// <summary>
        /// Very strict rate limiting policy: 10 requests per minute per user.
        /// </summary>
        public const string VeryStrict = "very-strict";

        /// <summary>
        /// Lenient rate limiting policy: 120 requests per minute per user.
        /// </summary>
        public const string Lenient = "lenient";
    }
}

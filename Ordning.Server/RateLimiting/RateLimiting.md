# Rate Limiting

Rate limiting middleware configured per user to prevent abuse and ensure fair resource usage.

## Available Policies

- `RateLimitPolicies.Default`: 60 requests per minute per user
- `RateLimitPolicies.Strict`: 30 requests per minute per user
- `RateLimitPolicies.VeryStrict`: 10 requests per minute per user
- `RateLimitPolicies.Lenient`: 120 requests per minute per user

## Usage

Apply rate limiting to controllers or individual action methods using the `[EnableRateLimiting]` attribute:

```csharp
[EnableRateLimiting(RateLimitPolicies.Default)]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() { }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Strict)] // Overrides controller policy
    public IActionResult Post() { }
}
```

## How It Works

- Rate limits are partitioned by user identity (authenticated users) or IP address (anonymous users)
- Each user gets their own rate limit counter
- When a limit is exceeded, requests return HTTP 429 (Too Many Requests)
- A global limiter (100 requests/minute) applies as a fallback to all endpoints

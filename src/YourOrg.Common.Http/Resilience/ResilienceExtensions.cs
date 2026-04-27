using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace YourOrg.Common.Http.Resilience;

public static class ResilienceExtensions
{
    /// <summary>
    /// Adds standard resilience to a named HttpClient:
    ///   - Retry: 3 attempts with exponential back-off
    ///   - Circuit breaker: opens after 50% failure over 30s
    ///   - Timeout: 30s per request
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder) =>
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(200);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

    /// <summary>Lightweight resilience for low-latency internal calls (no circuit breaker).</summary>
    public static IHttpClientBuilder AddLightweightResilience(this IHttpClientBuilder builder) =>
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromMilliseconds(50);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
        });
}

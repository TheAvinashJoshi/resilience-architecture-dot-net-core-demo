using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace resilience_architecture_dot_net_core_demo
{
    public static class ResilienceServiceCollectionExtension
    {
        public static IServiceCollection AddResilienceLayer(this IServiceCollection services)
        {
            services.AddResiliencePipeline<string>("CustomPipeline", pipelineBuilder =>
            {
                Console.WriteLine("--- Configuring Resilience Pipeline ---");

                // Add Retry Strategy
                pipelineBuilder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is HttpRequestException),

                    OnRetry = args =>
                    {
                        Console.WriteLine($"[Resilience] Retrying after failure. Attempt: {args.AttemptNumber}...");
                        return default;
                    }
                });

                // Add Circuit Breaker Strategy
                pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    BreakDuration = TimeSpan.FromSeconds(5),

                    ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is HttpRequestException),

                    OnHalfOpened = args => { Console.WriteLine($"[Circuit] State: Half-Open (Trial call will be allowed)."); return default; },
                    OnOpened = args => { Console.WriteLine($"[Circuit] State: OPEN (Blocking requests for 5s!)."); return default; },
                    OnClosed = args => { Console.WriteLine($"[Circuit] State: Closed (Service is healthy)."); return default; }
                });
            });
            return services;
        }
    }
}
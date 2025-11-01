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
                    MaxRetryAttempts = 3, // this will make total 4 attempts (1 initial + 3 retries)
                    Delay = TimeSpan.FromSeconds(1), // this is the base delay of 1 second after which backoff is calculated
                    BackoffType = DelayBackoffType.Exponential, // this is exponential backoff strategy that is calculated based on the Delay value and attempt number. for example 1st retry = 2^1 * Delay, 2nd retry = 2^2 * Delay, 3rd retry = 2^3 * Delay
                    ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is HttpRequestException), // this is to handle only HttpRequestException for retries for example 500 Internal Server Error

                    OnRetry = args =>
                    {
                        Console.WriteLine($"[Resilience] Retrying after failure. Attempt: {args.AttemptNumber}...");
                        return default;
                    }
                });

                // Add Circuit Breaker Strategy
                pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5, // this is the failure ratio threshold to open the circuit means 50% of the calls should fail to open the circuit
                    MinimumThroughput = 3, // this is the minimum number of calls in the sampling duration to evaluate the failure ratio means at least 3 calls should be made
                    SamplingDuration = TimeSpan.FromSeconds(10), // this is the duration in which the calls are evaluated for failure ratio means in last 10 seconds
                    BreakDuration = TimeSpan.FromSeconds(5), // this is the duration for which the circuit will remain open before transitioning to half-open state means after 5 seconds

                    ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is HttpRequestException), // this is to handle only HttpRequestException for circuit breaker

                    OnHalfOpened = args => { Console.WriteLine($"[Circuit] State: Half-Open (Trial call will be allowed)."); return default; }, // this is called when the circuit transitions to half-open state
                    OnOpened = args => { Console.WriteLine($"[Circuit] State: OPEN (Blocking requests for 5s!)."); return default; }, // this is called when the circuit transitions to open state
                    OnClosed = args => { Console.WriteLine($"[Circuit] State: Closed (Service is healthy)."); return default; } // this is called when the circuit transitions to closed state
                });
            });
            return services;
        }
    }
}
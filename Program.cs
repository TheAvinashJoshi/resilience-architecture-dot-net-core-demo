using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.Registry;
using resilience_architecture_dot_net_core_demo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<TestAPIService>();
        builder.Services.AddResilienceLayer();
        using IHost host = builder.Build();

        var serviceProvider = host.Services;
        var apiService = serviceProvider.GetRequiredService<TestAPIService>();

        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var pipeline = pipelineProvider.GetPipeline("CustomPipeline");

        Console.WriteLine("--- Starting Resilient Call Block ---");

        try
        {
            Console.Clear();
            string result = await pipeline.ExecuteAsync(async token =>
            {
                return await apiService.GetDataAsync();
            });
            Console.WriteLine($"\n[Application Result] {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Application Error] Operation failed after all retries and circuit breaker logic: {ex.Message}");
        }

        await host.RunAsync();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.Registry;
using resilience_architecture_dot_net_core_demo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // this is the host builder for dependency injection and service registration
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        // this is where we register our services
        builder.Services.AddSingleton<TestAPIService>();
        // this is where we add our resilience layer with custom pipeline
        builder.Services.AddResilienceLayer();
        // build the host
        using IHost host = builder.Build();

        // resolve services
        var serviceProvider = host.Services;
        // get the TestAPIService instance
        var apiService = serviceProvider.GetRequiredService<TestAPIService>();

        // get the ResiliencePipelineProvider to fetch our custom pipeline
        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

        // get our custom resilience pipeline
        var pipeline = pipelineProvider.GetPipeline("CustomPipeline");

        Console.WriteLine("--- Starting Resilient Call Block ---");

        try
        {
            // execute the API call within the resilience pipeline
            Console.Clear();
            string result = await pipeline.ExecuteAsync(async token =>
            {
                // call the API service method
                return await apiService.GetDataAsync();
            });
            // print the result
            Console.WriteLine($"\n[Application Result] {result}");
        }
        catch (Exception ex)
        {
            // print the final failure after all retries and circuit breaker logic
            Console.WriteLine($"\n[Application Error] Operation failed after all retries and circuit breaker logic: {ex.Message}");
        }
        // run the host
        await host.RunAsync();
    }
}
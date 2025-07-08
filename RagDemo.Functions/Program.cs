using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RagDemo.Functions.Services;

namespace RagDemo.Functions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                // Register services explicitly to avoid any conflicts
                services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
                services.AddSingleton<AzureSearchService>();

                // Register the separate interfaces for better SOLID compliance
                services.AddSingleton<IDocumentIndexService>(provider =>
                    provider.GetRequiredService<AzureSearchService>());
                services.AddSingleton<IDocumentSearchService>(provider =>
                    provider.GetRequiredService<AzureSearchService>());

                services.AddSingleton<ITextChunkingService, TextChunkingService>();
            })
            .Build();

        await host.RunAsync();
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Data.Tables;
using WeatherImageApp.Services;
using WeatherImageApp.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<ApiKeyAuthMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;
        var storageConnectionString = configuration["AzureWebJobsStorage"];

        // Register Azure Storage clients
        services.AddSingleton(new BlobServiceClient(storageConnectionString));
        services.AddSingleton(new QueueServiceClient(storageConnectionString));
        services.AddSingleton(new TableServiceClient(storageConnectionString));

        // Register HttpClient for external API calls with typed clients
        services.AddHttpClient<IWeatherService, WeatherService>();
        services.AddHttpClient<IImageService, ImageService>();

        // Register other services
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<ITableStorageService, TableStorageService>();
    })
    .Build();

host.Run();

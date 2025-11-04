using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;

namespace WeatherImageApp.Functions;

public class JobInitiatorFunction
{
    private readonly ILogger<JobInitiatorFunction> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IQueueService _queueService;
    private readonly ITableStorageService _tableStorageService;

    public JobInitiatorFunction(
        ILogger<JobInitiatorFunction> logger,
        IWeatherService weatherService,
        IQueueService queueService,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _weatherService = weatherService;
        _queueService = queueService;
        _tableStorageService = tableStorageService;
    }

    [Function("JobInitiator")]
    public async Task Run(
        [QueueTrigger("job-start-queue", Connection = "AzureWebJobsStorage")] string jobMessage)
    {
        _logger.LogInformation($"Job initiator triggered with message: {jobMessage}");

        try
        {
            // Parse the job message
            var jobData = JsonSerializer.Deserialize<JobStartMessage>(jobMessage);
            if (jobData == null)
            {
                _logger.LogError("Failed to deserialize job message");
                return;
            }

            _logger.LogInformation($"Processing job: {jobData.JobId}");

            // Fetch weather stations
            var stations = await _weatherService.GetWeatherStationsAsync();
            _logger.LogInformation($"Found {stations.Count} weather stations for job {jobData.JobId}");

            // Update job with total stations count
            await _tableStorageService.CreateJobAsync(jobData.JobId, stations.Count);

            // Fan out: Enqueue a message for each weather station
            foreach (var station in stations)
            {
                var imageMessage = new ImageProcessingMessage
                {
                    JobId = jobData.JobId,
                    StationId = station.StationId,
                    StationName = station.StationName
                };

                var messageJson = JsonSerializer.Serialize(imageMessage);
                await _queueService.EnqueueMessageAsync("image-processing-queue", messageJson);
            }

            _logger.LogInformation($"Job {jobData.JobId}: Enqueued {stations.Count} image processing tasks");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in job initiator");
            throw; // Re-throw to trigger retry mechanism
        }
    }
}

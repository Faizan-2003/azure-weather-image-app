using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;
using WeatherImageApp.Services.Interfaces;

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
        JobStartMessage? jobData = null;
        var startTime = DateTime.UtcNow;

        try
        {
            // Parse the job message
            jobData = JsonSerializer.Deserialize<JobStartMessage>(jobMessage);
            if (jobData == null || string.IsNullOrEmpty(jobData.JobId))
            {
                _logger.LogError("Invalid job message format: {JobMessage}", jobMessage);
                return; // Don't retry invalid messages
            }

            _logger.LogInformation(
                "Job initiator started for job {JobId}",
                jobData.JobId
            );

            // Fetch weather stations
            List<WeatherStation> stations;
            try
            {
                stations = await _weatherService.GetWeatherStationsAsync();
                
                _logger.LogInformation(
                    "Found {StationCount} weather stations for job {JobId}",
                    stations.Count,
                    jobData.JobId
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch weather stations from Buienradar API for job {JobId}. Will retry.",
                    jobData.JobId
                );
                throw; // Retry on network errors
            }

            if (stations.Count == 0)
            {
                _logger.LogWarning(
                    "No weather stations found for job {JobId}",
                    jobData.JobId
                );
                return; // Don't retry if no stations
            }

            // Update job with total stations count
            try
            {
                await _tableStorageService.CreateJobAsync(jobData.JobId, stations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create job record in table storage for job {JobId}. Will retry.",
                    jobData.JobId
                );
                throw; // Retry on storage errors
            }

            // Fan out: Enqueue a message for each weather station
            int enqueuedCount = 0;
            var failedStations = new List<string>();

            foreach (var station in stations)
            {
                try
                {
                    var imageMessage = new ImageProcessingMessage
                    {
                        JobId = jobData.JobId,
                        StationId = station.StationId,
                        StationName = station.StationName
                    };

                    var messageJson = JsonSerializer.Serialize(imageMessage);
                    await _queueService.EnqueueMessageAsync("image-processing-queue", messageJson);
                    enqueuedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to enqueue message for station {StationName} (ID: {StationId}) in job {JobId}",
                        station.StationName,
                        station.StationId,
                        jobData.JobId
                    );
                    failedStations.Add(station.StationName ?? station.StationId ?? "unknown");
                }
            }

            if (failedStations.Count > 0)
            {
                _logger.LogWarning(
                    "Job {JobId}: Failed to enqueue {FailedCount} stations: {FailedStations}",
                    jobData.JobId,
                    failedStations.Count,
                    string.Join(", ", failedStations)
                );

                if (enqueuedCount == 0)
                {
                    // All failed - throw to retry
                    throw new Exception($"Failed to enqueue all {stations.Count} messages");
                }
            }

            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Job {JobId}: Successfully enqueued {EnqueuedCount}/{TotalCount} image processing tasks. Processing time: {ProcessingTime}ms",
                jobData.JobId,
                enqueuedCount,
                stations.Count,
                processingTime.TotalMilliseconds
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize job message. Message: {JobMessage}",
                jobMessage
            );
            // Don't retry invalid JSON
            return;
        }
        catch (Exception ex)
        {
            var jobId = jobData?.JobId ?? "unknown";
            
            _logger.LogError(
                ex,
                "Fatal error in job initiator for job {JobId}. Message will be retried.",
                jobId
            );
            
            // Try to log error in table storage
            if (jobData != null && !string.IsNullOrEmpty(jobData.JobId))
            {
                try
                {
                    await _tableStorageService.UpdateJobErrorAsync(jobData.JobId, $"Job initiation failed: {ex.Message}");
                }
                catch
                {
                    // Ignore errors when logging the error
                }
            }
            
            throw; // Let Azure Functions retry mechanism handle it
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Functions;

public class PoisonQueueMonitorFunction
{
    private readonly ILogger<PoisonQueueMonitorFunction> _logger;
    private readonly ITableStorageService _tableStorageService;

    public PoisonQueueMonitorFunction(
        ILogger<PoisonQueueMonitorFunction> logger,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    [Function("PoisonQueueMonitor")]
    public async Task Run(
        [QueueTrigger("image-processing-queue-poison", Connection = "AzureWebJobsStorage")] string messageText)
    {
        _logger.LogError(
            "Message moved to poison queue after {MaxRetries} failed attempts. Message: {MessageText}",
            3, // maxDequeueCount from host.json
            messageText
        );

        try
        {
            // Try to parse the message to get job details
            var message = JsonSerializer.Deserialize<ImageProcessingMessage>(messageText);
            
            if (message != null && !string.IsNullOrEmpty(message.JobId))
            {
                _logger.LogError(
                    "Failed to process image for station {StationName} (ID: {StationId}) in job {JobId} after maximum retries",
                    message.StationName,
                    message.StationId,
                    message.JobId
                );

                // Update job status to indicate failure
                await _tableStorageService.UpdateJobErrorAsync(
                    message.JobId,
                    $"Station {message.StationName} failed after {3} retry attempts"
                );

                _logger.LogInformation(
                    "Updated job {JobId} with error information for failed station {StationName}",
                    message.JobId,
                    message.StationName
                );
            }
            else
            {
                _logger.LogError("Could not parse poison queue message: {MessageText}", messageText);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize poison queue message: {MessageText}",
                messageText
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while processing poison queue message: {MessageText}",
                messageText
            );
            // Don't throw - we don't want to retry poison queue processing
        }

        // Log to Application Insights for alerting
        _logger.LogCritical(
            "ALERT: Message in poison queue requires manual investigation: {MessageText}",
            messageText
        );
    }
}

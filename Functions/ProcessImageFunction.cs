using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Functions;

public class ProcessImageFunction
{
    private readonly ILogger<ProcessImageFunction> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IImageService _imageService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITableStorageService _tableStorageService;

    public ProcessImageFunction(
        ILogger<ProcessImageFunction> logger,
        IWeatherService weatherService,
        IImageService imageService,
        IBlobStorageService blobStorageService,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _weatherService = weatherService;
        _imageService = imageService;
        _blobStorageService = blobStorageService;
        _tableStorageService = tableStorageService;
    }

    [Function("ProcessImage")]
    public async Task Run(
        [QueueTrigger("image-processing-queue", Connection = "AzureWebJobsStorage")] string messageText)
    {
        ImageProcessingMessage? message = null;
        var startTime = DateTime.UtcNow;

        try
        {
            // Deserialize message
            message = JsonSerializer.Deserialize<ImageProcessingMessage>(messageText);
            
            if (message == null || string.IsNullOrEmpty(message.JobId) || string.IsNullOrEmpty(message.StationId))
            {
                _logger.LogError("Invalid message format: {MessageText}", messageText);
                return; // Don't retry invalid messages
            }

            // Structured logging with job context
            _logger.LogInformation(
                "Processing image for station {StationName} (ID: {StationId}) in job {JobId}",
                message.StationName,
                message.StationId,
                message.JobId
            );

            // Get weather data
            WeatherStation? weatherData = null;
            try
            {
                weatherData = await _weatherService.GetWeatherDataAsync(message.StationId);
                
                if (weatherData == null)
                {
                    _logger.LogWarning(
                        "Weather data not found for station {StationId} in job {JobId}",
                        message.StationId,
                        message.JobId
                    );
                    return; // Don't retry if station doesn't exist
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch weather data from Buienradar API for station {StationId} in job {JobId}. Will retry.",
                    message.StationId,
                    message.JobId
                );
                throw; // Retry on network errors
            }

            // Generate image with weather data
            Stream? imageStream = null;
            try
            {
                imageStream = await _imageService.GenerateWeatherImageAsync(weatherData);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch background image from Unsplash API for station {StationName} in job {JobId}. Will retry.",
                    message.StationName,
                    message.JobId
                );
                throw; // Retry on network errors
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate image for station {StationName} in job {JobId}",
                    message.StationName,
                    message.JobId
                );
                throw; // Retry on image generation errors
            }

            using (imageStream)
            {
                // Upload to blob storage
                string blobName;
                try
                {
                    blobName = await _blobStorageService.UploadImageAsync(
                        message.JobId, 
                        weatherData.StationName ?? "unknown", 
                        imageStream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to upload image to blob storage for station {StationName} in job {JobId}. Will retry.",
                        message.StationName,
                        message.JobId
                    );
                    throw; // Retry on storage errors
                }

                // Get SAS URL
                string imageUrl;
                try
                {
                    imageUrl = await _blobStorageService.GetBlobSasUrlAsync(blobName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to generate SAS URL for blob {BlobName} in job {JobId}",
                        blobName,
                        message.JobId
                    );
                    throw; // Retry on SAS generation errors
                }

                // Update job progress
                var imageInfo = new ImageInfo
                {
                    StationName = weatherData.StationName,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    await _tableStorageService.UpdateJobProgressAsync(message.JobId, imageInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update job progress in table storage for job {JobId}, station {StationName}",
                        message.JobId,
                        message.StationName
                    );
                    throw; // Retry on table storage errors
                }
            }

            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Successfully processed image for station {StationName} in job {JobId}. Processing time: {ProcessingTime}ms",
                message.StationName,
                message.JobId,
                processingTime.TotalMilliseconds
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize message. Message: {MessageText}",
                messageText
            );
            // Don't retry invalid JSON
            return;
        }
        catch (Exception ex)
        {
            var jobId = message?.JobId ?? "unknown";
            var stationName = message?.StationName ?? "unknown";
            
            _logger.LogError(
                ex,
                "Fatal error processing image for station {StationName} in job {JobId}. Message will be retried.",
                stationName,
                jobId
            );
            
            // Try to update job status to indicate error (but don't fail if this fails)
            if (message != null && !string.IsNullOrEmpty(message.JobId))
            {
                try
                {
                    await _tableStorageService.UpdateJobErrorAsync(message.JobId, $"Error processing {stationName}: {ex.Message}");
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

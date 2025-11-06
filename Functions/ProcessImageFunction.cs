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
        _logger.LogInformation($"Processing message: {messageText}");

        try
        {
            // Deserialize message
            var message = JsonSerializer.Deserialize<ImageProcessingMessage>(messageText);
            
            if (message == null || string.IsNullOrEmpty(message.JobId) || string.IsNullOrEmpty(message.StationId))
            {
                _logger.LogError("Invalid message format");
                return;
            }

            _logger.LogInformation($"Processing image for job {message.JobId}, station {message.StationName}");

            // Get weather data
            var weatherData = await _weatherService.GetWeatherDataAsync(message.StationId);
            
            if (weatherData == null)
            {
                _logger.LogError($"Weather data not found for station {message.StationId}");
                return;
            }

            // Generate image with weather data
            using var imageStream = await _imageService.GenerateWeatherImageAsync(weatherData);

            // Upload to blob storage
            var blobName = await _blobStorageService.UploadImageAsync(
                message.JobId, 
                weatherData.StationName ?? "unknown", 
                imageStream);

            // Get SAS URL
            var imageUrl = await _blobStorageService.GetBlobSasUrlAsync(blobName);

            // Update job progress
            var imageInfo = new ImageInfo
            {
                StationName = weatherData.StationName,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _tableStorageService.UpdateJobProgressAsync(message.JobId, imageInfo);

            _logger.LogInformation($"Successfully processed image for station {message.StationName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            throw; // Let Azure Functions retry mechanism handle it
        }
    }
}

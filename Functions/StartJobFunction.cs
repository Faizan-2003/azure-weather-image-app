using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;

namespace WeatherImageApp.Functions;

public class StartJobFunction
{
    private readonly ILogger<StartJobFunction> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IQueueService _queueService;
    private readonly ITableStorageService _tableStorageService;

    public StartJobFunction(
        ILogger<StartJobFunction> logger,
        IWeatherService weatherService,
        IQueueService queueService,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _weatherService = weatherService;
        _queueService = queueService;
        _tableStorageService = tableStorageService;
    }

    [Function("StartJob")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "job/start")] HttpRequestData req)
    {
        _logger.LogInformation("Starting new weather image job");

        try
        {
            // Generate unique job ID
            var jobId = Guid.NewGuid().ToString();

            // Fetch weather stations
            var stations = await _weatherService.GetWeatherStationsAsync();
            _logger.LogInformation($"Found {stations.Count} weather stations");

            // Create job record in Table Storage
            await _tableStorageService.CreateJobAsync(jobId, stations.Count);

            // Enqueue messages for each station
            foreach (var station in stations)
            {
                var message = new ImageProcessingMessage
                {
                    JobId = jobId,
                    StationId = station.StationId,
                    StationName = station.StationName
                };

                var messageJson = JsonSerializer.Serialize(message);
                await _queueService.EnqueueMessageAsync("image-processing-queue", messageJson);
            }

            _logger.LogInformation($"Job {jobId} created with {stations.Count} stations");

            // Return response
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new StartJobResponse
            {
                JobId = jobId,
                Status = "InProgress",
                Message = $"Job started with {stations.Count} stations"
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting job");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}

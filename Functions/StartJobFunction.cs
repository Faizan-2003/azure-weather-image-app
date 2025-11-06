using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;
using WeatherImageApp.Services.Interfaces;

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

            _logger.LogInformation($"Creating job {jobId}");

            // Create initial job entry in table storage
            await _tableStorageService.CreateJobAsync(jobId, 0);

            // Enqueue job start message to job-start-queue
            // The JobInitiatorFunction will fetch stations and fan out to image-processing-queue
            var jobStartMessage = new JobStartMessage
            {
                JobId = jobId
            };

            var messageJson = JsonSerializer.Serialize(jobStartMessage);
            await _queueService.EnqueueMessageAsync("job-start-queue", messageJson);

            _logger.LogInformation($"Job {jobId} enqueued to job-start-queue");

            // Return response
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new StartJobResponse
            {
                JobId = jobId,
                Status = "Queued",
                Message = "Job has been queued for processing"
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

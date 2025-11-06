using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Functions;

public class GetJobStatusFunction
{
    private readonly ILogger<GetJobStatusFunction> _logger;
    private readonly ITableStorageService _tableStorageService;

    public GetJobStatusFunction(
        ILogger<GetJobStatusFunction> logger,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    [Function("GetJobStatus")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "job/{jobId}")] HttpRequestData req,
        string jobId)
    {
        _logger.LogInformation($"Getting status for job {jobId}");

        try
        {
            var jobEntity = await _tableStorageService.GetJobAsync(jobId);

            if (jobEntity == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Job {jobId} not found");
                return notFoundResponse;
            }

            // Deserialize images
            var images = JsonSerializer.Deserialize<List<ImageInfo>>(jobEntity.ImagesJson) ?? new List<ImageInfo>();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new JobStatusResponse
            {
                JobId = jobId,
                Status = jobEntity.Status,
                TotalStations = jobEntity.TotalStations,
                ProcessedStations = jobEntity.ProcessedStations,
                Images = images
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting job status for {jobId}");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}

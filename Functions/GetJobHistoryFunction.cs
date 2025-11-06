using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WeatherImageApp.Services;
using WeatherImageApp.Middleware;

namespace WeatherImageApp.Functions;

public class GetJobHistoryFunction
{
    private readonly ILogger<GetJobHistoryFunction> _logger;
    private readonly ITableStorageService _tableStorageService;

    public GetJobHistoryFunction(
        ILogger<GetJobHistoryFunction> logger,
        ITableStorageService tableStorageService)
    {
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    [Function("GetJobHistory")]
    [ApiKeyAuth]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobs/history")] HttpRequestData req)
    {
        _logger.LogInformation("Getting job history");

        try
        {
            var jobs = await _tableStorageService.GetAllJobsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(jobs);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job history");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WeatherImageApp.Services;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Functions;

public class TestImageProcessingFunction
{
    private readonly ILogger<TestImageProcessingFunction> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IImageService _imageService;

    public TestImageProcessingFunction(
        ILogger<TestImageProcessingFunction> logger,
        IWeatherService weatherService,
        IImageService imageService)
    {
        _logger = logger;
        _weatherService = weatherService;
        _imageService = imageService;
    }

    [Function("TestImageProcessing")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test/image")] HttpRequestData req)
    {
        _logger.LogInformation("Testing image processing");

        try
        {
            // Get first weather station
            var stations = await _weatherService.GetWeatherStationsAsync();
            var firstStation = stations.FirstOrDefault();

            if (firstStation == null)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await errorResponse.WriteStringAsync("No weather stations found");
                return errorResponse;
            }

            // Generate image
            using var imageStream = await _imageService.GenerateWeatherImageAsync(firstStation);

            // Return image as response
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "image/jpeg");
            
            await imageStream.CopyToAsync(response.Body);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing image processing");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}

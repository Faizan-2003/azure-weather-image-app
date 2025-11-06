using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureWeatherImageApp.Functions
{
    public class StaticWebsiteFunction
    {
        private readonly ILogger<StaticWebsiteFunction> _logger;

        public StaticWebsiteFunction(ILogger<StaticWebsiteFunction> logger)
        {
            _logger = logger;
        }

        [Function("ServeWebsite")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Serving static website");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

            // Read the HTML file
            var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            
            if (File.Exists(htmlPath))
            {
                var htmlContent = await File.ReadAllTextAsync(htmlPath);
                await response.WriteStringAsync(htmlContent);
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync("Website not found");
            }

            return response;
        }
    }
}

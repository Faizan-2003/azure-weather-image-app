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

        [Function("HomePage")]
        public async Task<HttpResponseData> HomePage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "")] HttpRequestData req)
        {
            _logger.LogInformation("Serving homepage at root");
            return await ServeHtmlPage(req);
        }
        
        [Function("ServeWebsite")]
        public async Task<HttpResponseData> ServeWebsite(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ServeWebsite")] HttpRequestData req)
        {
            _logger.LogInformation("ServeWebsite function called");
            return await ServeHtmlPage(req);
        }

        private async Task<HttpResponseData> ServeHtmlPage(HttpRequestData req)
        {

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

            // Read the HTML file - try multiple paths for Azure compatibility
            var paths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"),
                Path.Combine(Environment.CurrentDirectory, "wwwroot", "index.html"),
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html"),
                "wwwroot/index.html"
            };

            string? htmlContent = null;
            string? foundPath = null;

            foreach (var path in paths)
            {
                _logger.LogInformation($"Trying path: {path}");
                if (File.Exists(path))
                {
                    htmlContent = await File.ReadAllTextAsync(path);
                    foundPath = path;
                    break;
                }
            }
            
            if (htmlContent != null)
            {
                _logger.LogInformation($"Successfully loaded HTML from: {foundPath}");
                await response.WriteStringAsync(htmlContent);
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var currentDir = Directory.GetCurrentDirectory();
                var appDir = AppContext.BaseDirectory;
                await response.WriteStringAsync($"Website not found. Tried paths: {string.Join(", ", paths)}. Current: {currentDir}, App: {appDir}");
            }

            return response;
        }
    }
}

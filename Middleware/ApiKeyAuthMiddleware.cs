using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace WeatherImageApp.Middleware;

public class ApiKeyAuthAttribute : Attribute
{
}

public class ApiKeyAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthMiddleware(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestData = await context.GetHttpRequestDataAsync();
        
        if (requestData != null)
        {
            // Check if the function should skip authentication (web interface functions and health check)
            var targetMethod = context.FunctionDefinition.Name;
            var skipAuth = targetMethod == "ServeWebsite" 
                        || targetMethod == "HomePage" 
                        || targetMethod == "HealthCheck";
            
            if (!skipAuth)
            {
                // Get API key from configuration
                var expectedApiKey = _configuration["ApiKey"];
                
                if (!string.IsNullOrEmpty(expectedApiKey))
                {
                    // Get API key from header first
                    var providedApiKey = requestData.Headers.TryGetValues("X-API-Key", out var values) 
                        ? values.FirstOrDefault() 
                        : null;

                    // If not in header, check query parameter
                    if (string.IsNullOrEmpty(providedApiKey))
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(requestData.Url.Query);
                        providedApiKey = query["apiKey"];
                    }

                    if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != expectedApiKey)
                    {
                        var response = requestData.CreateResponse(HttpStatusCode.Unauthorized);
                        await response.WriteStringAsync("Unauthorized: Invalid or missing API key. Provide 'X-API-Key' header or '?apiKey=' query parameter.");
                        
                        context.GetInvocationResult().Value = response;
                        return;
                    }
                }
            }
        }

        await next(context);
    }
}

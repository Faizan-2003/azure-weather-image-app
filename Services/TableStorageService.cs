using Azure.Data.Tables;
using System.Text.Json;
using WeatherImageApp.Models;
using WeatherImageApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace WeatherImageApp.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageService> _logger;
    private const string TableName = "JobStatus";

    public TableStorageService(
        TableServiceClient tableServiceClient,
        ILogger<TableStorageService> logger)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    private async Task<TableClient> GetTableClientAsync()
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();
        return tableClient;
    }

    public async Task CreateJobAsync(string jobId, int totalStations)
    {
        var tableClient = await GetTableClientAsync();

        var entity = new JobStatusEntity
        {
            PartitionKey = jobId,
            RowKey = "status",
            CreatedAt = DateTime.UtcNow,
            Status = "InProgress",
            TotalStations = totalStations,
            ProcessedStations = 0,
            ImagesJson = "[]"
        };

        // Use UpsertEntity to handle both create and update scenarios
        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
    }

    public async Task<JobStatusEntity?> GetJobAsync(string jobId)
    {
        var tableClient = await GetTableClientAsync();
        
        try
        {
            var response = await tableClient.GetEntityAsync<JobStatusEntity>(jobId, "status");
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpdateJobProgressAsync(string jobId, ImageInfo imageInfo)
    {
        var tableClient = await GetTableClientAsync();
        
        var entity = await GetJobAsync(jobId);
        if (entity == null)
        {
            throw new Exception($"Job {jobId} not found");
        }

        // Deserialize existing images
        var images = JsonSerializer.Deserialize<List<ImageInfo>>(entity.ImagesJson) ?? new List<ImageInfo>();
        
        // Add new image
        images.Add(imageInfo);
        
        // Update entity
        entity.ProcessedStations++;
        entity.ImagesJson = JsonSerializer.Serialize(images);
        
        // Mark as completed if all stations are processed
        if (entity.ProcessedStations >= entity.TotalStations)
        {
            entity.Status = "Completed";
        }

        await tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
    }

    public async Task<List<JobStatusEntity>> GetAllJobsAsync()
    {
        var tableClient = await GetTableClientAsync();

        var jobs = new List<JobStatusEntity>();
        
        await foreach (var entity in tableClient.QueryAsync<JobStatusEntity>())
        {
            jobs.Add(entity);
        }

        // Sort by creation time, most recent first
        return jobs.OrderByDescending(j => j.CreatedAt).ToList();
    }

    public async Task UpdateJobErrorAsync(string jobId, string errorMessage)
    {
        try
        {
            var tableClient = await GetTableClientAsync();
            var entity = await tableClient.GetEntityAsync<JobStatusEntity>("JobStatus", jobId);

            if (entity.Value != null)
            {
                var job = entity.Value;
                
                // Store error in a new property (optional - for tracking)
                if (job.ETag != default)
                {
                    // Update the entity to mark it as having errors
                    // You could add an Errors property to JobStatusEntity if needed
                    // For now, we'll just log it without modifying the entity
                    _logger.LogWarning(
                        "Error recorded for job {JobId}: {ErrorMessage}",
                        jobId,
                        errorMessage
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job error for job {JobId}", jobId);
            // Don't throw - this is a best-effort operation
        }
    }
}

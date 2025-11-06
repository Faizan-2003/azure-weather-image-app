using Azure.Data.Tables;
using System.Text.Json;
using WeatherImageApp.Models;

namespace WeatherImageApp.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private const string TableName = "JobStatus";

    public TableStorageService(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task CreateJobAsync(string jobId, int totalStations)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();

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
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        
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
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        
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
}

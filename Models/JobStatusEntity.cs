using Azure;
using Azure.Data.Tables;

namespace WeatherImageApp.Models;

public class JobStatusEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = "status";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "InProgress";
    public int TotalStations { get; set; }
    public int ProcessedStations { get; set; }
    public string ImagesJson { get; set; } = "[]";
}

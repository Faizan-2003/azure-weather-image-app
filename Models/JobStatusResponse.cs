namespace WeatherImageApp.Models;

public class JobStatusResponse
{
    public string? JobId { get; set; }
    public string? Status { get; set; }
    public int TotalStations { get; set; }
    public int ProcessedStations { get; set; }
    public List<ImageInfo> Images { get; set; } = new();
}

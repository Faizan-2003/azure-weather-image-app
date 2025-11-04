using WeatherImageApp.Models;

namespace WeatherImageApp.Services;

public interface ITableStorageService
{
    Task CreateJobAsync(string jobId, int totalStations);
    Task<JobStatusEntity?> GetJobAsync(string jobId);
    Task UpdateJobProgressAsync(string jobId, ImageInfo imageInfo);
}

using WeatherImageApp.Models;

namespace WeatherImageApp.Services.Interfaces;

public interface ITableStorageService
{
    Task CreateJobAsync(string jobId, int totalStations);
    Task<JobStatusEntity?> GetJobAsync(string jobId);
    Task UpdateJobProgressAsync(string jobId, ImageInfo imageInfo);
    Task<List<JobStatusEntity>> GetAllJobsAsync();
    Task UpdateJobErrorAsync(string jobId, string errorMessage);
}

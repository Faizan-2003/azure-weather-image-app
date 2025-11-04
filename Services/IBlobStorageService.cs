namespace WeatherImageApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(string jobId, string stationName, Stream imageStream);
    Task<string> GetBlobSasUrlAsync(string blobName);
}

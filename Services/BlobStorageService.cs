using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace WeatherImageApp.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "weather-images";

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadImageAsync(string jobId, string stationName, Stream imageStream)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        // Create container if it doesn't exist
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        // Create blob name: jobId/stationName.jpg
        var sanitizedStationName = SanitizeFileName(stationName);
        var blobName = $"{jobId}/{sanitizedStationName}.jpg";
        
        var blobClient = containerClient.GetBlobClient(blobName);
        
        // Upload the image
        await blobClient.UploadAsync(imageStream, new BlobHttpHeaders
        {
            ContentType = "image/jpeg"
        });

        return blobName;
    }

    public Task<string> GetBlobSasUrlAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        // Check if we can generate SAS tokens (requires account key)
        if (_blobServiceClient.CanGenerateAccountSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = ContainerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return Task.FromResult(sasUri.ToString());
        }
        else
        {
            // Fallback to regular URL (for development with Azurite)
            return Task.FromResult(blobClient.Uri.ToString());
        }
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return sanitized.Replace(" ", "_");
    }
}

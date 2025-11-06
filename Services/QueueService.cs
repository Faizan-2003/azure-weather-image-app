using Azure.Storage.Queues;
using System.Text;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Services;

public class QueueService : IQueueService
{
    private readonly QueueServiceClient _queueServiceClient;

    public QueueService(QueueServiceClient queueServiceClient)
    {
        _queueServiceClient = queueServiceClient;
    }

    public async Task EnqueueMessageAsync(string queueName, string message)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        
        // Create queue if it doesn't exist
        await queueClient.CreateIfNotExistsAsync();
        
        // Encode message as Base64 to handle special characters
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var base64Message = Convert.ToBase64String(messageBytes);
        
        await queueClient.SendMessageAsync(base64Message);
    }
}

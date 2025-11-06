namespace WeatherImageApp.Services.Interfaces;

public interface IQueueService
{
    Task EnqueueMessageAsync(string queueName, string message);
}

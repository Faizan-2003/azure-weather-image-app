namespace WeatherImageApp.Services;

public interface IQueueService
{
    Task EnqueueMessageAsync(string queueName, string message);
}

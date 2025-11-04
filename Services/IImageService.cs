using WeatherImageApp.Models;

namespace WeatherImageApp.Services;

public interface IImageService
{
    Task<Stream> GenerateWeatherImageAsync(WeatherStation weatherData);
}

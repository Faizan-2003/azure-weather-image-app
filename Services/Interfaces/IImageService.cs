using WeatherImageApp.Models;

namespace WeatherImageApp.Services.Interfaces;

public interface IImageService
{
    Task<Stream> GenerateWeatherImageAsync(WeatherStation weatherData);
}

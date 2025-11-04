using WeatherImageApp.Models;

namespace WeatherImageApp.Services;

public interface IWeatherService
{
    Task<List<WeatherStation>> GetWeatherStationsAsync();
    Task<WeatherStation?> GetWeatherDataAsync(string stationId);
}

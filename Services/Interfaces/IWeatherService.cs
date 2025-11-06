using WeatherImageApp.Models;

namespace WeatherImageApp.Services.Interfaces;

public interface IWeatherService
{
    Task<List<WeatherStation>> GetWeatherStationsAsync();
    Task<WeatherStation?> GetWeatherDataAsync(string stationId);
}

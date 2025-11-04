using System.Text.Json;
using WeatherImageApp.Models;

namespace WeatherImageApp.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private const string BuienradarApiUrl = "https://data.buienradar.nl/2.0/feed/json";

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<WeatherStation>> GetWeatherStationsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(BuienradarApiUrl);
            var jsonDoc = JsonDocument.Parse(response);
            
            var stations = new List<WeatherStation>();
            
            if (jsonDoc.RootElement.TryGetProperty("actual", out var actual) &&
                actual.TryGetProperty("stationmeasurements", out var stationMeasurements))
            {
                foreach (var station in stationMeasurements.EnumerateArray().Take(50))
                {
                    // Get stationid - it might be a string or number
                    string stationId;
                    if (station.TryGetProperty("stationid", out var stationIdProp))
                    {
                        stationId = stationIdProp.ValueKind == JsonValueKind.Number 
                            ? stationIdProp.GetInt32().ToString() 
                            : stationIdProp.GetString() ?? "unknown";
                    }
                    else
                    {
                        continue; // Skip stations without ID
                    }
                    
                    var weatherStation = new WeatherStation
                    {
                        StationId = stationId,
                        StationName = station.TryGetProperty("stationname", out var name) ? name.GetString() : "Unknown",
                        Temperature = station.TryGetProperty("temperature", out var temp) && temp.ValueKind == JsonValueKind.Number ? temp.GetDouble() : null,
                        WeatherDescription = station.TryGetProperty("weatherdescription", out var desc) ? desc.GetString() : "N/A",
                        Humidity = station.TryGetProperty("humidity", out var hum) && hum.ValueKind == JsonValueKind.Number ? (int?)hum.GetDouble() : null,
                        WindSpeed = station.TryGetProperty("windspeed", out var wind) && wind.ValueKind == JsonValueKind.Number ? wind.GetDouble() : null
                    };
                    
                    stations.Add(weatherStation);
                }
            }
            
            return stations;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch weather stations: {ex.Message}", ex);
        }
    }

    public async Task<WeatherStation?> GetWeatherDataAsync(string stationId)
    {
        var stations = await GetWeatherStationsAsync();
        return stations.FirstOrDefault(s => s.StationId == stationId);
    }
}

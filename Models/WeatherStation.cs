namespace WeatherImageApp.Models;

public class WeatherStation
{
    public string? StationId { get; set; }
    public string? StationName { get; set; }
    public double? Temperature { get; set; }
    public string? WeatherDescription { get; set; }
    public int? Humidity { get; set; }
    public double? WindSpeed { get; set; }
}

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using Microsoft.Extensions.Configuration;
using WeatherImageApp.Models;
using System.Text.Json;
using WeatherImageApp.Services.Interfaces;

namespace WeatherImageApp.Services;

public class ImageService : IImageService
{
    private readonly HttpClient _httpClient;
    private readonly string? _unsplashAccessKey;

    public ImageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _unsplashAccessKey = configuration["UnsplashAccessKey"];
    }

    public async Task<Stream> GenerateWeatherImageAsync(WeatherStation weatherData)
    {
        // Get a background image
        var backgroundStream = await GetBackgroundImageAsync();
        
        using var image = await Image.LoadAsync<Rgba32>(backgroundStream);
        
        // Prepare text to overlay - handle null values
        var tempText = weatherData.Temperature.HasValue ? $"{weatherData.Temperature.Value:F1}°C" : "N/A";
        var humidityText = weatherData.Humidity.HasValue ? $"{weatherData.Humidity.Value}%" : "N/A";
        var windText = weatherData.WindSpeed.HasValue ? $"{weatherData.WindSpeed.Value:F1} m/s" : "N/A";
        
        var weatherText = $"{weatherData.StationName}\n" +
                         $"Temperature: {tempText}\n" +
                         $"Weather: {weatherData.WeatherDescription}\n" +
                         $"Humidity: {humidityText}\n" +
                         $"Wind Speed: {windText}";

        // Create font - using built-in system fonts
        FontFamily fontFamily;
        var preferredFont = SystemFonts.Families.FirstOrDefault(f => 
            f.Name.Contains("Arial", StringComparison.OrdinalIgnoreCase) || 
            f.Name.Contains("Segoe", StringComparison.OrdinalIgnoreCase) || 
            f.Name.Contains("Sans", StringComparison.OrdinalIgnoreCase));
        
        fontFamily = preferredFont.Name != null ? preferredFont : SystemFonts.Families.First();
        
        var font = fontFamily.CreateFont(32, FontStyle.Bold);
        var smallFont = fontFamily.CreateFont(24, FontStyle.Regular);

        // Add semi-transparent background for text readability
        image.Mutate(ctx =>
        {
            // Draw semi-transparent black rectangle for text background
            var textOptions = new RichTextOptions(font)
            {
                Origin = new PointF(30, 30),
                WrappingLength = image.Width - 60,
                TextAlignment = TextAlignment.Start
            };

            // Calculate text bounds for background
            var textSize = TextMeasurer.MeasureBounds(weatherText, textOptions);
            var padding = 20;
            var rect = new RectangleF(
                textOptions.Origin.X - padding,
                textOptions.Origin.Y - padding,
                textSize.Width + (padding * 2),
                textSize.Height + (padding * 2)
            );

            // Draw background
            ctx.Fill(new Color(new Rgba32(0, 0, 0, 180)), rect);

            // Draw text
            ctx.DrawText(weatherText, font, Color.White, new PointF(30, 30));
        });

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        outputStream.Position = 0;
        
        return outputStream;
    }

    private async Task<Stream> GetBackgroundImageAsync()
    {
        try
        {
            // Try to get from Unsplash if API key is available
            if (!string.IsNullOrEmpty(_unsplashAccessKey))
            {
                var unsplashUrl = $"https://api.unsplash.com/photos/random?query=nature,landscape&orientation=landscape";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_unsplashAccessKey}");
                
                var response = await _httpClient.GetStringAsync(unsplashUrl);
                var jsonDoc = JsonDocument.Parse(response);
                
                if (jsonDoc.RootElement.TryGetProperty("urls", out var urls) &&
                    urls.TryGetProperty("regular", out var imageUrl))
                {
                    var imageResponse = await _httpClient.GetAsync(imageUrl.GetString());
                    if (imageResponse.IsSuccessStatusCode)
                    {
                        return await imageResponse.Content.ReadAsStreamAsync();
                    }
                }
            }
        }
        catch
        {
            // Fall through to placeholder
        }

        // Fallback: Create a simple gradient background
        return CreatePlaceholderImage();
    }

    private Stream CreatePlaceholderImage()
    {
        var image = new Image<Rgba32>(1200, 800);
        
        var random = new Random();
        var color1 = new Rgba32((byte)random.Next(100, 200), (byte)random.Next(100, 200), (byte)random.Next(150, 255));
        var color2 = new Rgba32((byte)random.Next(50, 150), (byte)random.Next(50, 150), (byte)random.Next(100, 200));

        image.Mutate(ctx =>
        {
            // Create a gradient effect
            for (int y = 0; y < image.Height; y++)
            {
                float ratio = (float)y / image.Height;
                var r = (byte)(color1.R * (1 - ratio) + color2.R * ratio);
                var g = (byte)(color1.G * (1 - ratio) + color2.G * ratio);
                var b = (byte)(color1.B * (1 - ratio) + color2.B * ratio);
                var color = new Rgba32(r, g, b);

                ctx.Fill(color, new RectangleF(0, y, image.Width, 1));
            }
        });

        var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        stream.Position = 0;
        return stream;
    }
}

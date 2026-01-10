namespace WeatherBotApp;

public class Weather
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; }
    
    [JsonPropertyName("daily")]
    public DailyWeather Daily { get; set; }
}
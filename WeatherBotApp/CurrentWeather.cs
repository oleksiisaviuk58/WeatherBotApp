namespace WeatherBotApp;

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }
    
    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }
    
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}
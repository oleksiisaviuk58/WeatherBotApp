namespace WeatherTelegramBotApp;

public class DailyWeather
{
    [JsonPropertyName("time")]
    public List<string> Date { get; set; }
    
    [JsonPropertyName("temperature_2m_min")]
    public List<double> MinTemperature { get; set; }
    
    [JsonPropertyName("temperature_2m_max")]
    public List<double> MaxTemperature { get; set; }
    
    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; }
}
namespace WeatherTelegramBotApp;

public class GeoCode
{
    [JsonPropertyName("name")]
    public string City { get; set; }
    
    [JsonPropertyName("display_name")]
    public string Country { get; set; }
    
    [JsonPropertyName("lat")]
    public string Latitude { get; set; }
    
    [JsonPropertyName("lon")]
    public string Longitude { get; set; }
}
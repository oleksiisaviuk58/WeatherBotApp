namespace WeatherBotApp;

public class GeoCodeService
{
    public static async Task<GeoCode> Get(string city)
    {
        string url = $"https://nominatim.openstreetmap.org/search?q={city}&format=json&limit=1&accept-language=uk";
        var geo = await HttpClientService.Get<GeoCode[]>(url);

        return geo[0];
    }
    
    public static async Task<(string country, string city)> Get(double latitude, double longitude)
    {
        string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json" +
                     $"&accept-language=uk";
        
        var json = await HttpClientService.GetJson(url);
        using var reader = JsonDocument.Parse(json);
        
        var city = reader.RootElement.GetProperty("address").GetProperty("city").GetString();
        var country = reader.RootElement.GetProperty("address").GetProperty("country").GetString();
        
        return (country!, city!);
    }
    
    public static bool TryParseCoordinates(string? text, out double latitude, out double longitude)
    {
        latitude = longitude = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var parts = text.Split(',', ' ');
        if (parts.Length < 2)
            return false;

        return double.TryParse(parts[0], out latitude) && double.TryParse(parts[1], out longitude);
    }
}
namespace WeatherBotApp;

public class GeoCodeService
{
    public static async Task<GeoCode> Get(string city)
    {
        string url = $"https://nominatim.openstreetmap.org/search?q={city}&format=json&limit=1&accept-language=uk";
        var geo = await HttpClientService.Get<GeoCode[]>(url);
        
        if (geo is null)
            return new GeoCode();

        return geo[0];
    }
    
    public static async Task<string> Get(double latitude, double longitude)
    {
        try
        {
            string url = $"https://nominatim.openstreetmap.org/reverse?" +
                         $"lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
                         $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}&format=json&accept-language=uk";

            var json = await HttpClientService.GetJson(url);
            using var reader = JsonDocument.Parse(json);
            var address = reader.RootElement.GetProperty("display_name").GetString();
            
            return address;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return default;
        }
    }
    
    public static bool TryParseCoordinates(string? text, out double latitude, out double longitude)
    {
        latitude = longitude = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;
        if (!Regex.IsMatch(text, "[0-9,.]"))
            return false;
        
        var parts = text.Split(',', ' ');
        if (parts.Length < 2)
            return false;

        return double.TryParse(parts[0], out latitude) && double.TryParse(parts[1], out longitude);
    }
}
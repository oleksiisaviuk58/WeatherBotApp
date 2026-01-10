namespace WeatherBotApp;

public class WeatherService
{
    public static async Task<string> GetFromCityName(GeoCode geoCode, int days)
    {
        var _days = GetDays(days);
        
        string url =
            $"https://api.open-meteo.com/v1/forecast?latitude={geoCode.Latitude}&longitude={geoCode.Longitude}" +
            $"&current=temperature_2m,apparent_temperature,cloud_cover,visibility,wind_direction_10m,weather_code" +
            $"&daily=temperature_2m_min,temperature_2m_max,weather_code" +
            $"&timezone=auto&start_date={_days.start}&end_date={_days.end}";
        var weather = await HttpClientService.Get<Weather>(url);
        
        var result = CreateMessage(weather!, geoCode.Country);
        
        return result;
    }
    
    public static async Task<string> GetFromCoordinates(double latitude, double longitude, int days)
    {
        var _days = GetDays(days);
        
        string url =
            $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}" +
            $"&current=temperature_2m,apparent_temperature,cloud_cover,visibility,wind_direction_10m,weather_code" +
            $"&daily=temperature_2m_min,temperature_2m_max,weather_code" +
            $"&timezone=auto&start_date={_days.start}&end_date={_days.end}";
        var weather = await HttpClientService.Get<Weather>(url);

        var temp = await GeoCodeService.Get(latitude, longitude);
        var result = CreateMessage(weather!, temp.city + ", " + temp.country);
        
        return result;
    }

    private static (string start, string end) GetDays(int days)
    {
        var start = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(days - 1)).ToString("yyyy-MM-dd");

        return (start, end);
    }

    private static string CreateMessage(Weather weather, string city)
    {
        string message = $"Місто: {city}\n" +
                         $"\nЗАРАЗ\nТемпература: {weather.Current.Temperature}°C\n" +
                         $"Відчувається як: {weather.Current.ApparentTemperature}°C\n";
        
        for (int i = 0; i < weather.Daily.Date.Count; i++)
        {
            message += $"\nДата: {weather.Daily.Date[i]}\n" +
                       $"Мінімальна температура: {weather.Daily.MinTemperature[i]}°C\n" +
                       $"Максимальна температура: {weather.Daily.MaxTemperature[i]}°C\n" +
                       $"Погода: {WeatherCodeDiscription.Map[weather.Daily.WeatherCode[i]]}\n";
        }

        return message;
    }
}
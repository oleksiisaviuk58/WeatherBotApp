namespace WeatherBotApp;

public class WeatherService
{
    public static async Task<string> GetFromCityName(GeoCode geoCode, int days)
    {
        var _days = GetDays(days);
        
        string url =
            $"https://api.open-meteo.com/v1/forecast?latitude={geoCode.Latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={geoCode.Longitude.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m," +
            $"apparent_temperature,weather_code&daily=temperature_2m_min,temperature_2m_max,weather_code&timezone=auto" +
            $"&start_date={_days.start}&end_date={_days.end}";
        var weather = await HttpClientService.Get<Weather>(url);
        
        if (weather is null) 
            return "Упппс, шось трапилось 🙄 Давай спочатку...";
        
        var result = CreateMessage(weather, geoCode.Country);
        return result;
    }
    
    public static async Task<string> GetFromCoordinates(double latitude, double longitude, int days)
    {
        var _days = GetDays(days);

        string url =
            $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m," +
            $"apparent_temperature,weather_code&daily=temperature_2m_min,temperature_2m_max,weather_code" +
            $"&timezone=auto&start_date={_days.start}&end_date={_days.end}";
        var weather = await HttpClientService.Get<Weather>(url);
        if (weather is null) 
            return "Упппс, шось трапилось 🙄 Давай спочатку...";

        var address = await GeoCodeService.Get(latitude, longitude);
        if (string.IsNullOrEmpty(address))
            return "Упппс, шось трапилось 🙄 Давай спочатку...";

        var result = CreateMessage(weather, address);
        return result;
    }

    private static (string start, string end) GetDays(int days)
    {
        var start = UrlFormat.Invariant(DateOnly.FromDateTime(DateTime.Today));
        
        if (days > 1)
        {
            var end = UrlFormat.Invariant(DateOnly.FromDateTime(DateTime.Today.AddDays(days - 1)));
            return (start, end);
        }
        
        return (start, start);
    }

    private static string CreateMessage(Weather weather, string city)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"📍 **{city}**");
        sb.AppendLine("━━━━━━━━━━━━━━━");
    
        sb.AppendLine("⏰ **ЗАРАЗ**");
        sb.AppendLine($"🌡 Температура: **{weather.Current.Temperature}°C**");
        sb.AppendLine($"🤒 Відчувається як: **{weather.Current.ApparentTemperature}°C**");
        sb.AppendLine($"🌥 Погода: **{WeatherCodeDiscription.Get(weather.Current.WeatherCode, true)}**");

        sb.AppendLine();
        sb.AppendLine("📅 **ПРОГНОЗ**");
        sb.AppendLine("━━━━━━━━━━━━━━━");

        for (int i = 0; i < weather.Daily.Date.Count; i++)
        {
            sb.AppendLine($"🗓 **{weather.Daily.Date[i]}**");
            sb.AppendLine($"🔻 Мін: {weather.Daily.MinTemperature[i]}°C");
            sb.AppendLine($"🔺 Макс: {weather.Daily.MaxTemperature[i]}°C");
            sb.AppendLine($"☁️ {WeatherCodeDiscription.Get(weather.Daily.WeatherCode[i], true)}");
            sb.AppendLine("────────────────");
        }

        sb.AppendLine("✨ Гарного дня та нехай погода буде на твоєму боці 😌");

        return sb.ToString();
    }
}
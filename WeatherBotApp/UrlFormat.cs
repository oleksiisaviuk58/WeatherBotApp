using System.Globalization;

namespace WeatherBotApp;

public class UrlFormat
{
    public static string Invariant(double value) => value.ToString(CultureInfo.CurrentCulture);

    public static string Invariant(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
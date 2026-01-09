namespace WeatherTelegramBotApp;

public class HttpClientService
{
    private static HttpClient _httpClient = new HttpClient();
    
    public static async Task<TKey> Get<TKey>(string url)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "application/json");
        var json = await _httpClient.GetStringAsync(url);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = JsonSerializer.Deserialize<TKey>(json, options);

        return response!;
    }
    
    public static async Task<string> GetJson(string url)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "application/json");
        var json = await _httpClient.GetStringAsync(url);

        return json;
    }
}
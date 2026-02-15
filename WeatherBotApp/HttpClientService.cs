namespace WeatherBotApp;

public class HttpClientService
{
    private static HttpClient _httpClient = new HttpClient
    {
        DefaultRequestHeaders = { { "User-Agent", "WeatherBotApp/1.0" } }
    };
    
    public static async Task<TKey> Get<TKey>(string url)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<TKey>(json, options);
            
            return response;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return default;
        }
    }
    
    public static async Task<string> GetJson(string url)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            return json;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return default;
        }
    }
}
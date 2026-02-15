namespace WeatherBotApp;

public class User
{
    public User(long chatId) => ChatId = chatId;
    
    public long ChatId { get; set; }
    
    public int Days { get; set; }
    
    public string City { get; set; }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
}
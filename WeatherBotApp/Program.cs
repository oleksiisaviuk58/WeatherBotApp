using var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;

using StringReader telegramTokenReader = new StringReader("/Users/alexsavyuk/RiderProjects/" +
                                                    "WeatherTelegramBotApp/WeatherBotApp/Token.txt");
var telegramToken = await telegramTokenReader.ReadLineAsync();

var bot = new TelegramBotClient(telegramToken!, cancellationToken: token);


try
{

}
catch
{

}
finally
{
    
}
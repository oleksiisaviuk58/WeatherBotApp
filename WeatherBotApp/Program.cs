using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();

using StringReader telegramTokenReader = new StringReader("/Users/alexsavyuk/RiderProjects/" +
                                                    "WeatherTelegramBotApp/WeatherBotApp/Token.txt");
var telegramToken = await telegramTokenReader.ReadLineAsync();

var bot = new TelegramBotClient(telegramToken!, cancellationToken: cts.Token);

try
{
    bot.OnError += OnError;
    bot.OnMessage += OnMessage;
    bot.OnUpdate += OnUpdate;
    Console.WriteLine("Bot started!");
    
    Console.ReadLine();
    Console.WriteLine("Bot ended!");
}
catch (Exception ex) { Console.WriteLine(ex.Message); }
finally { cts.Cancel(); }

Task OnError(Exception exception, HandleErrorSource source)
{
    throw new NotImplementedException();
}

Task OnUpdate(Update update)
{
    throw new NotImplementedException();
}

Task OnMessage(Message message, UpdateType type)
{
    throw new NotImplementedException();
}
private static readonly Dictionary<long, UserState> _states = new Dictionary<long, UserState>();

using var cts = new CancellationTokenSource();

using StreamReader telegramTokenReader = new StreamReader("/Users/alexsavyuk/RiderProjects/WeatherBotApp/" +
                                                          "WeatherBotApp/Token.txt");
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

async Task OnUpdate(Update update)
{
    if (update.CallbackQuery is not null)
    {
        var callback = update.CallbackQuery;
        await bot.AnswerCallbackQuery(callback.Id);

        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var data = callback.Data;

        switch (data)
        {
            case "today":
                break;

            case "tomorrow":
                break;

            case "3days":
                break;

            case "7days":
                break;
        }

        await bot.EditMessageText(chatId, messageId, "🌎 Надішли назву міста, координати або поділись своїми:", 
            replyMarkup: null);
        await bot.SendMessage(chatId, "👇👇👇",
            replyMarkup: new KeyboardButton[] { KeyboardButton.WithRequestLocation("Share location") });
        
        _states[chatId] = UserState.WaitingForLocation;
    }
}

async Task OnMessage(Message message, UpdateType type)
{
    //var message = update.Message;
    var chatId = message.Chat.Id;
    
    if (message.Text == "/start")
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⛅️ Сьогодні", "today"),
                InlineKeyboardButton.WithCallbackData("🌦 Завтра", "tomorrow")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📅 3 дні", "3days"),
                InlineKeyboardButton.WithCallbackData("📅 7 днів", "7days")
            }
        });

        await bot.SendMessage(message.Chat, "Welcome!", replyMarkup: keyboard);
        
        _states[message.Chat.Id] = UserState.WaitingForDays;
    }
    
    if (_states.TryGetValue(chatId, out var state) && state == UserState.WaitingForLocation)
    {
        if (message.Location is not null)
        {
            var latitude = message.Location.Latitude;
            var longitude = message.Location.Longitude;
                
            //!!!!!!
        }
        else if (TryParseCoordinates(message.Text, out var lat,  out var lon))
        {
            //!!!!!!!
        }
            
        else if (!string.IsNullOrWhiteSpace(message.Text))
        {
            var city = message.Text.Trim();
            //!!!!!!!!
        }

        else
        {
            //!!!!!!
        }

        _states[chatId] = UserState.None;
    }
}

bool TryParseCoordinates(string? text, out double lat, out double lon)
{
    lat = lon = 0;

    if (string.IsNullOrWhiteSpace(text))
        return false;

    var parts = text.Split(',', ' ');
    if (parts.Length < 2)
        return false;

    return double.TryParse(parts[0], out lat) && double.TryParse(parts[1], out lon);
}
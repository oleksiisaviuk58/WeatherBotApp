Dictionary<long, UserState> _states = new Dictionary<long, UserState>();
int _days = 0;

using StreamReader telegramTokenReader = new StreamReader("/Users/alexsavyuk/RiderProjects/WeatherBotApp/" +
                                                          "WeatherBotApp/Token.txt");
var telegramToken = await telegramTokenReader.ReadLineAsync();

using var cts = new CancellationTokenSource();
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
                _days = 1;
                break;

            case "tomorrow":
                _days = 2;
                break;

            case "3days":
                _days = 3;
                break;

            case "7days":
                _days = 7;
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
    var chatId = message.Chat.Id;
    
    if (message.Text == "/start") 
        await SendInitialMessage(chatId);
    
    else if (!string.IsNullOrWhiteSpace(message.Text) && _states[chatId] == UserState.None) 
        await SendInitialMessage(chatId);
    
    else if (_states.TryGetValue(chatId, out var state) && state == UserState.WaitingForLocation)
    {
        if (message.Location is not null)
        {
            var latitude = message.Location.Latitude;
            var longitude = message.Location.Longitude;

            var weather = WeatherService.GetFromCoordinates(latitude, longitude, _days);
            await bot.SendMessage(chatId, weather.Result, replyMarkup: new ReplyKeyboardRemove());

        }
        else if (GeoCodeService.TryParseCoordinates(message.Text, out var latitude,  out var longitude))
        {
            var weather = WeatherService.GetFromCoordinates(latitude, longitude, _days);
            await bot.SendMessage(chatId, weather.Result, replyMarkup: new ReplyKeyboardRemove());
        }
            
        else if (!string.IsNullOrWhiteSpace(message.Text))
        {
            var city = message.Text.Trim();

            var geocode = GeoCodeService.Get(city);
            var weather = WeatherService.GetFromCityName(geocode.Result, _days);
            await bot.SendMessage(chatId, weather.Result, replyMarkup: new ReplyKeyboardRemove());
        }

        else
        {
            await bot.SendMessage(chatId, "Упппс, шось трапилось 🙄 Давай спочатку...", 
                replyMarkup: new ReplyKeyboardRemove());
            await SendInitialMessage(chatId);
        }

        _states[chatId] = UserState.None;
    }
}

async Task SendInitialMessage(long chatId)
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

    await bot.SendMessage(chatId, "Welcome!", replyMarkup: keyboard);
        
    _states[chatId] = UserState.WaitingForDays;
}
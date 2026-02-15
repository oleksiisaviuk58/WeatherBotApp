Dictionary<long, User> _users = new Dictionary<long, User>();
Dictionary<long, UserState> _states = new Dictionary<long, UserState>();

using StreamReader telegramTokenReader = new StreamReader("/Users/alexsavyuk/RiderProjects/WeatherBotApp/" +
                                                           "WeatherBotApp/Token.txt");
var telegramToken = await telegramTokenReader.ReadLineAsync();

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(telegramToken!, cancellationToken: cts.Token);

try
{
    bot.OnError += OnError;
    bot.OnUpdate += OnUpdate;
    Console.WriteLine("Bot started!");
     
    async Task OnUpdate(Update update)
    {
        User _user;
        if (update.CallbackQuery is not null)
        {
            var chatId = update!.CallbackQuery!.Message!.Chat.Id;
            _user = GetUser(chatId);

            var callback = update.CallbackQuery;
            await bot.AnswerCallbackQuery(callback.Id);
             
            _user.Days = callback.Data switch
            {
                "today" => 1,
                "tomorrow" => 2,
                "3days" => 3,
                "7days" => 7,
                _ => 1
            };
             
            await SendLocationMessage(_user.ChatId,  callback);
        }

        else if (update.Message is not null)
        {
            var message = update.Message;
            var chatId = update!.Message!.Chat.Id;
            _user = GetUser(chatId);
             
            if (!string.IsNullOrWhiteSpace(message.Text) && message.Text == "/start")
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                await SendInitialMessage(_user.ChatId);
            }
             
            else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.None)
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                await SendInitialMessage(_user.ChatId);
            }
             
            else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.WaitingForDays)
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                
                if (int.TryParse(message.Text.Trim(), out var _days))
                {
                    _user.Days = _days;
                    await SendLocationMessage(_user.ChatId, update!.CallbackQuery!);
                }

                else
                {
                    await bot.SendMessage(_user.ChatId, "Число!!!!!", replyMarkup: new ReplyKeyboardRemove());
                    Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {"Число!!!!!"}");
                    RemoveUser(_user.ChatId);
                }
            }

            else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.WaitingForLocation)
            {
                if (CheckRequestMessage(message.Text.Trim()))
                {
                    Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                 
                    _user.City = message.Text.Trim();
                     
                    var geocode = await GeoCodeService.Get(_user.City);
                    if (geocode is null)
                    {
                        await bot.SendMessage(_user.ChatId, "🤔 Я намагався знайти це місто.\nАле навіть Google не зрозумів, що ти мав на увазі.", replyMarkup: new ReplyKeyboardRemove());
                        Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {"🤔 Я намагався знайти це місто.\nАле навіть Google не зрозумів, що ти мав на увазі."}");
                        RemoveUser(_user.ChatId);
                        return;
                    }
                     
                    var weather = await WeatherService.GetFromCityName(geocode, _user.Days);
                    await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove(), parseMode: ParseMode.Markdown);
                    Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {weather}");
                }
                else
                {
                    var weather = 
                        "🤔 Я намагався знайти це місто.\nАле навіть Google не зрозумів, що ти мав на увазі.";
                    await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove());
                    Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {weather}");
                }
                 
                RemoveUser(_user.ChatId);
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] Delete {_user.ChatId} user!");
            }

            else if (!string.IsNullOrWhiteSpace(message.Text) 
                     && GeoCodeService.TryParseCoordinates(message.Text, out var latitude, out var longitude)
                     && _states[_user.ChatId] == UserState.WaitingForLocation)
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                 
                _user.Latitude = latitude;
                _user.Longitude = longitude;
                 
                var weather = await WeatherService.GetFromCoordinates(_user.Latitude, _user.Longitude, _user!.Days);
                await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove(), parseMode: ParseMode.Markdown);
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {weather}");
                 
                RemoveUser(_user.ChatId);
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] Delete {_user.ChatId} user!");
            }
                 
            else if (message.Location is not null && _states[_user.ChatId] == UserState.WaitingForLocation)
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Location.Latitude.ToString().Trim()}, {message.Location.ToString().Trim()}");
                 
                _user.Latitude = message.Location.Latitude;
                _user.Longitude = message.Location.Longitude;
                 
                var weather = await WeatherService.GetFromCoordinates(_user.Latitude, _user.Longitude, _user.Days);
                await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove(), parseMode: ParseMode.Markdown);
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {weather}");
                 
                RemoveUser(_user.ChatId);
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] Delete {_user.ChatId} user!");
            }
             
            else if (!string.IsNullOrWhiteSpace(message.Text))
            {
                Console.WriteLine($"[{DateTime.UtcNow}, {_user.ChatId}] {message.Text.Trim()}");
                await SendInitialMessage(_user.ChatId);
            }
        }
    }
    
    Task OnError(Exception exception, HandleErrorSource source) => throw (exception);
     
    Console.ReadLine();
    Console.WriteLine("Bot ended!");
}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);
}
finally { cts.Cancel(); }

async Task SendLocationMessage(long chatId, CallbackQuery callback)
{
    await bot.SendMessage(chatId, 
        "🌎 Надішли назву міста, координати або поділись своїми...", 
        replyMarkup: new ReplyKeyboardRemove());

    var locationKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton("Share location") { RequestLocation = true }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
    await bot.SendMessage(chatId, text: "👇 Please share your location to continue:", 
        replyMarkup: locationKeyboard);

    _states[chatId] = UserState.WaitingForLocation;
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
    
    await bot.SendMessage(chatId, "Welcome!", replyMarkup: keyboard, parseMode: ParseMode.Markdown);
    
    _states[chatId] = UserState.WaitingForDays;
}

bool RemoveUser(long chatId)
{
    try
    {
        _users.Remove(chatId);
        _states.Remove(chatId);
         
        return true;
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception);
        return false;
    }
}

User GetUser(long chatId)
{
    if (_users.TryGetValue(chatId, out var _user)) 
        return _user;
     
    _user = new User(chatId);
    _users[chatId] = _user;
    _states[chatId] = UserState.None;
     
    return _user;
}

bool CheckRequestMessage(string message)
{
    if (!Regex.IsMatch(message, @"^[a-zA-Zа-яА-ЯіїєґІЇЄҐ\s\-]+$")) 
        return false;
     
    return true;
}
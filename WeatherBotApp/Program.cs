using User = WeatherBotApp.User;

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
                 await SendInitialMessage(_user.ChatId);
             }
             
             else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.None)
             {
                 await SendInitialMessage(_user.ChatId);
             }
             
             else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.WaitingForDays)
             {
                 _user.Days = Convert.ToInt32(message.Text.Trim());

                 await SendLocationMessage(_user.ChatId, update!.CallbackQuery!);
             }

             else if (!string.IsNullOrWhiteSpace(message.Text) && _states[_user.ChatId] == UserState.WaitingForLocation)
             { 
                 _user.City = message.Text.Trim();
                     
                 var geocode = await GeoCodeService.Get(_user.City);
                 var weather = await WeatherService.GetFromCityName(geocode, _user.Days);
                 await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove());
                 
                 RemoveUser(_user.ChatId);
             }

             else if (!string.IsNullOrWhiteSpace(message.Text) 
                      && GeoCodeService.TryParseCoordinates(message.Text, out var latitude, out var longitude)
                      && _states[_user.ChatId] == UserState.WaitingForLocation)
             { 
                 _user.Latitude = latitude;
                 _user.Longitude = longitude;
                 
                 var weather = await WeatherService.GetFromCoordinates(_user.Latitude, _user.Longitude, _user!.Days);
                 await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove());
                 
                 RemoveUser(_user.ChatId);
             }
                 
             else if (message.Location is not null && _states[_user.ChatId] == UserState.WaitingForLocation)
             { 
                 _user.Latitude = message.Location.Latitude;
                 _user.Longitude = message.Location.Longitude;
                 
                 var weather = await WeatherService.GetFromCoordinates(_user.Latitude, _user.Longitude, _user.Days);
                 await bot.SendMessage(_user.ChatId, weather, replyMarkup: new ReplyKeyboardRemove());
                 
                 RemoveUser(_user.ChatId);
             }
             
             else if (!string.IsNullOrWhiteSpace(message.Text))
             {
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
     await bot.SendMessage(callback.Message!.Chat.Id,
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
     await bot.SendMessage(callback.Message!.Chat.Id, text: "👇 Please share your location to continue:",
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
     
     _states[chatId] = UserState.WaitingForDays;
     
     await bot.SendMessage(chatId, "Welcome!", replyMarkup: keyboard);
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
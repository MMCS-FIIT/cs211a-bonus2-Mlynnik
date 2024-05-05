using System.Reflection.Metadata.Ecma335;

namespace BoolFuncTGBot;

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "7073974786:AAG6gy8Hd6l9OfoJlPxOdDvIJuY60RmeA9A";
    
    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.
        // TODO: Инициализация необходимых полей
        
        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);
        
        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();
        
        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new [] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape){}

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    /// <summary>
    /// регулярка функции
    /// </summary>
    private Regex reg_func = new Regex(@"^[01]+$");

    /// <summary>
    /// нынешнее сотояние бота
    /// </summary>
    private Input_modes input_mode_now = Input_modes.Wait;

    /// <summary>
    /// столбец значений исходной функции
    /// </summary>
    private string current_func = "";

    private short count_err = 0;

    private Math_funcs f_now;

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        if (message is null)
        {
            return;
        }
        // Будем обрабатывать только текстовые сообщения.
        // При желании можно обрабатывать стикеры, фото, голосовые и т. д.
        //
        // Обратите внимание на использованную конструкцию. Она эквивалентна проверке на null, приведённой выше.
        // Подробнее об этом синтаксисе: https://medium.com/@mattkenefick/snippets-in-c-more-ways-to-check-for-null-4eb735594c09
        if (message.Text is not { } messageText)
        {
            return;
        }

        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;
        
        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");

        // TODO: Обработка пришедших сообщений

        StringBuilder output_message = new StringBuilder("");
        if (messageText == "/start")
        {
            input_mode_now = Input_modes.Wait;
            output_message.Append("Это бот, анализирующий булевы функции и он может:\n" +
                "\nОпределять принадлежность классам Поста" +
                "\nПереводить в полином Жегалкина" +
                "\nДополнять до базиса" +
                "\n\nДоступны следующие команды:\n" +
                "/enter_func - ожидается столбец значений функции\n" +
                "/info - вызывает справку\n" +
                "/info_theory - вызывает справку по теории\n" +
                "/end - заверщает программу");
            count_err = 1;
        }
        else if (input_mode_now == Input_modes.End)
        {
            return;
        }
        else if (messageText == "/info")
        {
            output_message.Append("Это бот, анализирующий булевы функции и он может:\n" +
                "\nОпределять принадлежность классам Поста" +
                "\nПереводить в полином Жегалкина" +
                "\nДополнять до базиса" +
                "\n\nПолный список комманд бота:\n\nГлобальные команды:\n" +
                "/info - вызывает справку\n" +
                "/info_theory - вызывает справку по теории\n" +
                "/end - заверщает программу\n" +
                "\n\nОстальные:\n" +
                "/enter_func - ожидается столбец значений булевой функции\n\n" +
                "/classes_Post - выводит информацию о принадлежности функции классам Поста\n\n" +
                "/polynomial - выводит функцию в виде полинома Жегалкина\n\n" +
                "/basis - дополняет до базиса, если это возможно\n\n" +
                "/download - скачивает html файл с полным отчетом по функции"
                );
        }
        else if (messageText == "/info_theory")
        {
            Message message_info_theory;
            using (Stream stream = System.IO.File.OpenRead("img_info_theory.jpg"))
            {
                message_info_theory = await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: stream,
                    cancellationToken: cancellationToken
                );
            }
            return;
        }
        else if (messageText == "/end")
        {
            output_message.Append("Программа завершена");
            input_mode_now = Input_modes.End;
        }
        else if (messageText == "/enter_func")
        {
            output_message.Append("Введите столбец значений функции (только '0' и '1'):");
            input_mode_now = Input_modes.Enter_Func;
        }
        else if (input_mode_now == Input_modes.Wait)
        {
            return;
        }
        else if (input_mode_now == Input_modes.Enter_Func)
        {
            bool flag_pow = true;
            int is_pow_2 = messageText.Length;
            while (is_pow_2 > 1)
            {
                if (is_pow_2 % 2 == 1)
                {
                    flag_pow = false;
                    break;
                }
                is_pow_2 /= 2;
            }
            if (reg_func.IsMatch(messageText) && flag_pow)
            {
                current_func = messageText;
                output_message.Append($"Получена функция f({messageText})\nВведите команду:");
                input_mode_now = Input_modes.Command_Func;
                f_now = new Math_funcs(current_func);
            }
            else
            {
                
                count_err++;
                if (count_err == 3)
                {
                    Message message_photo;
                    using (Stream stream = System.IO.File.OpenRead("img_err.jpg"))
                    {
                        message_photo = await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: stream,
                            cancellationToken: cancellationToken
                        );
                    }
                    output_message.Append("Экзорцисты уже выехали, никуда не уходите...");
                    input_mode_now = Input_modes.End;
                }
                else
                {
                    output_message.Append("Ошибка в значениях функции, попробуйте снова");
                }
            }
        }
        else if (input_mode_now == Input_modes.Command_Func)
        {
            if (messageText == "/classes_Post")
            {
                output_message.Append($"Функция f({current_func})\nПринадлежность классам Поста:" +
                    $"\nP0 {(f_now.Is_Saving_0 ? "+" : "-")}" +
                    $"\nP1 {(f_now.Is_Saving_1 ? "+" : "-")}" +
                    $"\nL {(f_now.Is_Linear ? "+" : "-")}" +
                    $"\nS {(f_now.Is_Self_dual ? "+" : "-")}" +
                    $"\nM {(f_now.Is_Monotone ? "+" : "-")}");
            }
            else if (messageText == "/polynomial")
            {
                output_message.Append($"Функция f({current_func})\nПолином Жегалкина:" +
                    $"\n{f_now.polynomial_Zhegalkin}");
            }
            else if (messageText == "/basis")
            {
                if (f_now.basis.Keys.Count == 1)
                {
                    output_message.Append($"Функция f({current_func})\nДополнение до базиса:\n");
                    if (f_now.Is_Saving_0)
                        output_message.Append("Функцию нельзя дополнить до базиса");
                    else
                        output_message.Append("Функция уже является базисом");

                }
                else
                {
                    output_message.Append($"{f_now.basis.Keys.First()}\n\nДополнено:\n{string.Join("\n", f_now.basis.Keys.Skip(1))}\n\nРекомендуется скачать отчет (/download)");
                }
            }
            else if (messageText == "/download")
            {
                Message message_file;
                f_now.OutPut_to_File();
                using (Stream stream = System.IO.File.OpenRead("table.html"))
                {
                    message_file = await botClient.SendDocumentAsync(
                        chatId: chatId,
                        document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, "table.html"),
                        cancellationToken: cancellationToken
                    );
                }
                return;
            }
            else
            {
                return;
            }
        }

        IReplyMarkup replay_now;
        if (input_mode_now == Input_modes.Command_Func)
            replay_now = GetButtons_2();
        else if (input_mode_now == Input_modes.End)
            replay_now = GetButtons_3();
        else
            replay_now = GetButtons_1();

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: output_message.ToString(),
            replyMarkup: replay_now,
            cancellationToken: cancellationToken);
    }


    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        // Завершаем работу
        return Task.CompletedTask;
    }


    /// <summary>
    /// кнопки для выполнения действий 1
    /// </summary>
    static IReplyMarkup GetButtons_1()
    {
        var Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton("/enter_func"), new KeyboardButton("/info"), },
            };
        return new ReplyKeyboardMarkup(Keyboard);
    }

    /// <summary>
    /// кнопки для выполнения действий 2
    /// </summary>
    static IReplyMarkup GetButtons_2()
    {
        var Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton("/enter_func"), new KeyboardButton("/info"), },
                new List<KeyboardButton> { new KeyboardButton("/classes_Post"), new KeyboardButton("/polynomial"), },
                new List<KeyboardButton> { new KeyboardButton("/basis"), new KeyboardButton("/download"), }
            };
        return new ReplyKeyboardMarkup(Keyboard);
    }

    /// <summary>
    /// кнопки для выполнения действий 3
    /// </summary>
    static IReplyMarkup GetButtons_3()
    {
        var Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton("/start"), new KeyboardButton("/info"), },
            };
        return new ReplyKeyboardMarkup(Keyboard);
    }

}
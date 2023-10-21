using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DiscordBot.Commands;
using DiscordBot.SLCommands;
using DSharpPlus.SlashCommands;
using TokenType = DSharpPlus.TokenType;


namespace DiscordBot
{
    /// <summary>
    /// Клас у якому виконується конфігурація бота
    /// </summary>
    class Bot
    {
        /// <summary>
        /// Змінна, для роботою з Discord API
        /// </summary>
        private DiscordClient _client;

        /// <summary>
        /// Змінна, для долучення команд у форматі: ?{command_name}
        /// </summary>
        private CommandsNextExtension _commands;

        /// <summary>
        /// Змінна, для долучення слеш-команд
        /// </summary>
        private SlashCommandsExtension _slashCommands;

        /// <summary>
        /// Метод для конфігурації та запуску бота Discord.
        /// </summary>
        /// <returns>Завдання, яке представляє асинхронний запуск бота.</returns>
        public async Task RunAsync()
        {
            // Створення конфігурації бота.
            var config = new DiscordConfiguration
            {
                Token = "Token_Here", // Замініть "Token_Here" на реальний токен вашого бота.
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.All
            };

            // Ініціалізація клієнта Discord з використанням зазначеної конфігурації.
            _client = new DiscordClient(config);

            // Включення підтримки голосових функцій.
            _client.UseVoiceNext();

            // Додавання обробників подій.
            _client.Ready += OnClientReady;
            _client.GuildCreated += OnGuildCreated;

            // Конфігурація командного модуля.
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "?" }, // Префікс для активації команд.
                EnableMentionPrefix = true // Дозволяє активацію команд шляхом згадування бота.
            };

            // Ініціалізація командного модуля та реєстрація команд.
            _commands = _client.UseCommandsNext(commandsConfig);
            _commands.RegisterCommands<MusicCommands>(); // Реєстрація команд з класу MusicCommands.
            _commands.RegisterCommands<GroupCommands>(); // Реєстрація команд з класу GroupCommands.

            // Ініціалізація модуля для обробки Slash-команд (інтерактивних команд Discord).
            _slashCommands = _client.UseSlashCommands();
            _slashCommands.RegisterCommands<MusicSL>(); // Реєстрація Slash-команд з класу MusicSL.
            _slashCommands.RegisterCommands<GroupSL>(); // Реєстрація Slash-команд з класу GroupSL.

            // Підключення до серверів Discord.
            await _client.ConnectAsync();

            // Безкінечне очікування завершення виконання бота (завдяки Task.Delay(-1)).
            await Task.Delay(-1);
        }

        /// <summary>
        /// Обробник події викликається, коли Discord бот готовий до використання.
        /// </summary>
        /// <param name="sender">Екземпляр клієнта Discord, що викликав подію.</param>
        /// <param name="e">Параметри події ReadyEventArgs, які містять інформацію про готовність бота.</param>
        /// <returns>Завдання, яке позначає завершення обробки події.</returns>
        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot is ready!");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обробник події, яка викликається при долученні бота до сервера (гільдії) Discord.
        /// </summary>
        /// <param name="sender">Екземпляр клієнта Discord, що викликав подію.</param>
        /// <param name="e">Параметри події GuildCreateEventArgs, які містять інформацію про новий сервер.</param>
        /// <returns>Завдання, яке позначає завершення обробки події.</returns>
        private async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            Console.WriteLine($"Бот приєднався до гільдії: {e.Guild.Name}");

            await Task.CompletedTask;
        }


    }

}

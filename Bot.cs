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
    class Bot
    {
        private DiscordClient _client;
        private CommandsNextExtension _commands;
        private SlashCommandsExtension _slashCommands;

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                Token = "Token_Here",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.All
            };


            _client = new DiscordClient(config);

            _client.UseVoiceNext();

            _client.Ready += OnClientReady;
            _client.GuildCreated += OnGuildCreated;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "?" },
                EnableMentionPrefix = true
            };

            _commands = _client.UseCommandsNext(commandsConfig);
            _commands.RegisterCommands<MusicCommands>();
            _commands.RegisterCommands<GroupCommands>();

            _slashCommands = _client.UseSlashCommands();
            _slashCommands.RegisterCommands<MusicSL>();
            _slashCommands.RegisterCommands<GroupSL>();

            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot is ready!");
            return Task.CompletedTask;
        }

        private Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            Console.WriteLine($"Joined guild: {e.Guild.Name}");
            return Task.CompletedTask;
        }

    }

}

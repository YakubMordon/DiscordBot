using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    /// <summary>
    /// Клас GroupCommands для обробки команд по типу ?{command_name}, пов'язаних із музикою.
    /// </summary>
    public class MusicCommands : BaseCommandModule
    {
        /// <summary>
        /// Обробляє команду `?ping` та надсилає відповідь "Pong!".
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє асинхронну відповідь</returns>
        [Command("ping")]
        [Description("Returns pong")]
        [RequireRoles(RoleCheckMode.Any, "Owner", "Admins")]
        public async Task Ping(CommandContext ctx)
        {
            Console.WriteLine("Entering ?ping command");
            await ctx.RespondAsync("Pong!");
        }

        /// <summary>
        /// Обробляє команду `?play` та готується до відтворення відео за вказаним URL на YouTube.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <param name="url">URL відео на YouTube.</param>
        /// <returns>Завдання, яке представляє асинхронне відтворення відео</returns>
        [Command("play")]
        [Description("Plays the audio path from Youtube video by given url")]
        public async Task Play(CommandContext ctx, [RemainingText][Description("Youtube video url")] string url)
        {
            Console.WriteLine("Entering ?play command");
            await MusicPlayer.PlayMusicAsync(ctx, url);
        }

        /// <summary>
        /// Обробляє команду `?stop` та готується до зупинення відтворення відео.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє зупинку відтворення відео</returns>
        [Command("stop")]
        [Description("Stops Music Player Bot and disconnects him from voice chat")]
        public async Task Stop(CommandContext ctx)
        {
            Console.WriteLine("Entering ?stop command");
            await MusicPlayer.StopAsync(ctx);
        }

        /// <summary>
        /// Обробляє команду `?skip` та готується до пропуску відео, яке є поточним.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє пропуск відео, яке є поточним.</returns>
        [Command("skip")]
        [Description("Skips current song in queue")]
        public async Task Skip(CommandContext ctx)
        {
            Console.WriteLine("Entering ?skip command");
            await MusicPlayer.SkipSongAsync(ctx);
        }

        /// <summary>
        /// Обробляє команду `?list` та готується до відображення списку черги відтворення відео.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє список черги відтворення відео.</returns>
        [Command("list")]
        [Description("Shows queue of music")]
        public async Task List(CommandContext ctx)
        {
            Console.WriteLine("Entering ?list command");
            await MusicPlayer.ShowQueueAsync(ctx);
        }

        /// <summary>
        /// Обробляє команду `?volume` та готується до зміни гучності відтворення відео.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <param name="volume">Рядок, у якому вказано гучність</param>
        /// <returns>Завдання, яке представляє зміну гучності.</returns>
        [Command("volume")]
        [Description("Change the volume of the bot")]
        public async Task ChangeVolumeCommand(CommandContext ctx, [Description("Argument, which needs to be in range from 0,0 to 2,0 Number should be written with comma")] string volume)
        {
            Console.WriteLine("Entering ?volume command");
            if (float.TryParse(volume, out var volumeParsed))
            {
                await MusicPlayer.ChangeVolumeAsync(ctx, volumeParsed);
            }
            else
            {
                await ctx.RespondAsync("Volume isn't float number");
            }
        }

        /// <summary>
        /// Обробляє команду `?repeatoff` та готується до зупинення повторення поточної пісні або списку пісень.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє зупинення повторення.</returns>
        [Command("repeatoff")]
        [Description("Turns off repeat of all music")]
        public async Task RepeatOff(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeatoff command");
            await MusicPlayer.RepeatStatusAsync(ctx, null);
            await ctx.RespondAsync("You turned off repeat of music");
        }

        /// <summary>
        /// Обробляє команду `?repeat` та готується до повторення поточної пісні.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє повторення поточної пісні.</returns>
        [Command("repeat")]
        [Description("Turns on repeat on a single music. Single music is current playing music")]
        public async Task RepeatSingle(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeat command");
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatSingle);
            await ctx.RespondAsync("You turned on repeat of single music");
        }

        /// <summary>
        /// Обробляє команду `?repeatall` та готується до повторення всього списку пісень.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <returns>Завдання, яке представляє повторення всього списку пісень.</returns>
        [Command("repeatall")]
        [Description("Turns on repeat on a queue of music.")]
        public async Task RepeatAll(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeatall command");
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatAll);
            await ctx.RespondAsync("You turned on repeat of music queue");
        }

        /// <summary>
        /// Обробляє команду `?download` та готується до скачування відео з відеохостингу Youtube.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <param name="url">URL-адреса на Youtube відео</param>
        /// <returns>Завдання, яке представляє скачування відео.</returns>
        [Command("download")]
        [Description("Downloads from Youtube by given url")]
        public async Task Download(CommandContext ctx, [RemainingText][Description("Youtube video url")] string url)
        {
            Console.WriteLine("Entering ?download command");
            await MusicPlayer.DownloadVideoAsync(ctx, url);
        }

    }
}

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("ping")]
        [Description("Returns pong")]
        [RequireRoles(RoleCheckMode.Any, "Owner", "Admins")]
        public async Task Ping(CommandContext ctx)
        {
            Console.WriteLine("Entering ?ping command");
            await ctx.RespondAsync("Pong!");
        }

        [Command("play")]
        [Description("Plays the audio path from Youtube video by given url")]
        public async Task Play(CommandContext ctx, [RemainingText][Description("Youtube video url")] string url)
        {
            Console.WriteLine("Entering ?play command");
            await MusicPlayer.PlayMusicAsync(ctx, url);
        }

        [Command("stop")]
        [Description("Stops Music Player Bot and disconnects him from voice chat")]
        public async Task Stop(CommandContext ctx)
        {
            Console.WriteLine("Entering ?stop command");
            await MusicPlayer.StopAsync(ctx);
        }

        [Command("skip")]
        [Description("Skips current song in queue")]
        public async Task Skip(CommandContext ctx)
        {
            Console.WriteLine("Entering ?skip command");
            await MusicPlayer.SkipSongAsync(ctx);
        }

        [Command("list")]
        [Description("Shows queue of music")]
        public async Task List(CommandContext ctx)
        {
            Console.WriteLine("Entering ?list command");
            await MusicPlayer.ShowQueueAsync(ctx);
        }

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

        [Command("repeatoff")]
        [Description("Turns off repeat of all music")]
        public async Task RepeatOff(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeatoff command");
            await MusicPlayer.RepeatStatusAsync(ctx, null);
            await ctx.RespondAsync("You turned off repeat of music");
        }

        [Command("repeat")]
        [Description("Turns on repeat on a single music. Single music is current playing music")]
        public async Task RepeatSingle(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeat command");
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatSingle);
            await ctx.RespondAsync("You turned on repeat of single music");
        }

        [Command("repeatall")]
        [Description("Turns on repeat on a queue of music.")]
        public async Task RepeatAll(CommandContext ctx)
        {
            Console.WriteLine("Entering ?repeatall command");
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatAll);
            await ctx.RespondAsync("You turned on repeat of music queue");
        }

        [Command("download")]
        [Description("Downloads from Youtube by given url")]
        public async Task Download(CommandContext ctx, [RemainingText][Description("Youtube video url")] string url)
        {
            Console.WriteLine("Entering ?download command");
            await MusicPlayer.DownloadVideoAsync(ctx, url);
        }

    }
}

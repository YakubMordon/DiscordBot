using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.SLCommands
{
    public class MusicSL : ApplicationCommandModule
    {
        [SlashCommand("ping", "Returns pong")]
        [RequireRoles(RoleCheckMode.Any, "Owner", "Admins")]
        public async Task PingCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /ping command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }

        [SlashCommand("play", "Plays the audio path from a YouTube video by the given URL")]
        public async Task PlayCommand(InteractionContext ctx, [Option("url", "YouTube video URL")] string url)
        {
            Console.WriteLine("Entering /play command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Preparing to play song..."));

            var commandsNext = ctx.Client.GetCommandsNext();
            var commandName = "play";
            var command = commandsNext.FindCommand(commandName, out var args);

            var rawArguments = $"{commandName} {url}";

            var fakeContext = commandsNext.CreateFakeContext(ctx.User, ctx.Channel, rawArguments, "?", command, url);

            await commandsNext.ExecuteCommandAsync(fakeContext);
        }

        [SlashCommand("stop", "Stops Music Player Bot and disconnects it from the voice chat")]
        public async Task StopCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /stop command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Stopping current song..."));
            await MusicPlayer.StopAsync(ctx);
        }

        [SlashCommand("skip", "Skips the current song in the queue")]
        public async Task SkipCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /skip command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Skipping current song..."));
            await MusicPlayer.SkipSongAsync(ctx);
        }

        [SlashCommand("list", "Shows the queue of music")]
        public async Task ListCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /list command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Showing list of songs..."));
            await MusicPlayer.ShowQueueAsync(ctx);
        }

        [SlashCommand("volume", "Changes the volume of the bot")]
        public async Task ChangeVolumeCommand(InteractionContext ctx, [Option("volume", "Argument, which needs to be in range from 0,0 to 2,0 Number should be written with comma")] string volume)
        {
            Console.WriteLine("Entering /volume command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Changing volume..."));

            var commandsNext = ctx.Client.GetCommandsNext();
            var commandName = "volume";
            var command = commandsNext.FindCommand(commandName, out var args);

            var rawArguments = $"{commandName} {volume}";

            var fakeContext = commandsNext.CreateFakeContext(ctx.User, ctx.Channel, rawArguments, "?", command, volume);

            await commandsNext.ExecuteCommandAsync(fakeContext);

        }

        [SlashCommand("repeatoff", "Turns off repeat of all music")]
        public async Task RepeatOffCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /repeatoff command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You turned off repeat of music"));
            await MusicPlayer.RepeatStatusAsync(ctx, null);
        }

        [SlashCommand("repeat", "Turns on repeat on a single music. Single music is the currently playing music")]
        public async Task RepeatSingleCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /repeat command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You turned on repeat of single music"));
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatSingle);
            
        }

        [SlashCommand("repeatall", "Turns on repeat on a queue of music")]
        public async Task RepeatAllCommand(InteractionContext ctx)
        {
            Console.WriteLine("Entering /repeatall command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You turned on repeat of music queue"));
            await MusicPlayer.RepeatStatusAsync(ctx, RepeatStatus.RepeatAll);

        }

        [SlashCommand("download", "Downloads from YouTube by the given URL")]
        public async Task DownloadCommand(InteractionContext ctx, [Option("url", "YouTube video URL")] string url)
        {
            Console.WriteLine("Entering /download command");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Downloading video from Youtube..."));
            await MusicPlayer.DownloadVideoAsync(ctx, url);
        }

    }
}

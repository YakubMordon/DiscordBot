using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot.Services
{
    /// <summary>
    /// Перелік статусів для повторення аудіо композицій.
    /// </summary>
    public enum RepeatStatus
    {
        RepeatSingle, // Повторювати одну аудіо композицію
        RepeatAll     // Повторювати всі аудіо композиції у черзі
    }

    /// <summary>
    /// Клас для управління програвачем музики та пов'язаної інфраструктури.
    /// </summary>
    public class MusicPlayer
    {
        /// <summary>
        /// Словник, у якому зберігаються черги пісень для кожної групи
        /// </summary>
        private static readonly Dictionary<ulong, Queue<Video>> _musicQueue = new Dictionary<ulong, Queue<Video>>();

        /// <summary>
        /// Словник, у якому зберігаються всі голосові канали, до яких доєднаний бот
        /// </summary>
        private static readonly Dictionary<ulong, VoiceNextConnection> _voiceConnections = new Dictionary<ulong, VoiceNextConnection>();

        /// <summary>
        /// Словник, у якому зберігається стан, чи потрібно пропускати поточне відео, в голосовому каналі
        /// </summary>
        private static readonly Dictionary<ulong, bool> _isSkipped = new Dictionary<ulong, bool>();

        /// <summary>
        /// Словник, у якому зберігається статус повторення для кожної гільдії
        /// </summary>
        private static readonly Dictionary<ulong, RepeatStatus?> _repeatStatus = new Dictionary<ulong, RepeatStatus?>();

        /// <summary>
        /// Змінна, для взаємодії з Youtube
        /// </summary>
        private static readonly YoutubeClient _youtubeClient = new YoutubeClient();

        /// <summary>
        /// Шаблон, для надсилання повідомлення, яке відео відтворюється
        /// </summary>
        private static readonly DiscordEmbedBuilder _queueEmbed = new DiscordEmbedBuilder()
            .WithTitle("Now Playing:")
            .WithColor(DiscordColor.Blurple);

        /// <summary>
        /// Відтворює аудіо з YouTube за URL у голосовому каналі користувача.
        /// </summary>
        /// <param name="ctx">Контекст взаємодії з командою.</param>
        /// <param name="url">URL-адреса YouTube відео.</param>
        /// <returns>Завдання, яке представляє асинхронне відтворення відео.</returns>
        public static async Task PlayMusicAsync(CommandContext ctx, string url)
        {
            if (ctx.Guild == null)
            {
                await ctx.RespondAsync("This bot does not accept commands over direct messages");
                Console.WriteLine("This bot does not accept commands over direct messages");
                return;
            }

            if (string.IsNullOrEmpty(url))
            {
                await ctx.RespondAsync("There are no arguments");
                Console.WriteLine("There are no arguments");
                return;
            }

            var botVoiceState = ctx.Guild.CurrentMember?.VoiceState;
            var botVoiceChannel = botVoiceState?.Channel;

            if (botVoiceChannel == null)
            {
                Console.WriteLine("Bot Voice channel is null");
                var userVoiceState = ctx.Member?.VoiceState;
                var voiceChannel = userVoiceState?.Channel;

                if (voiceChannel != null)
                {
                    Console.WriteLine("User voice channel is not null");
                    if (!UrlChecker.IsYouTubeUrl(url))
                    {
                        await ctx.RespondAsync("Invalid YouTube URL");
                        Console.WriteLine("Invalid YouTube URL");
                        return;
                    }

                    if (!_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue))
                    {
                        Console.WriteLine("Creating queue of songs");
                        guildQueue = new Queue<Video>();
                        _musicQueue[ctx.Guild.Id] = guildQueue;
                    }

                    Console.WriteLine("Adding song to queue");
                    
                    guildQueue.Enqueue(await _youtubeClient.Videos.GetAsync(url));

                    Console.WriteLine("Added to music queue");

                    await ctx.RespondAsync("Added to the music queue");

                    
                    if (guildQueue.Count == 1)
                    {
                        Console.WriteLine("Queue is not 0");
                        await PlayNextSongAsync(ctx, voiceChannel);
                    }
                }
                else
                {
                    await ctx.RespondAsync("You're not in a voice channel");
                    Console.WriteLine("You're not in a voice channel");
                }
            }
            else
            {
                Console.WriteLine("Bot voice channel is not null");
                
                if (!UrlChecker.IsYouTubeUrl(url))
                {
                    await ctx.RespondAsync("Invalid YouTube URL");
                    Console.WriteLine("Invalid YouTube URL");
                    return;
                }

                
                if (!_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue))
                {
                    Console.WriteLine("Creating queue of songs");
                    guildQueue = new Queue<Video>();
                    _musicQueue[ctx.Guild.Id] = guildQueue;
                }

                Console.WriteLine("Adding song to queue");
                
                guildQueue.Enqueue(await _youtubeClient.Videos.GetAsync(url));

                Console.WriteLine("Added to music queue");

                await ctx.RespondAsync("Added to the music queue");

                if (guildQueue.Count == 1)
                {
                    var userVoiceState = ctx.Member?.VoiceState;
                    var voiceChannel = userVoiceState?.Channel;

                    Console.WriteLine("Queue is not 0");
                    await PlayNextSongAsync(ctx, voiceChannel);
                }
            }
        }

        /// <summary>
        /// Асинхронно відтворює наступне відео з черги для заданого сервера.
        /// </summary>
        /// <param name="ctx">Контекст команди Discord.</param>
        /// <param name="voiceChannel">Аудіоканал, на якому відтворювати відео.</param>
        /// <returns>Завдання, яке представляє асинхронне відтворення наступного відео з черги.</returns>
        private static async Task PlayNextSongAsync(CommandContext ctx, DiscordChannel voiceChannel)
        {
            if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
            {
                var video = guildQueue.Peek();

                var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                var audioStream = await _youtubeClient.Videos.Streams.GetAsync(audioStreamInfo);

                var voiceNext = ctx.Client?.GetVoiceNext();

                if (audioStream is not null)
                {
                    Console.WriteLine("Downloading video");
                    var tempFilePath = Path.GetTempFileName();
                    int bufferLen = (int)audioStream.Length;

                    await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferLen, true))
                    {
                        await audioStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine("Entering Audio transmitter");

                    await using (var audioFileReader = new AudioFileReader(tempFilePath))
                    using (var resampler = new MediaFoundationResampler(audioFileReader, new WaveFormat(48000, audioFileReader.WaveFormat.Channels))
                    {
                        ResamplerQuality = 60
                    })
                    {
                        VoiceNextConnection connection;
                        if (!_voiceConnections.TryGetValue(ctx.Guild.Id, out var existingConnection))
                        {
                            Console.WriteLine("Creating new voice connection");
                            connection = await voiceNext.ConnectAsync(voiceChannel);
                            _voiceConnections[ctx.Guild.Id] = connection;
                        }
                        else
                        {
                            Console.WriteLine("Using existing voice connection");
                            connection = existingConnection;
                        }

                        var discordStream = connection.GetTransmitSink();

                        var videoTitle = video.Title;

                        _queueEmbed.AddField("Video: ", videoTitle);

                        await ctx.RespondAsync(embed: _queueEmbed);

                        _queueEmbed.RemoveFieldAt(0);

                        Console.WriteLine($"Now Playing: {videoTitle}");

                        var buffer = new byte[8192];
                        int bytesRead;
                        do
                        {
                            if (_isSkipped.TryGetValue(ctx.Guild.Id, out var _))
                            {
                                Console.WriteLine("Song is skipped");
                                _isSkipped.Remove(ctx.Guild.Id);
                                break;
                            }

                            bytesRead = resampler.Read(buffer, 0, buffer.Length);
                            await discordStream.WriteAsync(buffer, 0, bytesRead);

                        }
                        while (bytesRead > 0);

                        Console.WriteLine("Music transfer has been completed");

                        Console.WriteLine("Music ended");

                        File.Delete(tempFilePath);

                        if (_repeatStatus.TryGetValue(ctx.Guild.Id, out var status))
                        {
                            if (status == RepeatStatus.RepeatAll)
                            {
                                // Move the song to the end of the queue
                                guildQueue.Enqueue(guildQueue.Dequeue());
                            }
                            else if (status is null)
                            {
                                guildQueue.Dequeue();
                            }
                        }
                        else
                        {
                            guildQueue.Dequeue();
                        }


                        if (guildQueue.Count > 0)
                        {
                            Console.WriteLine("There are songs left");
                            await PlayNextSongAsync(ctx, voiceChannel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Перескакує до наступної пісні в черзі музичного бота у голосовому каналі сервера.
        /// </summary>
        /// <param name="ctx">Контекст команди, який містить інформацію про сервер та виклик.</param>
        /// <returns>Завдання, яке представляє асинхронний процес перескакування пісні.</returns>
        public static async Task SkipSongAsync(CommandContext ctx)
        {
            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var connection))
            {

                if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
                {
                    _isSkipped[ctx.Guild.Id] = true;
                }
                else
                {
                    await ctx.RespondAsync("Music queue is empty");
                }
            }
            else
            {
                await ctx.RespondAsync("The bot is not currently playing any songs");
            }
        }

        /// <summary>
        /// Перескакує до наступної пісні в черзі музичного бота у голосовому каналі сервера для Слеш-команди.
        /// </summary>
        /// <param name="ctx">Контекст команди, який містить інформацію про сервер та виклик.</param>
        /// <returns>Завдання, яке представляє асинхронний процес перескакування пісні.</returns>
        public static async Task SkipSongAsync(InteractionContext ctx)
        {
            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var connection))
            {

                if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
                {
                    _isSkipped[ctx.Guild.Id] = true;
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Music queue is empty"));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The bot is not currently playing any songs"));
            }
        }

        /// <summary>
        /// Повторює поточний статус аудіоплеєра у голосовому каналі.
        /// </summary>
        /// <param name="ctx">Контекст команди, що викликає метод.</param>
        /// <param name="status">Новий статус для повторення (або null для вимкнення повторення).</param>
        /// <returns>Асинхронна задача, яка представляє статус повторення.</returns>
        public static async Task RepeatStatusAsync(CommandContext ctx, RepeatStatus? status)
        {
            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var connection))
            {
                if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
                {
                    _repeatStatus[ctx.Guild.Id] = status;
                }
                else
                {
                    await ctx.RespondAsync("Music queue is empty");
                }
            }
            else
            {
                await ctx.RespondAsync("The bot is not currently playing any songs");
            }
        }

        /// <summary>
        /// Повторює поточний статус аудіоплеєра у голосовому каналі для Слеш-команди.
        /// </summary>
        /// <param name="ctx">Контекст команди, що викликає метод.</param>
        /// <param name="status">Новий статус для повторення (або null для вимкнення повторення).</param>
        /// <returns>Асинхронна задача, яка представляє статус повторення.</returns>
        public static async Task RepeatStatusAsync(InteractionContext ctx, RepeatStatus? status)
        {
            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var connection))
            {
                if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
                {
                    _repeatStatus[ctx.Guild.Id] = status;
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Music queue is empty"));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The bot is not currently playing any songs"));
            }
        }

        /// <summary>
        /// Відображає чергу відтворення музики для вказаного серверу.
        /// </summary>
        /// <param name="ctx">Контекст команди, що викликає цей метод.</param>
        /// <returns>Завдання, яке представляє асинхронну операцію.</returns>
        public static async Task ShowQueueAsync(CommandContext ctx)
        {
            if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
            {
                _queueEmbed.WithTitle("Music queue");
                var index = 1;

                foreach (var video in guildQueue)
                {
                    var videoTitle = video.Title;

                    _queueEmbed.AddField($"Video #{index}", videoTitle);
                    index++;
                }

                await ctx.RespondAsync(embed: _queueEmbed);

                _queueEmbed.RemoveFieldRange(0, _queueEmbed.Fields.Count());

                _queueEmbed.WithTitle("Now Playing:");
            }
            else
            {
                await ctx.RespondAsync("The music queue is empty");
            }
        }

        /// <summary>
        /// Відображає чергу відтворення музики для вказаного серверу для Слеш-команд.
        /// </summary>
        /// <param name="ctx">Контекст команди, що викликає цей метод.</param>
        /// <returns>Завдання, яке представляє асинхронну операцію.</returns>
        public static async Task ShowQueueAsync(InteractionContext ctx)
        {
            if (_musicQueue.TryGetValue(ctx.Guild.Id, out var guildQueue) && guildQueue.Count > 0)
            {
                _queueEmbed.WithTitle("Music queue");
                var index = 1;

                foreach (var video in guildQueue)
                {
                    var videoTitle = video.Title;

                    _queueEmbed.AddField($"Video #{index}", videoTitle);
                    index++;
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(_queueEmbed));


                _queueEmbed.RemoveFieldRange(0, _queueEmbed.Fields.Count());

                _queueEmbed.WithTitle("Now Playing:");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("The music queue is empty"));
            }
        }

        /// <summary>
        /// Асинхронний метод, який зупиняє відтворення музики та роз'єднує бота у голосовому каналі.
        /// </summary>
        /// <param name="ctx">Контекст команди, який містить інформацію про викликану команду.</param>
        /// <returns>Завдання, яке представляє асинхронний процес зупинки бота та відтворення музики.</returns>
        public static async Task StopAsync(CommandContext ctx)
        {
            var voiceNext = ctx.Client?.GetVoiceNext();

            var connection = voiceNext?.GetConnection(ctx.Guild);

            if (connection != null)
            {
                connection.Disconnect();
                await ctx.RespondAsync("Music Player Bot has been stopped");
                Console.WriteLine("Music Player Bot has been stopped");
            }

            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var _))
            {
                _voiceConnections.Remove(ctx.Guild.Id);
            }

            if (_musicQueue.TryGetValue(ctx.Guild.Id, out var _))
            {
                _musicQueue.Remove(ctx.Guild.Id);
            }
        }

        /// <summary>
        /// Асинхронний метод, який зупиняє відтворення музики та роз'єднує бота у голосовому каналі для Слеш-команд.
        /// </summary>
        /// <param name="ctx">Контекст команди, який містить інформацію про викликану команду.</param>
        /// <returns>Завдання, яке представляє асинхронний процес зупинки бота та відтворення музики.</returns>
        public static async Task StopAsync(InteractionContext ctx)
        {
            var voiceNext = ctx.Client?.GetVoiceNext();

            var connection = voiceNext?.GetConnection(ctx.Guild);

            if (connection != null)
            {
                connection.Disconnect();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Music Player Bot has been stopped"));
                Console.WriteLine("Music Player Bot has been stopped");
            }

            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var _))
            {
                _voiceConnections.Remove(ctx.Guild.Id);
            }

            if (_musicQueue.TryGetValue(ctx.Guild.Id, out var _))
            {
                _musicQueue.Remove(ctx.Guild.Id);
            }
        }

        /// <summary>
        /// Змінює гучність аудіо для відтворення в голосовому каналі.
        /// </summary>
        /// <param name="ctx">Об'єкт, який представляє контекст команди.</param>
        /// <param name="volume">Значення гучності, яке слід встановити (повинно бути в межах від 0 до 2.0).</param>
        /// <returns>Задача, яка представляє асинхронний процес зміни гучності.</returns>
        public static async Task ChangeVolumeAsync(CommandContext ctx, float volume)
        {
            if (_voiceConnections.TryGetValue(ctx.Guild.Id, out var connection))
            {
                if (volume > 2.5 || volume < 0)
                {
                    await ctx.RespondAsync("Volume must be smaller than 2.0 and greater than 0");
                    Console.WriteLine("Volume must be smaller than 2.0 and greater than 0");
                }
                else
                {
                    connection.GetTransmitSink().VolumeModifier = volume;
                    await ctx.RespondAsync($"Volume has been changed to {volume}");
                    Console.WriteLine($"Volume has been changed to {volume}");
                }
            }
            else
            {
                await ctx.RespondAsync("The bot is not currently playing any songs");
                Console.WriteLine("The bot is not currently playing any songs");
            }
        }

        /// <summary>
        /// Завантажує відео з YouTube за заданим URL та надсилає його на сервер Discord.
        /// </summary>
        /// <param name="ctx">Контекст команди Discord.</param>
        /// <param name="url">URL відео на YouTube, яке потрібно завантажити.</param>
        /// <returns>Асинхронна задача для завантаження відео.</returns>
        public static async Task DownloadVideoAsync(CommandContext ctx, string url)
        {
            if (!UrlChecker.IsYouTubeUrl(url))
            {
                await ctx.RespondAsync("Invalid YouTube URL");
                Console.WriteLine("Invalid YouTube URL");
                return;
            }

            var video = await _youtubeClient.Videos.GetAsync(url);

            if (video == null)
            {
                await ctx.RespondAsync("Failed to get video information");
                Console.WriteLine("Failed to get video information");
                return;
            }

            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            if (streamInfo == null)
            {
                await ctx.RespondAsync("Failed to get video stream");
                Console.WriteLine("Failed to get video stream");
                return;
            }

            var videoFilePath = $"{video.Id}.{streamInfo.Container.Name}";

            using (var output = File.OpenWrite(videoFilePath))
            using (var input = await _youtubeClient.Videos.Streams.GetAsync(streamInfo))
            {
                await input.CopyToAsync(output);
            }


            var builder = new DiscordMessageBuilder()
                .WithContent("Here's the video you requested:")
                .AddFile(File.OpenRead(videoFilePath));

            await ctx.RespondAsync(builder);
            Console.WriteLine("Video was sended");
            File.Delete(videoFilePath);
        }

        /// <summary>
        /// Завантажує відео з YouTube за заданим URL та надсилає його на сервер Discord для Слеш-команд.
        /// </summary>
        /// <param name="ctx">Контекст команди Discord.</param>
        /// <param name="url">URL відео на YouTube, яке потрібно завантажити.</param>
        /// <returns>Асинхронна задача для завантаження відео.</returns>
        public static async Task DownloadVideoAsync(InteractionContext ctx, string url)
        {
            if (!UrlChecker.IsYouTubeUrl(url))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Invalid YouTube URL"));
                Console.WriteLine("Invalid YouTube URL");
                return;
            }

            var video = await _youtubeClient.Videos.GetAsync(url);

            if (video == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Failed to get video information"));
                Console.WriteLine("Failed to get video information");
                return;
            }

            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            if (streamInfo == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Failde to get video stream"));
                Console.WriteLine("Failed to get video stream");
                return;
            }

            var videoFilePath = $"{video.Id}.{streamInfo.Container.Name}";

            using (var output = File.OpenWrite(videoFilePath))
            using (var input = await _youtubeClient.Videos.Streams.GetAsync(streamInfo))
            {
                await input.CopyToAsync(output);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Here's the video you requested")
                    .AddFile(File.OpenRead(videoFilePath)));

            Console.WriteLine("Video was sended");
            File.Delete(videoFilePath);
        }
    }
}

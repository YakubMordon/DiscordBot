using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.SLCommands
{
    public class GroupSL : ApplicationCommandModule
    {
        [SlashCommand("poll", "Creates a poll with a question and options")]
        public async Task Poll(InteractionContext ctx,
    [Option("question", "The question for the poll")]
    string question,
    [Option("options", "Options for the poll separated by comma")]
    string options)
        {
            Console.WriteLine("Entering /poll command");
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(options))
            {
                Console.WriteLine("There are no arguments");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("There are no arguments"));
                return;
            }

            // Split the options into individual option strings
            string[] optionArray = options.Split(',');

            // Ensure there are at least two options
            if (optionArray.Length < 2)
            {
                Console.WriteLine("Please provide at least two options for the poll.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("Please provide at least two options for the poll."));
                return;
            }

            // Create the poll embed
            var pollEmbed = new DiscordEmbedBuilder()
            {
                Title = "Poll: " + question,
                Color = ctx.Client.CurrentUser.BannerColor ?? DiscordColor.Blue
            };

            StringBuilder description = new StringBuilder();

            // Add the options under the question
            for (int i = 0; i < optionArray.Length; i++)
            {
                string optionLetter = ((char)(65 + i)).ToString();
                description.Append($"{optionLetter}. {optionArray[i]}\n");
            }

            pollEmbed.Description = description.ToString();

            // Send the poll message with the embed
            var pollMessage = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(pollEmbed)).ConfigureAwait(false);

            Console.WriteLine("Poll created");

            // Add reactions to the poll message for each option
            for (int i = 0; i < optionArray.Length; i++)
            {
                string emoji = GetEmoji(i);
                await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emoji)).ConfigureAwait(false);
            }

            Console.WriteLine("Poll reactions created");
        }

        private string GetEmoji(int index)
        {
            // Define a list of emojis for the options (A, B, C, D, ...)
            List<string> emojis = new List<string>()
    {
        ":regional_indicator_a:",
        ":regional_indicator_b:",
        ":regional_indicator_c:",
        ":regional_indicator_d:",
        ":regional_indicator_e:",
        ":regional_indicator_f:",
        ":regional_indicator_g:",
        ":regional_indicator_h:",
        ":regional_indicator_i:",
        ":regional_indicator_j:",
        ":regional_indicator_k:",
        ":regional_indicator_l:",
        ":regional_indicator_m:",
        ":regional_indicator_n:",
        ":regional_indicator_o:",
        ":regional_indicator_p:",
        ":regional_indicator_q:",
        ":regional_indicator_r:",
        ":regional_indicator_s:",
        ":regional_indicator_t:",
        ":regional_indicator_u:",
        ":regional_indicator_v:",
        ":regional_indicator_w:",
        ":regional_indicator_x:",
        ":regional_indicator_y:",
        ":regional_indicator_z:"
    };

            return index < emojis.Count ? emojis[index] : (index + 1).ToString();
        }

    }
}

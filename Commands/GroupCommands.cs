using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot.Commands
{
    public class GroupCommands : BaseCommandModule
    {
        [Command("poll")]
        [Description("Creates a poll, first argument is always a question, other are options, you need to write at least 2 options, separate them by comma")]
        public async Task Poll(CommandContext ctx, [Description("String which contains question and options\nQuestion is First argument. Others are options of answer")][RemainingText] string pollTitle)
        {
            Console.WriteLine("Entering ?poll command");
            if (string.IsNullOrEmpty(pollTitle))
            {
                Console.WriteLine("There are no arguments");
                await ctx.RespondAsync("There are no arguments");
                return;
            }

            // Split the remaining text into individual options
            string[] options = pollTitle.Split(',');

            // Ensure there are at least two options
            if (options.Length < 3)
            {
                Console.WriteLine("Please provide at least two options and a question for the poll.");
                await ctx.RespondAsync("Please provide at least two options and a question for the poll.");
                return;
            }

            // Create the poll embed
            var pollEmbed = new DiscordEmbedBuilder()
            {
                Title = "Poll: " + options[0],
                Color = ctx.Client.CurrentUser.BannerColor ?? DiscordColor.Blue
            };

            StringBuilder description = new StringBuilder();

            // Add the options under the question
            for (int i = 0; i < options.Length - 1; i++)
            {
                string optionLetter = ((char)(65 + i)).ToString();
                description.Append($"{optionLetter}. {options[i + 1]}\n");
            }

            pollEmbed.Description = description.ToString();

            // Send the poll message with the embed
            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            Console.WriteLine("Poll created");

            // Add reactions to the poll message for each option
            for (int i = 0; i < options.Length - 1; i++)
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

namespace DiscordBot
{

    /// <summary>
    /// Головний клас у програмі.Клас Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Метод у якому розпочинаємо виконання програми та ініціалізуємо бота
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}

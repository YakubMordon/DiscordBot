using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    /// <summary>
    /// Клас для перевірки url-адреси
    /// </summary>
    public static class UrlChecker
    {
        /// <summary>
        /// Перевіряє, чи вказана URL-адреса належить до відеохостингу YouTube.
        /// </summary>
        /// <param name="url">URL-адреса для перевірки.</param>
        /// <returns>
        /// Значення true, якщо URL-адреса належить до YouTube; в іншому випадку - значення false.
        /// </returns>
        public static bool IsYouTubeUrl(string url)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com|youtu\.be)/.+$";

            return Regex.IsMatch(url, pattern);
        }

        /// <summary>
        /// Перевіряє, чи вказана URL-адреса належить до Spotify.
        /// </summary>
        /// <param name="url">URL-адреса для перевірки.</param>
        /// <returns>
        /// Значення true, якщо URL-адреса належить до Spotify; в іншому випадку - значення false.
        /// </returns>
        public static bool IsSpotifyUrl(string url)
        {
            string pattern = @"https:\/\/open\.spotify\.com\/track\/([a-zA-Z0-9]+)";

            return Regex.IsMatch(url, pattern);
        }
    }
}

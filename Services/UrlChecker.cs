using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public static class UrlChecker
    {
        public static bool IsYouTubeUrl(string url)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com|youtu\.be)/.+$";

            return Regex.IsMatch(url, pattern);
        }

        public static bool IsSpotifyUrl(string url)
        {
            string pattern = @"https:\/\/open\.spotify\.com\/track\/([a-zA-Z0-9]+)";

            return Regex.IsMatch(url, pattern);
        }
    }
}

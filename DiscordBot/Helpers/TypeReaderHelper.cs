using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordBotFanatic.Helpers {
    public static class TypeReaderHelper {
        public static IEnumerable<string> ToCollectionOfParameters(this string input) {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Select(m => m.Value.Replace("\"", ""));
        }

        public static bool IsValidUrl(this string urlString) {
            Uri uri;
            return Uri.TryCreate(urlString, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile);
        }
    }
}
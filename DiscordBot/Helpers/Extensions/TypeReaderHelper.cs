using System.Text;

namespace DiscordBot.Helpers.Extensions; 

public static class TypeReaderHelper {
    //https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
    public static IEnumerable<string> ToCollectionOfParameters(this string input) {
        input = input.Replace("><", "> <"); // Fix users on one line. perhaps we can replace this with regex as this is defn buggy (users with a name including '><' for example
        var result = new StringBuilder();

        var quoted = false;
        var escaped = false;
        var started = false;
        var allowcaret = false;
        for (var i = 0; i < input.Length; i++) {
            var chr = input[i];

            if (chr == '^' && !quoted) {
                if (allowcaret) {
                    result.Append(chr);
                    started = true;
                    escaped = false;
                    allowcaret = false;
                } else if (i + 1 < input.Length && input[i + 1] == '^') {
                    allowcaret = true;
                } else if (i + 1 == input.Length) {
                    result.Append(chr);
                    started = true;
                    escaped = false;
                }
            } else if (escaped) {
                result.Append(chr);
                started = true;
                escaped = false;
            } else if (chr == '"') {
                quoted = !quoted;
                started = true;
            } else if (chr == '\\' && i + 1 < input.Length && input[i + 1] == '"') {
                escaped = true;
            } else if (chr == ' ' && !quoted) {
                if (started) {
                    yield return result.ToString();
                }

                result.Clear();
                started = false;
            } else {
                result.Append(chr);
                started = true;
            }
        }

        if (started) {
            yield return result.ToString();
        }
    }

    public static bool IsValidUrl(this string urlString) {
        Uri uri;
        return Uri.TryCreate(urlString, UriKind.Absolute, out uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile);
    }
}
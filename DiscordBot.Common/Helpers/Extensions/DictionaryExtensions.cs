namespace DiscordBot.Common.Helpers.Extensions; 

public static class DictionaryExtensions {
    public static Dictionary<T, List<U>> Insert<T, U>(this Dictionary<T, List<U>> dict, T key, U value) {
        if (!dict.TryGetValue(key, out var list)) {
            list = new List<U>() {
                value
            };
            dict.Add(key, list);
            return dict;
        }

        list.Add(value);
        return dict;
    }

    public static Dictionary<U, IEnumerable<T>> ReverseDictionary<T, U>(this Dictionary<T, IEnumerable<U>> dict) {
        // creates middle table
        var middleTable = dict.SelectMany(x => x.Value, (entry, elem) => new { entry.Key, elem });
        
        // regroup
        return middleTable
            .GroupBy(x => x.elem, x => x.Key, (elem, key) => new { elem, key })
            .ToDictionary(x => x.elem, x => x.key);
    }
}

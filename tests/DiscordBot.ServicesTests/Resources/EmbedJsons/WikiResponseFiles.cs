using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.ServicesTests.Resources.EmbedJsons {
    public class WikiResponseFiles {
        public static IEnumerable<File> Files =>
            new List<File> {
                new(@"Resources/EmbedJsons/OrsrWiki_CollectionLog_response.json", 1,1326)
            };
        public static IEnumerable<object[]> AllFiles => Files.Select(x => new object[] {x.Path, x.Pages});
        public static IEnumerable<object[]> AllFilesWithQuantities => Files.Select(x => new object[] {x.Path, x.Pages, x.ItemQuantities});

        public class File {
            public File(string path, int pages, int itemQuantities) {
                Path = path;
                Pages = pages;
                ItemQuantities = itemQuantities;
            }

            public string Path { get; }
            public int Pages { get; }
            public int ItemQuantities { get; }
        }
    }
}

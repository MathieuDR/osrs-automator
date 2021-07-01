using System;
using System.Collections.Generic;

namespace DiscordBot.Helpers {
    public static class CommonHelper {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize) {
            if (chunksize < 1) throw new InvalidOperationException();
            var wrapper = new EnumeratorWrapper<T>(source);
            int currentPos = 0;

            try {
                wrapper.AddRef();
                while (wrapper.Get(currentPos, out _)) {
                    yield return new ChunkedEnumerable<T>(wrapper, chunksize, currentPos);
                    currentPos += chunksize;
                }
            }
            finally {
                wrapper.RemoveRef();
            }
        }
    }
}
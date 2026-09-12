using LiteDB;

namespace DiscordBot.Data;

public sealed class DatabaseLease : IDisposable {
    private readonly LiteDbManager _manager;
    private readonly LiteDatabase _database;
    private int _disposed;

    internal DatabaseLease(LiteDbManager manager, string filePath, LiteDatabase database) {
        _manager = manager;
        FilePath = filePath;
        _database = database;
    }

    public string FilePath { get; }

    public LiteDatabase Database {
        get {
            if (Interlocked.CompareExchange(ref _disposed, 0, 0) != 0) {
                throw new ObjectDisposedException(nameof(DatabaseLease));
            }

            return _database;
        }
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) {
            return;
        }

        _manager.Release(FilePath);
    }
}

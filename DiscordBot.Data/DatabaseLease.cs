using LiteDB;

namespace DiscordBot.Data;

public sealed class DatabaseLease : IDisposable {
    private readonly LiteDbManager _manager;
    private readonly LiteDatabase _database;
    // The exact Entry this lease was created against, so Release() can refuse to touch a
    // different (newer) entry that may since have been cached for the same path - see
    // LiteDbManager.Release().
    private readonly LiteDbManager.Entry _entry;
    private int _disposed;

    internal DatabaseLease(LiteDbManager manager, string filePath, LiteDatabase database, LiteDbManager.Entry entry) {
        _manager = manager;
        FilePath = filePath;
        _database = database;
        _entry = entry;
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

        _manager.Release(FilePath, _entry);
    }
}

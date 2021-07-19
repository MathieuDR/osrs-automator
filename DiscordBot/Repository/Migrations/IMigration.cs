using LiteDB;

namespace DiscordBot.Repository.Migrations {
    public interface IMigration {
        int Version { get; }
        void Up(LiteDatabase database);
        void Down(LiteDatabase database);
    }
}

using LiteDB;

namespace DiscordBot.Data.Repository.Migrations {
    public interface IMigration {
        int Version { get; }
        void Up(LiteDatabase database);
        void Down(LiteDatabase database);
    }
}

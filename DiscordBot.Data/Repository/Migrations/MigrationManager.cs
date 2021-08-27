using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Serilog;

namespace DiscordBot.Data.Repository.Migrations {
    public class MigrationManager {
        private readonly ILogger _logger;
        private readonly List<IMigration> _migrations;

        private int? _currentMigration;

        public MigrationManager(ILogger logger) {
            _logger = logger;
            _migrations = new List<IMigration>();
            _migrations.Add(new MigrationToBetterCountModels(logger));
            Validate();
        }

        public int CurrentMigration => _currentMigration ??= _migrations.Max(x => x.Version);

        private void Validate() {
            var hasMultiple = _migrations.GroupBy(x => x.Version).Where(x => x.Count() > 1).ToList();

            if (hasMultiple.Any()) {
                throw new Exception(
                    $"Multiple of following migration versions = {string.Join(",", hasMultiple.Select(x => x.Key.ToString()))}");
            }

            for (var i = 0; i < CurrentMigration; i++) {
                var migration = GetMigration(i + 1);
                if (migration is null) {
                    throw new Exception($"Cannot find migration version {i + 1}");
                }
            }
        }

        public void Migrate(LiteDatabase liteDatabase, int? migrateTo = null) {
            var toVersion = migrateTo ?? CurrentMigration;

            var currentVersion = liteDatabase.UserVersion;
            if (currentVersion == toVersion) {
                return;
            }

            // Ups
            for (var version = currentVersion; version < toVersion; version++) {
                var migration = GetMigration(version + 1);
                migration.Up(liteDatabase);
                currentVersion = migration.Version;
            }

            // Downs
            for (var version = currentVersion; version > toVersion; version--) {
                var migration = GetMigration(version);
                migration.Down(liteDatabase);
                currentVersion = migration.Version - 1;
            }
        }

        private IMigration GetMigration(int version) {
            if (version > CurrentMigration) {
                throw new ArgumentException("Migration is above supported migration");
            }

            return _migrations.FirstOrDefault(x => x.Version == version);
        }
    }
}

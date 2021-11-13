using System;
using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters; 

internal class StringToPlayerBuildConverter : ITypeConverter<string, PlayerBuild> {
    public PlayerBuild Convert(string source, PlayerBuild destination, ResolutionContext context) {
        //var lowerInvariant = source.ToLowerInvariant();
        if (Enum.TryParse(typeof(PlayerBuild), source, true, out var temp)) {
            destination = (PlayerBuild) temp;
            return destination;
        }


        if (string.IsNullOrWhiteSpace(source)) {
            destination = PlayerBuild.Main;
        } else {
            var lowerInvariant = source.ToLowerInvariant();
            destination = lowerInvariant switch {
                "main" => PlayerBuild.Main,
                "1def" => PlayerBuild.Def1,
                "lvl3" => PlayerBuild.Lvl3,
                "10hp" => PlayerBuild.Hp10,
                "f2p" => PlayerBuild.F2P,
                "zerker" => PlayerBuild.Zerker,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }

        return destination;
    }
}
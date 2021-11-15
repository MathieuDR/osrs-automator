using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters;

internal class StringToPlayerTypeConverter : ITypeConverter<string, PlayerType> {
    public PlayerType Convert(string source, PlayerType destination, ResolutionContext context) {
        //var lowerInvariant = source.ToLowerInvariant();
        if (Enum.TryParse(typeof(PlayerType), source, true, out var temp)) {
            destination = (PlayerType)temp;
            return destination;
        }


        if (string.IsNullOrWhiteSpace(source)) {
            destination = PlayerType.Unknown;
        } else {
            var lowerInvariant = source.ToLowerInvariant();
            destination = lowerInvariant switch {
                "hardcore" => PlayerType.HardcoreIronMan,
                "ultimate" => PlayerType.UltimateIronMan,
                "ironman" => PlayerType.IronMan,
                "regular" => PlayerType.Regular,
                "unknown" => PlayerType.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }

        return destination;
    }
}

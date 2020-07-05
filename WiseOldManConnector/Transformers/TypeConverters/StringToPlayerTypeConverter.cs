using System;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class StringToPlayerTypeConverter : ITypeConverter<string, PlayerType> {
        public PlayerType Convert(string source, PlayerType destination, ResolutionContext context) {
            //var lowerInvariant = source.ToLowerInvariant();
            if (Enum.TryParse(typeof(PlayerType), source, true, out object temp)) {
                destination = (PlayerType) temp;
                return destination;
            }

            var lowerInvariant = source.ToLowerInvariant();

            if (lowerInvariant == "hardcore") {
                destination = PlayerType.HardcoreIronMan;
            } else if (lowerInvariant == "ultimate") {
                destination = PlayerType.UltimateIronMan;
            } else if (lowerInvariant == "ironman") {
                destination = PlayerType.IronMan;
            } else if (lowerInvariant == "regular") {
                destination = PlayerType.Regular;
            } else if (lowerInvariant == "unknown") {
                destination = PlayerType.Unknown;
            } else {
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            return destination;
        }
    }
}
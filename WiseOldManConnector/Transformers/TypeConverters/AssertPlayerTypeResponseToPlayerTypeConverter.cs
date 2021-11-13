using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters; 

internal class AssertPlayerTypeResponseToPlayerTypeConverter : ITypeConverter<AssertPlayerTypeResponse, PlayerType> {
    public PlayerType Convert(AssertPlayerTypeResponse source, PlayerType destination, ResolutionContext context) {
        destination = context.Mapper.Map<PlayerType>(source.PlayerType);
        return destination;
    }
}
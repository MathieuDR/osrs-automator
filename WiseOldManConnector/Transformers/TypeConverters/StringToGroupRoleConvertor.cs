using AutoMapper;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters;

internal class StringToGroupRoleConvertor : ITypeConverter<string, GroupRole?> {
    public GroupRole? Convert(string source, GroupRole? destination, ResolutionContext context) {
        if (string.IsNullOrEmpty(source)) {
            return null;
        }

        //var lowerInvariant = source.ToLowerInvariant();
        if (Enum.TryParse(typeof(GroupRole), source, true, out var temp)) {
            destination = (GroupRole)temp;
            return destination;
        }

        destination = source.ToEnum<GroupRole>();

        return destination;
    }
}

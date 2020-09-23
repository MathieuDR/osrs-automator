using System;
using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class StringToGroupRoleConvertor : ITypeConverter<string, GroupRole?> {
        public GroupRole? Convert(string source, GroupRole? destination, ResolutionContext context) {
            if (string.IsNullOrEmpty(source)) {
                return null;
            }
            
            //var lowerInvariant = source.ToLowerInvariant();
            if (Enum.TryParse(typeof(GroupRole), source, true, out object temp)) {
                destination = (GroupRole) temp;
                return destination;
            }


            
            var lowerInvariant = source.ToLowerInvariant();
            destination = lowerInvariant switch {
                "member" => GroupRole.Member,
                "leader" => GroupRole.Leader,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
            

            return destination;
        }
    }
}
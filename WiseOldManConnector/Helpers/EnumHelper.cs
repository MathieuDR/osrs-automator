using System.Linq;
using System.Runtime.Serialization;

namespace WiseOldManConnector.Helpers {
    public static class EnumHelper {
        public static string GetEnumValueNameOrDefault<T>(this T enumVal) {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();

            if (attr != null) {
                return attr.Value;
            }

            return enumVal.ToString();
        }
    }
}
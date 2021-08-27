using AutoMapper;
using WiseOldManConnector.Models.API.Responses;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class AssertDisplayNameResponseToStringConverter : ITypeConverter<AssertDisplayNameResponse, string> {
        public string Convert(AssertDisplayNameResponse source, string destination, ResolutionContext context) {
            destination = source.DisplayName;
            return destination;
        }
    }
}

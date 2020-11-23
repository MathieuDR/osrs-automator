using System.Collections.Generic;

namespace WiseOldManConnector.Models {
    public class ConnectorCollectionResponse<T> : ConnectorResponseBase {
        internal ConnectorCollectionResponse(T data) : this(new List<T>() {data}, null) { }
        internal ConnectorCollectionResponse(IEnumerable<T> data) : this(data, null) { }

        internal ConnectorCollectionResponse(T data, string message) : this(new List<T>() {data}, message) { }

        internal ConnectorCollectionResponse(IEnumerable<T> data, string message) : base(message) {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; }
    }
}
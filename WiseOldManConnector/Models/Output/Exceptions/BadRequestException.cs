using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace WiseOldManConnector.Models.Output.Exceptions {
    public class BadRequestException : ApiException {
        public BadRequestException(string wiseOldManMessage, string resource, HttpStatusCode statusCode, List<Parameter> parameters) : base(
            wiseOldManMessage, resource, statusCode, parameters) { }

        public BadRequestException(string wiseOldManMessage, string resource, HttpStatusCode statusCode, List<Parameter> parameters,
            Exception innerException) : base(wiseOldManMessage, resource, statusCode, parameters, innerException) { }

        public BadRequestException(string wiseOldManMessage, IRestResponse response) : base(wiseOldManMessage, response) { }

        public BadRequestException(string wiseOldManMessage, IRestResponse response, Exception innerException) : base(wiseOldManMessage, response,
            innerException) { }
    }
}

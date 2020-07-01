
using System;
using System.Collections.Generic;
using RestSharp;

namespace WiseOldManConnector.Models.Output.Exceptions {
    public class BadRequestException : ApiException {
        public BadRequestException(string wiseOldManMessage, string resource, List<Parameter> parameters) : base(wiseOldManMessage, resource, parameters) { }
        public BadRequestException(string wiseOldManMessage, string resource, List<Parameter> parameters, Exception innerException) : base(wiseOldManMessage, resource, parameters, innerException) { }
        public BadRequestException(string wiseOldManMessage, IRestResponse response) : base(wiseOldManMessage, response) { }
        public BadRequestException(string wiseOldManMessage, IRestResponse response, Exception innerException) : base(wiseOldManMessage, response, innerException) { }
    }
}
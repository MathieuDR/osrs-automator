using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RestSharp;

namespace WiseOldManConnector.Models.Output.Exceptions {
    public abstract class ApiException : Exception {
        public string WiseOldManMessage { get; set; }
        public string Resource { get; set; }
        public List<Parameter> Parameters { get; set; }

        protected ApiException() { }

        protected ApiException(string wiseOldManMessage, string resource, List<Parameter> parameters) : this(wiseOldManMessage, resource, parameters, null) { }

        protected ApiException(string wiseOldManMessage, string resource, List<Parameter> parameters, Exception innerException) : base(wiseOldManMessage, innerException) {
            Resource = resource;
            Parameters = parameters;
            WiseOldManMessage = wiseOldManMessage;
        }

        protected ApiException(string wiseOldManMessage, IRestResponse response) : this(wiseOldManMessage, response.Request.Resource, response.Request.Parameters) { }
        protected ApiException(string wiseOldManMessage, IRestResponse response, Exception innerException) : this(wiseOldManMessage, response.Request.Resource, response.Request.Parameters, innerException) { }
       


    }
}
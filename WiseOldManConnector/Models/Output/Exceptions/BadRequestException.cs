using System.Net;
using RestSharp;

namespace WiseOldManConnector.Models.Output.Exceptions;

public class BadRequestException : ApiException {
    public BadRequestException(string wiseOldManMessage, string resource, HttpStatusCode statusCode, ParametersCollection parameters) : base(
        wiseOldManMessage, resource, statusCode, parameters) { }

    public BadRequestException(string wiseOldManMessage, string resource, HttpStatusCode statusCode,ParametersCollection parameters,
        Exception innerException) : base(wiseOldManMessage, resource, statusCode, parameters, innerException) { }

    public BadRequestException(string wiseOldManMessage, RestResponse response) : base(wiseOldManMessage, response) { }

    public BadRequestException(string wiseOldManMessage, RestResponse response, Exception innerException) : base(wiseOldManMessage, response,
        innerException) { }
}

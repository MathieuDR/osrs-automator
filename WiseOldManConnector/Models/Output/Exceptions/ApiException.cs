using System.Net;
using RestSharp;

namespace WiseOldManConnector.Models.Output.Exceptions;

public abstract class ApiException : Exception {
    protected ApiException() { }

    protected ApiException(string wiseOldManMessage, string resource, HttpStatusCode statusCode, ParametersCollection parameters) :
        this(wiseOldManMessage, resource, statusCode, parameters, null) { }

    protected ApiException(string wiseOldManMessage, string resource, HttpStatusCode statusCode, ParametersCollection parameters,
        Exception innerException) : base(wiseOldManMessage, innerException) {
        Resource = resource;
        Parameters = parameters;
        WiseOldManMessage = wiseOldManMessage;
        StatusCode = statusCode;
    }

    protected ApiException(string wiseOldManMessage, RestResponse response) : this(wiseOldManMessage,
        response.Request?.Resource, response.StatusCode, response.Request.Parameters) { }

    protected ApiException(string wiseOldManMessage, RestResponse response, Exception innerException) : this(
        wiseOldManMessage, response.Request.Resource, response.StatusCode, response.Request.Parameters, innerException) { }

    public string WiseOldManMessage { get; set; }
    public string Resource { get; set; }
    public ParametersCollection Parameters { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

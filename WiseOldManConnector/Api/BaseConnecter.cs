using System.Net;
using AutoMapper;
using MathieuDR.Common.Semaphores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output.Exceptions;

namespace WiseOldManConnector.Api;

internal abstract class BaseConnecter {
    protected const string BaseUrl = "https://api.wiseoldman.net/v2";
    private readonly RestClient Client;
    protected readonly IWiseOldManLogger Logger;
    protected readonly Mapper Mapper;
    protected readonly TimeSpanSemaphore Semaphore;

    protected BaseConnecter(IServiceProvider provider) {
        Logger = provider.GetService(typeof(IWiseOldManLogger)) as IWiseOldManLogger;

        var settings = provider.GetService<IOptions<WiseOldManOptions>>();
        var userAgent = settings?.Value.UserAgent ?? "OSRS - Automator";
        var opts = new RestClientOptions(BaseUrl) {
            UserAgent = userAgent
        };
        
        Client = new RestClient(opts);
        if (settings is not null && !string.IsNullOrWhiteSpace(settings.Value.ApiKey)) {
            Client.AddDefaultHeader("x-api-key", settings.Value.ApiKey);
        }


        Client.UseNewtonsoftJson();
        Mapper = Transformers.Configuration.GetMapper();
        Semaphore = provider.GetRequiredService<TimeSpanSemaphore>();
    }

    protected abstract string Area { get; }

    private void LogRequest(RestRequest request) {
        Logger?.Log(LogLevel.Information, null, "Request sent to Wise old man API. [{Resource}, {@Parameters:j}]", request.Resource,
            request.Parameters);
    }


    private void LogResponse(RestResponse response) {
        Logger?.Log(LogLevel.Information, null, "Response received from Wise Old Man API. [{Resource}, {Content:j}]", response.Content,
            response.Request?.Resource);
    }

    protected async Task<T> ExecuteRequest<T>(RestRequest request) where T : IResponse {
        LogRequest(request);
        var result = await Semaphore.RunAsync(() => Client.ExecuteAsync<T>(request));
        LogResponse(result);

        ValidateResponse(result);
        return result.Data;
    }

    protected async Task<IEnumerable<T>> ExecuteCollectionRequest<T>(RestRequest request) where T : IResponse {
        LogRequest(request);
        var result = await Semaphore.RunAsync(() => Client.ExecuteAsync<List<T>>(request));
        LogResponse(result);

        ValidateResponse(result);
        return result.Data;
    }

    protected ConnectorCollectionResponse<T> GetResponse<TU, T>(IEnumerable<TU> collection) where TU : IResponse {
        var mappedCollection = Mapper.Map<IEnumerable<TU>, IEnumerable<T>>(collection);
        return new ConnectorCollectionResponse<T>(mappedCollection);
    }

    protected ConnectorCollectionResponse<T> GetCollectionResponse<T>(object data) {
        var mappedCollection = Mapper.Map<IEnumerable<T>>(data);
        return new ConnectorCollectionResponse<T>(mappedCollection);
    }

    protected ConnectorResponse<T> GetResponse<T>(object data) {
        var mapped = Mapper.Map<T>(data);
        return new ConnectorResponse<T>(mapped);
    }

    protected RestRequest GetNewRestRequest() {
        return GetNewRestRequest("");
    }

    protected RestRequest GetNewRestRequest(string resourcePath) {
        var resource = $"{Area}";
        if (!string.IsNullOrEmpty(resource)) {
            resource = $"{resource}/{resourcePath}";
        }

        return new RestRequest(resource) {
            RequestFormat = DataFormat.Json
        };
    }

    private void ValidateResponse<T>(RestResponse<T> response) {
        if (response == null) {
            throw new NullReferenceException("We did not receive a response. Please try again later or contact the administration.");
        }

        if (response.ErrorException != null) {
            Logger?.Log(LogLevel.Error, null, "Error for [{Resource}, {Parameters:j}]", response.Request?.Resource, response.Request?.Parameters);
            throw new BadRequestException(response.ErrorException.Message, response);
        }

        switch (response.StatusCode) {
            case HttpStatusCode.Accepted:
            case HttpStatusCode.NoContent:
            case HttpStatusCode.Created:
            case HttpStatusCode.PartialContent:
            case HttpStatusCode.OK:
                break;
            case HttpStatusCode.Ambiguous:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Conflict:
            case HttpStatusCode.Continue:
            case HttpStatusCode.ExpectationFailed:
            case HttpStatusCode.Forbidden:
            case HttpStatusCode.Found:
            case HttpStatusCode.GatewayTimeout:
            case HttpStatusCode.Gone:
            case HttpStatusCode.HttpVersionNotSupported:
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.LengthRequired:
            case HttpStatusCode.MethodNotAllowed:
            case HttpStatusCode.Moved:
            case HttpStatusCode.NonAuthoritativeInformation:
            case HttpStatusCode.NotAcceptable:
            case HttpStatusCode.NotFound:
            case HttpStatusCode.NotImplemented:
            case HttpStatusCode.NotModified:
            case HttpStatusCode.PaymentRequired:
            case HttpStatusCode.PreconditionFailed:
            case HttpStatusCode.ProxyAuthenticationRequired:
            case HttpStatusCode.RedirectKeepVerb:
            case HttpStatusCode.RedirectMethod:
            case HttpStatusCode.RequestedRangeNotSatisfiable:
            case HttpStatusCode.RequestEntityTooLarge:
            case HttpStatusCode.RequestTimeout:
            case HttpStatusCode.RequestUriTooLong:
            case HttpStatusCode.ResetContent:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.SwitchingProtocols:
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.UnsupportedMediaType:
            case HttpStatusCode.Unused:
            case HttpStatusCode.UpgradeRequired:
            case HttpStatusCode.UseProxy:
            case HttpStatusCode.TooManyRequests:
                var responseMessage = "";
                var data = response.Data ?? (object)JsonConvert.DeserializeObject<WOMMessageResponse>(response.Content);

                switch (data) {
                    case BaseResponse messageResponse:
                        responseMessage = messageResponse.Message;
                        break;
                    case IEnumerable<WOMMessageResponse> collectionBaseResponses:
                        responseMessage = string.Join(", ", collectionBaseResponses.Select(x => x.Message).ToArray());
                        break;
                }

                Logger?.Log(LogLevel.Error, null, "Error for [{Resource}, {Parameters:j}]", response.Request.Resource,
                    response.Request.Parameters);
                throw new BadRequestException(responseMessage, response);
            case HttpStatusCode.AlreadyReported:
                break;
            case HttpStatusCode.EarlyHints:
                break;
            case HttpStatusCode.FailedDependency:
                break;
            case HttpStatusCode.IMUsed:
                break;
            case HttpStatusCode.InsufficientStorage:
                break;
            case HttpStatusCode.Locked:
                break;
            case HttpStatusCode.LoopDetected:
                break;
            case HttpStatusCode.MisdirectedRequest:
                break;
            case HttpStatusCode.MultiStatus:
                break;
            case HttpStatusCode.NetworkAuthenticationRequired:
                break;
            case HttpStatusCode.NotExtended:
                break;
            case HttpStatusCode.PermanentRedirect:
                break;
            case HttpStatusCode.PreconditionRequired:
                break;
            case HttpStatusCode.Processing:
                break;
            case HttpStatusCode.RequestHeaderFieldsTooLarge:
                break;
            case HttpStatusCode.UnavailableForLegalReasons:
                break;
            case HttpStatusCode.UnprocessableEntity:
                break;
            case HttpStatusCode.VariantAlsoNegotiates:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

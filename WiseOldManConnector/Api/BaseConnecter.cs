using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output.Exceptions;

namespace WiseOldManConnector.Api {
    internal abstract class BaseConnecter {
        protected const string BaseUrl = "https://wiseoldman.net/api";
        protected readonly RestClient Client;
        protected readonly IWiseOldManLogger Logger;
        protected readonly Mapper Mapper;

        protected BaseConnecter(IServiceProvider provider) {
            Logger = provider.GetService(typeof(IWiseOldManLogger)) as IWiseOldManLogger;
            Client = new RestClient(BaseUrl);
            Client.UseNewtonsoftJson();
            Mapper = Transformers.Configuration.GetMapper();
        }

        protected abstract string Area { get; }

        private void LogRequest(RestRequest request) {
            Logger?.Log(LogLevel.Information, null, "Request sent to Wise old man API. [{Resource}, {Parameters:j}]",
                request.Resource, request.Parameters);
        }

        protected async Task<T> ExecuteRequest<T>(RestRequest request) where T : BaseResponse {
            LogRequest(request);
            IRestResponse<T> result = await Client.ExecuteAsync<T>(request);

            ValidateResponse(result);
            return result.Data;
        }

        protected async Task<IEnumerable<T>> ExecuteCollectionRequest<T>(RestRequest request) where T : BaseResponse {
            LogRequest(request);
            IRestResponse<List<T>> result = await Client.ExecuteAsync<List<T>>(request);

            ValidateResponse(result);
            return result.Data;
        }

        protected ConnectorCollectionResponse<T> GetResponse<TU, T>(IEnumerable<TU> collection) where TU : BaseResponse {
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


        protected RestRequest GetNewRestRequest(string resourcePath) {
            string resource = $"{Area}";
            if (!string.IsNullOrEmpty(resource)) {
                resource = $"{resource}/{resourcePath}";
            }

            return new RestRequest(resource, DataFormat.Json);
        }

        private void ValidateResponse<T>(IRestResponse<T> response) {
            if (response == null) {
                // SHOULD NEVER HAPPEN I THINK
                throw new NullReferenceException($"We did not receive a response. Please try again later or contact the administration.");
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
                    string responseMessage = "";
                    object data = response.Data ?? (object) JsonConvert.DeserializeObject<BaseResponse>(response.Content);

                    switch (data) {
                        case BaseResponse baseResponse:
                            responseMessage = baseResponse.Message;
                            break;
                        case IEnumerable<BaseResponse> collectionBaseResponses:
                            responseMessage = string.Join(", ", collectionBaseResponses.Select(x => x.Message).ToArray());
                            break;
                    }

                    throw new BadRequestException(responseMessage, response);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api {
    internal class NameConnector: BaseConnecter, IWiseOldManNameApi {
        public NameConnector(IServiceProvider provider) : base(provider) { }
        protected override string Area { get; } = "names";
        
        public Task<ConnectorCollectionResponse<NameChange>> View(int limit = 20, int offset = 0) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<NameChange>> View(string username, int limit = 20, int offset = 0) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<NameChange>> View(NameChangeStatus status, int limit = 20, int offset = 0) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<NameChange>> View(string username, NameChangeStatus status, int limit = 20, int offset = 0) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<NameChange>> Request(string oldUsername, string newUsername) {
            var request = GetNewRestRequest();
            request.Method = Method.POST;
            request.AddJsonBody(new {
                oldName = oldUsername,
                newName = newUsername
            });

            var result = await ExecuteRequest<NameChangeResponse>(request);
            return GetResponse<NameChange>(result);
        }

        public async Task<ConnectorCollectionResponse<NameChange>> Request(IEnumerable<Tuple<string, string>> items) {
            var request = GetNewRestRequest();
            request.Method = Method.POST;
            
            request.AddJsonBody(items.Select(x => new {
                oldName = x.Item1,
                newName = x.Item2
            }));

            var result = await ExecuteCollectionRequest<NameChangeResponse>(request);
            return GetResponse<NameChangeResponse,NameChange>(result);
        }
    }
}

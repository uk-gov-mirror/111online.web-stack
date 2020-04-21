﻿using NHS111.Business.Configuration;
using NHS111.Models.Models.Domain;
using NHS111.Utils.RestTools;
using RestSharp;
using System.Threading.Tasks;

namespace NHS111.Business.Services
{
    public class VersionService : IVersionService
    {
        private readonly IConfiguration _configuration;
        private readonly IRestClient _restClient;

        public VersionService(IConfiguration configuration, IRestClient restClientDomainApi)
        {

            _configuration = configuration;
            _restClient = restClientDomainApi;
        }

        public async Task<VersionInfo> GetVersionInfo()
        {
            var version = await _restClient.ExecuteTaskAsync<VersionInfo>(new JsonRestRequest(_configuration.GetDomainApiVersionUrl(), Method.GET));
            return version.Data;
        }
    }

    public interface IVersionService
    {
        Task<VersionInfo> GetVersionInfo();
    }
}

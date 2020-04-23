﻿
using NHS111.Models.Models.Domain;
using NHS111.Utils.RestTools;
using RestSharp;

namespace NHS111.Business.Services
{
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CareAdviceService
        : ICareAdviceService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingRestClient _restClient;

        public CareAdviceService(IConfiguration configuration, ILoggingRestClient restClientDomainApi)
        {
            _configuration = configuration;
            _restClient = restClientDomainApi;
        }

        public async Task<IEnumerable<CareAdvice>> GetCareAdvice(int age, string gender, IEnumerable<string> markers)
        {
            var careAdvice = await _restClient.ExecuteAsync<IEnumerable<CareAdvice>>(new JsonRestRequest(_configuration.GetDomainApiCareAdviceUrl(age, gender, markers), Method.GET));
            return careAdvice.Data;
        }

        public async Task<IEnumerable<CareAdvice>> GetCareAdvice(string ageCategory, string gender, string keywords, string dxCode)
        {
            var domainApiCareAdviceUrl = _configuration.GetDomainApiCareAdviceUrl(dxCode, ageCategory, gender);
            var request = new JsonRestRequest(domainApiCareAdviceUrl, Method.POST);
            request.AddJsonBody(keywords);

            var response = await _restClient.ExecuteAsync<IEnumerable<CareAdvice>>(request);
            if (!response.IsSuccessful)
                throw new Exception(string.Format("A problem occured requesting {0}. {1}", domainApiCareAdviceUrl, response.ErrorMessage));

            return response.Data;
        }
    }

    public interface ICareAdviceService
    {
        Task<IEnumerable<CareAdvice>> GetCareAdvice(int age, string gender, IEnumerable<string> markers);
        Task<IEnumerable<CareAdvice>> GetCareAdvice(string ageCategory, string gender, string keywords, string dxCode);
    }
}
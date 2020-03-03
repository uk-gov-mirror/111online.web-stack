﻿using System;
using System.Net;
using System.Threading.Tasks;
using log4net;
using RestSharp;

namespace NHS111.Utils.RestTools
{
    public class LoggingRestClient : RestClient
    {
        private readonly ILog _logger;
        public LoggingRestClient(string baseUrl, ILog logger) : base(baseUrl)
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            _logger = logger;
            InitializeSerialisationHandlers();
        }

        private void InitializeSerialisationHandlers()
        {
            this.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
            this.AddHandler("text/json", NewtonsoftJsonSerializer.Default);
            this.AddHandler("text/x-json", NewtonsoftJsonSerializer.Default);
            this.AddHandler("text/javascript", NewtonsoftJsonSerializer.Default);
            this.AddHandler("*+json", NewtonsoftJsonSerializer.Default);
        }

        public override async Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request)
        {
            _logger.Info(string.Format("Request to: {0}{1} performed", BaseUrl,request.Resource));
            
            var response = await base.ExecuteTaskAsync<T>(request);

            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                var statusCode = response == null ? "500" : Convert.ToString(response.StatusCode);
                var errorMessage = response == null ? "Not Found" : Convert.ToString(response.ErrorMessage);

                _logger.Error(String.Format("Request to: {0}{1} returned with Error Code: {2} and response: {3}", BaseUrl, request.Resource, statusCode, errorMessage));
            }
            
            return response;
        }
    }
}

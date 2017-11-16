﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHS111.Business.DOS.Service;
using NUnit.Framework;
using RestSharp;

namespace NHS111.Business.DOS.Test.Service
{
    public class ITKAvailabilityFilterServiceTest
    {
        private Mock<Configuration.IConfiguration> _mockConfiguration;
        private Mock<IRestClient> _restClient;
        private readonly string _localServiceIdWhiteListUrl = "http://localhost/whitelist/{0}";
        private readonly string _serviceIdBlackListUrl = "http://localhost/blacklist/{0}";

        private static readonly string CheckCapacitySummaryResults = @"{
            ""CheckCapacitySummaryResult"": [{
                    ""idField"": 1419419101,
                    ""nameField"": ""Test Service 1"",
                    ""serviceTypeField"": {
                        ""idField"": 100,
                    }
                },
                {
                    ""idField"": 1419419102,
                    ""nameField"": ""Test Service 2"",
                    ""serviceTypeField"": {
                        ""idField"": 25,
                    }
                },
                {
                    ""idField"": 1419419103,
                    ""nameField"": ""Test Service 3"",
                    ""serviceTypeField"": {
                        ""idField"": 46,
                    }
                },
            ]}";

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<Configuration.IConfiguration>();
            _restClient = new Mock<IRestClient>();

            _mockConfiguration.Setup(c => c.CCGApiGetCCGByPostcode).Returns(_localServiceIdWhiteListUrl);
            _mockConfiguration.Setup(c => c.CCGApiGetServiceIdBlacklist).Returns(_serviceIdBlackListUrl);


        }

        [Test]
        public async void ITKAvailabilityFilter_CallbackEnabledForWhitelistedServiceId()
        {
            const string postcode = "SO302UN";
            string whitelist = string.Format(_localServiceIdWhiteListUrl, postcode);
            string blacklist = string.Format(_serviceIdBlackListUrl, postcode);
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(whitelist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 123, 456, 789, 1419419102 } }));
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(blacklist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 444, 555 } }));

            var jObj = (JObject)JsonConvert.DeserializeObject(CheckCapacitySummaryResults);
            var results = jObj["CheckCapacitySummaryResult"].ToObject<List<Models.Models.Business.DosService>>();

            //Act
            var sut = new ITKAvailabilityFilterService(_restClient.Object, _mockConfiguration.Object);
            var result = await sut.Filter(results, postcode);

            Assert.AreEqual(3, result.Count());
            Assert.IsFalse(result[0].CallbackEnabled);
            Assert.IsTrue(result[1].CallbackEnabled);
            Assert.IsFalse(result[2].CallbackEnabled);
        }

        [Test]
        public async void ITKAvailabilityFilter_EmptyWhitelist()
        {
            const string postcode = "SO302UN";
            string whitelist = string.Format(_localServiceIdWhiteListUrl, postcode);
            string blacklist = string.Format(_serviceIdBlackListUrl, postcode);
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(whitelist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { } }));
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(blacklist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 444, 555 } }));

            var jObj = (JObject)JsonConvert.DeserializeObject(CheckCapacitySummaryResults);
            var results = jObj["CheckCapacitySummaryResult"].ToObject<List<Models.Models.Business.DosService>>();

            //Act
            var sut = new ITKAvailabilityFilterService(_restClient.Object, _mockConfiguration.Object);
            var result = await sut.Filter(results, postcode);

            Assert.AreEqual(3, result.Count());
            Assert.IsFalse(result[0].CallbackEnabled);
            Assert.IsFalse(result[1].CallbackEnabled);
            Assert.IsFalse(result[2].CallbackEnabled);
        }

        [Test]
        public async void ITKAvailabilityFilter_EmptyBlacklist()
        {
            const string postcode = "SO302UN";
            string whitelist = string.Format(_localServiceIdWhiteListUrl, postcode);
            string blacklist = string.Format(_serviceIdBlackListUrl, postcode);
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(whitelist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 123,456,789 } }));
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(blacklist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> {  } }));

            var jObj = (JObject)JsonConvert.DeserializeObject(CheckCapacitySummaryResults);
            var results = jObj["CheckCapacitySummaryResult"].ToObject<List<Models.Models.Business.DosService>>();

            //Act
            var sut = new ITKAvailabilityFilterService(_restClient.Object, _mockConfiguration.Object);
            var result = await sut.Filter(results, postcode);

            Assert.AreEqual(3, result.Count());
            Assert.IsFalse(result[0].CallbackEnabled);
            Assert.IsFalse(result[1].CallbackEnabled);
            Assert.IsFalse(result[2].CallbackEnabled);
        }

        [Test]
        public async void ITKAvailabilityFilter_CallbackNotEnabledForNonWhitelistedServiceId()
        {
            const string postcode = "SO302UN";
            string whitelist = string.Format(_localServiceIdWhiteListUrl, postcode);
            string blacklist = string.Format(_serviceIdBlackListUrl, postcode);
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(whitelist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 123, 456, 789 } }));
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(blacklist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 444, 555 } }));

            var jObj = (JObject)JsonConvert.DeserializeObject(CheckCapacitySummaryResults);
            var results = jObj["CheckCapacitySummaryResult"].ToObject<List<Models.Models.Business.DosService>>();

            //Act
            var sut = new ITKAvailabilityFilterService(_restClient.Object, _mockConfiguration.Object);
            var result = await sut.Filter(results, postcode);

            Assert.AreEqual(3, result.Count());
            Assert.IsFalse(result[0].CallbackEnabled);
            Assert.IsFalse(result[1].CallbackEnabled);
            Assert.IsFalse(result[2].CallbackEnabled);
        }
        
        [Test]
        public async void ITKAvailabilityFilter_ServiceRemovedFromResultForBlacklistedServiceId()
        {
            const string postcode = "SO302UN";
            string whitelist = string.Format(_localServiceIdWhiteListUrl, postcode);
            string blacklist = string.Format(_serviceIdBlackListUrl, postcode);
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(whitelist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 123, 456, 789 } }));
            _restClient.Setup(r => r.ExecuteTaskAsync<List<int>>(It.Is<RestRequest>(req => req.Resource.Equals(blacklist)))).Returns(() => StartedTask((IRestResponse<List<int>>)new RestResponse<List<int>>() { ResponseStatus = ResponseStatus.Completed, Data = new List<int> { 444, 555, 1419419101 } }));

            var jObj = (JObject)JsonConvert.DeserializeObject(CheckCapacitySummaryResults);
            var results = jObj["CheckCapacitySummaryResult"].ToObject<List<Models.Models.Business.DosService>>();

            //Act
            var sut = new ITKAvailabilityFilterService(_restClient.Object, _mockConfiguration.Object);
            var result = await sut.Filter(results, postcode);

            Assert.AreEqual(2, result.Count());
            Assert.IsFalse(result[0].CallbackEnabled);
            Assert.IsFalse(result[1].CallbackEnabled);
            Assert.AreEqual(1419419102, result[0].Id);
            Assert.AreEqual(1419419103, result[1].Id);
        }
        
        private static Task<T> StartedTask<T>(T taskResult)
        {
            return Task<T>.Factory.StartNew(() => taskResult);
        }

        private static JObject GetJObjectFromResponse(HttpResponseMessage response)
        {
            var val = response.Content.ReadAsStringAsync().Result;
            return (JObject)JsonConvert.DeserializeObject(val);
        }
    }
}

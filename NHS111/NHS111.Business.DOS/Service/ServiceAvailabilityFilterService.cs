﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHS111.Business.DOS.Configuration;
using NHS111.Business.DOS.EndpointFilter;
using NHS111.Business.DOS.WhitelistFilter;
using NHS111.Models.Models.Web.DosRequests;
using NHS111.Features;
using NHS111.Models.Models.Business;
using NHS111.Models.Models.Web.Clock;

namespace NHS111.Business.DOS.Service
{
    public class ServiceAvailabilityFilterService : IServiceAvailabilityFilterService
    {
        private readonly IDosService _dosService;
        private readonly IConfiguration _configuration;
        private readonly IServiceAvailabilityManager _serviceAvailabilityManager;
        private readonly IFilterServicesFeature _filterServicesFeature;
        private readonly IServiceWhitelistFilter _serviceWhitelistFilter;
        private readonly IOnlineServiceTypeMapper _serviceTypeMapper;
        private readonly IOnlineServiceTypeFilter _serviceTypeFilter;
        private readonly IPublicHolidayService _publicHolidayService;
        private readonly ISearchDistanceService _searchDistanceService;

        public ServiceAvailabilityFilterService(IDosService dosService, IConfiguration configuration, IServiceAvailabilityManager serviceAvailabilityManager, IFilterServicesFeature filterServicesFeature, IServiceWhitelistFilter serviceWhitelistFilter, IOnlineServiceTypeMapper serviceTypeMapper, IOnlineServiceTypeFilter serviceTypeFilter, IPublicHolidayService publicHolidayService, ISearchDistanceService searchDistanceService)
        {
            _dosService = dosService;
            _configuration = configuration;
            _serviceAvailabilityManager = serviceAvailabilityManager;
            _filterServicesFeature = filterServicesFeature;
            _serviceWhitelistFilter = serviceWhitelistFilter;
            _serviceTypeMapper = serviceTypeMapper;
            _serviceTypeFilter = serviceTypeFilter;
            _publicHolidayService = publicHolidayService;
            _searchDistanceService = searchDistanceService;
        }

        public async Task<HttpResponseMessage> GetFilteredServices(HttpRequestMessage request, bool filterServices, DosEndpoint? endpoint)
        {
            var content = await request.Content.ReadAsStringAsync();

            var dosCase = GetObjectFromRequest<DosCase>(content);
            dosCase.SearchDistance = await _searchDistanceService.GetSearchDistanceByPostcode(dosCase.PostCode);

            var dosCaseRequest = BuildRequestMessage(dosCase);
            var originalPostcode = dosCase.PostCode;

            var response = await _dosService.GetServices(dosCaseRequest, endpoint);

            if (response.StatusCode != HttpStatusCode.OK) return response;

            var dosFilteredCase = GetObjectFromRequest<DosFilteredCase>(content);

            var val = await response.Content.ReadAsStringAsync();
            var jObj = (JObject)JsonConvert.DeserializeObject(val);
            var services = jObj["CheckCapacitySummaryResult"];
            
            // get the search datetime if one has been set, if not set to now
            DateTime searchDateTime;
            if (!DateTime.TryParse(dosCase.SearchDateTime, out searchDateTime)) 
                searchDateTime = DateTime.Now;

            // use dosserviceconvertor to specify the time to use for each dos service object
            var settings = new JsonSerializer();
            settings.Converters.Add(new DosServiceConverter(new SearchDateTimeClock(searchDateTime)));
            var results = services.ToObject<IList<Models.Models.Business.DosService>>(settings);

            var publicHolidayAjustedResults =
                _publicHolidayService.AdjustServiceRotaSessionOpeningForPublicHoliday(results);

            var filteredByServiceWhitelistResults = await _serviceWhitelistFilter.Filter(publicHolidayAjustedResults.ToList(), originalPostcode);
            var mappedByServiceTypeResults = await _serviceTypeMapper.Map(filteredByServiceWhitelistResults, originalPostcode);
            var filteredByUnknownTypeResults = _serviceTypeFilter.FilterUnknownTypes(mappedByServiceTypeResults);
        
            if (!_filterServicesFeature.IsEnabled && !filterServices)
            {
                return BuildResponseMessage(filteredByUnknownTypeResults);
            }
            var filteredByclosedCallbackTypeResults = _serviceTypeFilter.FilterClosedCallbackServices(filteredByUnknownTypeResults);
            var serviceAvailability = _serviceAvailabilityManager.FindServiceAvailability(dosFilteredCase);
            return BuildResponseMessage(serviceAvailability.Filter(filteredByclosedCallbackTypeResults));
        }


        public HttpRequestMessage BuildRequestMessage(DosCase dosCase)
        {
            var dosCheckCapacitySummaryRequest = new DosCheckCapacitySummaryRequest(_configuration.DosUsername, _configuration.DosPassword, dosCase);
            return new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(dosCheckCapacitySummaryRequest), Encoding.UTF8, "application/json") };
        }

        public HttpResponseMessage BuildResponseMessage(IEnumerable<Models.Models.Business.DosService> results)
        {
            var result = new JsonCheckCapacitySummaryResult(results);
            return new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json") };
        }

        public T GetObjectFromRequest<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }

    public interface IServiceAvailabilityFilterService
    {
        Task<HttpResponseMessage> GetFilteredServices(HttpRequestMessage request, bool filterServices, DosEndpoint? endpoint);
    }

    public class JsonCheckCapacitySummaryResult
    {
        private readonly CheckCapacitySummaryResult[] _checkCapacitySummaryResults;

        public JsonCheckCapacitySummaryResult(IEnumerable<Models.Models.Business.DosService> dosServices)
        {
            var serialisedServices = JsonConvert.SerializeObject(dosServices);
            _checkCapacitySummaryResults = JsonConvert.DeserializeObject<CheckCapacitySummaryResult[]>(serialisedServices);
        }

        [JsonProperty(PropertyName = "CheckCapacitySummaryResult")]
        public CheckCapacitySummaryResult[] CheckCapacitySummaryResults
        {
            get { return _checkCapacitySummaryResults; }
        }
    }

//TODO:: Put these in the right places VVVVVVV
    public class DosServiceConverter : JsonConverter
    {
        private readonly IClock _clock;

        public DosServiceConverter(IClock clock)
        {
            _clock = clock;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JArray
            var servicesArray = JArray.Load(reader);
            var dosServices = new List<Models.Models.Business.DosService>();
            foreach (var service in servicesArray)
            {
                var dosService = new Models.Models.Business.DosService(_clock);
                serializer.Populate(service.CreateReader(), dosService);
                dosServices.Add(dosService);
            }

            // Return the result
            return dosServices;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IList<Models.Models.Business.DosService>));
        }
    }

    public class SearchDateTimeClock : IClock
    {
        private readonly DateTime _searchDatetime;

        public SearchDateTimeClock(DateTime searchDatetime)
        {
            _searchDatetime = searchDatetime;
        }

        public DateTime Now
        {
            get { return _searchDatetime; }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web;
using NHS111.Models.Models.Web.Enums;
using NHS111.Models.Models.Web.FromExternalServices;
using NHS111.Utils.Parser;
using NHS111.Utils.RestTools;
using RestSharp;
using IConfiguration = NHS111.Web.Presentation.Configuration.IConfiguration;

namespace NHS111.Web.Presentation.Builders
{
    public class JustToBeSafeFirstViewModelBuilder : BaseBuilder, IJustToBeSafeFirstViewModelBuilder
    {
        private readonly IConfiguration _configuration;
        private readonly IMappingEngine _mappingEngine;
        private readonly IRestClient _restClient;
        private readonly IKeywordCollector _keywordCollector;
        private readonly IUserZoomDataBuilder _userZoomDataBuilder;

        public JustToBeSafeFirstViewModelBuilder(IRestClient restClient, IConfiguration configuration, IMappingEngine mappingEngine, IKeywordCollector keywordCollector, IUserZoomDataBuilder userZoomDataBuilder)
        {
            _restClient = restClient;
            _configuration = configuration;
            _mappingEngine = mappingEngine;
            _keywordCollector = keywordCollector;
            _userZoomDataBuilder = userZoomDataBuilder;
        }

        public async Task<Tuple<string, QuestionViewModel>> JustToBeSafeFirstBuilder(JustToBeSafeViewModel model)
        {
            if (model.PathwayId != null)
                model = await DoWorkPreviouslyDoneInQuestionBuilder(model); //todo refactor away

            var identifiedModel = await BuildIdentifiedModel(model);
            var questionsWithAnswers = await _restClient.ExecuteTaskAsync<IEnumerable<QuestionWithAnswers>>(new JsonRestRequest(_configuration.GetBusinessApiJustToBeSafePartOneUrl(identifiedModel.PathwayId), Method.GET));

            CheckResponse(questionsWithAnswers);

            if (!questionsWithAnswers.Data.Any())
            {
                var questionViewModel = new QuestionViewModel
                {
                    PathwayId = identifiedModel.PathwayId,
                    PathwayNo = identifiedModel.PathwayNo,
                    PathwayTitle = identifiedModel.PathwayTitle,
                    PathwayTraumaType = identifiedModel.PathwayTraumaType,
                    DigitalTitle = string.IsNullOrEmpty(identifiedModel.DigitalTitle) ? identifiedModel.PathwayTitle : identifiedModel.DigitalTitle,
                    UserInfo = identifiedModel.UserInfo,
                    JourneyJson = identifiedModel.JourneyJson,
                    State = JsonConvert.DeserializeObject<Dictionary<string, string>>(identifiedModel.StateJson),
                    StateJson = identifiedModel.StateJson,
                    CollectedKeywords = identifiedModel.CollectedKeywords,
                    Journey = JsonConvert.DeserializeObject<Journey>(identifiedModel.JourneyJson),
                    SessionId = model.SessionId,
                    JourneyId = identifiedModel.JourneyId,
                    FilterServices = model.FilterServices,
                    Campaign = model.Campaign,
                    Source = model.Source,
                    CurrentPostcode = model.CurrentPostcode,
                    EntrySearchTerm = model.EntrySearchTerm
                };

                var question = await _restClient.ExecuteTaskAsync<QuestionWithAnswers>(new JsonRestRequest(_configuration.GetBusinessApiFirstQuestionUrl(identifiedModel.PathwayId, identifiedModel.StateJson), Method.GET));

                CheckResponse(question);

                _mappingEngine.Mapper.Map(question.Data, questionViewModel);

                //retrieve encounter id generated fromfirst  evaluate interaction
                //and set journey id to it
                questionViewModel.JourneyId = question.Data.State.ContainsKey("ENCOUNTER_ID")
                    ? Guid.Parse(question.Data.State["ENCOUNTER_ID"])
                    : Guid.NewGuid();

                _userZoomDataBuilder.SetFieldsForQuestion(questionViewModel);
                if (questionViewModel.NodeType == NodeType.Page)
                    return new Tuple<string, QuestionViewModel>("../Question/Page", questionViewModel);

                return new Tuple<string, QuestionViewModel>("../Question/Question", questionViewModel);
            }
            identifiedModel.Part = 1;
            identifiedModel.Questions = questionsWithAnswers.Data.ToList();
            identifiedModel.QuestionsJson = JsonConvert.SerializeObject(questionsWithAnswers.Data);
            identifiedModel.JourneyJson = string.IsNullOrEmpty(identifiedModel.JourneyJson) ? JsonConvert.SerializeObject(new Journey()) : identifiedModel.JourneyJson;
            identifiedModel.FilterServices = model.FilterServices;
            return new Tuple<string, QuestionViewModel>("../JustToBeSafe/JustToBeSafe", identifiedModel);

        }

        private async Task<JustToBeSafeViewModel> DoWorkPreviouslyDoneInQuestionBuilder(JustToBeSafeViewModel model) {
            var businessApiPathwayUrl = _configuration.GetBusinessApiPathwayUrl(model.PathwayId);
            var response = await _restClient.ExecuteTaskAsync<Pathway>(new JsonRestRequest(businessApiPathwayUrl, Method.GET));

            CheckResponse(response);

            if (response.Data == null) return null;

            var pathway = response.Data;
            var derivedAge = model.UserInfo.Demography.Age == -1 ? pathway.MinimumAgeInclusive : model.UserInfo.Demography.Age;
            var newModel = new JustToBeSafeViewModel
            {
                PathwayId = pathway.Id,
                PathwayNo = pathway.PathwayNo,
                PathwayTitle = pathway.Title,
                PathwayTraumaType = pathway.TraumaType,
                DigitalTitle = string.IsNullOrEmpty(model.DigitalTitle) ? pathway.Title : model.DigitalTitle,
                UserInfo = new UserInfo { Demography = new AgeGenderViewModel { Age = derivedAge, Gender = pathway.Gender } },
                JourneyId = model.JourneyId,
                JourneyJson = model.JourneyJson,
                SymptomDiscriminatorCode = model.SymptomDiscriminatorCode,
                State = JourneyViewModelStateBuilder.BuildState(model.JourneyId.ToString(), pathway.Gender, derivedAge),
                SessionId = model.SessionId,
                Campaign = model.Campaign,
                Source = model.Source,
                FilterServices = model.FilterServices
            };

            newModel.StateJson = JourneyViewModelStateBuilder.BuildStateJson(newModel.State);

            return newModel;
        }

        private async Task<JustToBeSafeViewModel> BuildIdentifiedModel(JustToBeSafeViewModel model)
        {
            var response = await _restClient.ExecuteTaskAsync<Pathway>(new JsonRestRequest(_configuration.GetBusinessApiPathwayIdUrl(model.PathwayNo, model.UserInfo.Demography.Gender, model.UserInfo.Demography.Age), Method.GET));

            CheckResponse(response);

            if (response.Data == null) return null;

            var pathway = response.Data;
            model.PathwayId = pathway.Id;
            model.PathwayTitle = pathway.Title;
            model.PathwayNo = pathway.PathwayNo;
            model.PathwayTraumaType = pathway.TraumaType;
            model.JourneyId = model.JourneyId;
            model.State = JourneyViewModelStateBuilder.BuildState(model.JourneyId.ToString(), model.UserInfo.Demography.Gender,model.UserInfo.Demography.Age, model.State);
            model.StateJson = JourneyViewModelStateBuilder.BuildStateJson(model.State);
            model.CollectedKeywords = new KeywordBag(_keywordCollector.ParseKeywords(pathway.Keywords, false).ToList(), _keywordCollector.ParseKeywords(pathway.ExcludeKeywords, false).ToList());
            return model;
        }
    }

    public interface IJustToBeSafeFirstViewModelBuilder
    {
        Task<Tuple<string, QuestionViewModel>> JustToBeSafeFirstBuilder(JustToBeSafeViewModel model);
    }
}
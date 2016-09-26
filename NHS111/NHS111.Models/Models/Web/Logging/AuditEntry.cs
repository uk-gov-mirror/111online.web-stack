﻿using System;
using Newtonsoft.Json;

namespace NHS111.Models.Models.Web.Logging
{
    public class AuditEntry : LogEntry
    {
        private string _pathwayId = string.Empty;
        private string _pathwayTitle = string.Empty;
        private string _state = string.Empty;
        private string _journey = string.Empty;
        private string _answerTitle = string.Empty;
        private string _answerOrder = string.Empty;
        private string _questionTitle = string.Empty;
        private string _questionNo = string.Empty;
        private string _questionId = string.Empty;
        private string _dxCode = string.Empty;
        private string _eventData = string.Empty;


        [JsonProperty(PropertyName = "sessionId")]
        public Guid SessionId { get; set; }

        [JsonProperty(PropertyName = "pathwayId")]
        public string PathwayId { get { return _pathwayId; } set { _pathwayId = value; } }

        [JsonProperty(PropertyName = "pathwayTitle")]
        public string PathwayTitle { get { return _pathwayTitle; } set { _pathwayTitle = value; } }

        [JsonProperty(PropertyName = "state")]
        public string State { get { return _state; } set { _state = value; } }

        [JsonProperty(PropertyName = "journey")]
        public string Journey { get { return _journey; } set { _journey = value; } }

        [JsonProperty(PropertyName = "answerTitle")]
        public string AnswerTitle { get { return _answerTitle; } set { _answerTitle = value; } }

        [JsonProperty(PropertyName = "answerOrder")]
        public string AnswerOrder { get { return _answerOrder; } set { _answerOrder = value; } }

        [JsonProperty(PropertyName = "questionTitle")]
        public string QuestionTitle { get { return _questionTitle; } set { _questionTitle = value; } }

        [JsonProperty(PropertyName = "questionNo")]
        public string QuestionNo { get { return _questionNo; } set { _questionNo = value; } }

        [JsonProperty(PropertyName = "questionId")]
        public string QuestionId { get { return _questionId; } set { _questionId = value; } }

        [JsonProperty(PropertyName = "dxCode")]
        public string DxCode { get { return _dxCode; } set { _dxCode = value; } }

        [JsonProperty(PropertyName = "eventData")]
        public string EventData { get { return _eventData; } set { _eventData = value; } }
    }
}
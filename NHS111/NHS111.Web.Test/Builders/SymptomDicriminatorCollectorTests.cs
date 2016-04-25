﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web;
using NHS111.Web.Presentation.Builders;
using NUnit.Framework;
namespace NHS111.Web.Presentation.Builders.Tests
{
    [TestFixture()]
    public class SymptomDicriminatorCollectorTests
    {
        SymptomDicriminatorCollector _symptomDicriminatorCollector = new SymptomDicriminatorCollector();
        const string TEST_SD_CODE = "1234";

        [Test()]
        public void Collect_Answer_with_no_symptonDiscriminator_returns_empty_symptomDiscriminator_Test()
        {
            var testViewModel = new JourneyViewModel();
            var answer = new Answer(){Order = 0, Title = "Yes"};
            var viewModelWithCollectedSymptomDicriminator = _symptomDicriminatorCollector.Collect(answer, testViewModel);
            Assert.AreEqual(String.Empty, viewModelWithCollectedSymptomDicriminator.SymptomDiscriminator);
        }

        [Test()]
        public void Collect_Answer_with_symptonDiscriminator_returns_symptomDiscriminator_Test()
        {
            var testViewModel = new JourneyViewModel();
            var answer = new Answer() { Order = 0, Title = "Yes", SymptomDiscriminator = TEST_SD_CODE };
            var viewModelWithCollectedSymptomDicriminator = _symptomDicriminatorCollector.Collect(answer, testViewModel);
            Assert.AreEqual(TEST_SD_CODE, viewModelWithCollectedSymptomDicriminator.SymptomDiscriminator);
        }

        [Test()]
        public void Collect_QuestionWithAnswers_with_symptonDiscriminator_returns_symptomDiscriminator_Test()
        {
            var testViewModel = new JourneyViewModel();
            var questionWithAnswers = new QuestionWithAnswers()
            {
                Answered = new Answer() { Order = 0, Title = "Yes", SymptomDiscriminator = TEST_SD_CODE },
                Answers = new List<Answer>() { new Answer() { Order = 0, Title = "Yes" }, new Answer() { Order = 1, Title = "No" }},
                Question = new Question() { Order = "0", Title = "Test Question"}
            };
            var viewModelWithCollectedSymptomDicriminator = _symptomDicriminatorCollector.Collect(questionWithAnswers, testViewModel);
            Assert.AreEqual(TEST_SD_CODE, viewModelWithCollectedSymptomDicriminator.SymptomDiscriminator);
        }
    }
}

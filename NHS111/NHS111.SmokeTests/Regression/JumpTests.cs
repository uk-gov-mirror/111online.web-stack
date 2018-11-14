﻿using NHS111.SmokeTest.Utils;
using NUnit.Framework;

namespace NHS111.SmokeTests.Regression
{
    public class JumpTests : BaseTests
    {
        [Test]
        public void Pt8JumpToDx35()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Tiredness (Fatigue)", TestScenerioSex.Male, TestScenerioAgeGroups.Adult);

            questionPage.VerifyQuestion("Have you got a raised temperature now or have you had one at any time since the tiredness started?");
            var outcomePage = questionPage
                .AnswerSuccessiveByOrder(3, 4)
                .AnswerSuccessiveByOrder(4, 2)
                .Answer(3)
                .Answer(4)
                .Answer(3)
                .AnswerSuccessiveByOrder(5, 2)
                .AnswerSuccessiveByOrder(3, 4)
                .AnswerForDispostion<OutcomePage>("Alcohol");

            outcomePage.VerifyOutcome(OutcomePage.BookCallBackText);
            outcomePage.VerifyDispositionCode("Dx35");
        }

        [Test]
        //PT8 via Behaviour Change Tx 140148 Tx221449 and Tx222008
        public void Pt8ViaBehaviourChangeTx140148Tx221449Tx222008()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Behaviour Change", TestScenerioSex.Male, TestScenerioAgeGroups.Adult);

            var outcomePage = questionPage
                .AnswerSuccessiveNo(3)
                .Answer(3)
                .AnswerSuccessiveNo(12)
                .AnswerForDispostion<OutcomePage>("Yes");

            outcomePage.VerifyOutcome(OutcomePage.BookCallBackText);
            outcomePage.VerifyDispositionCode("Dx35");
        }

        [Test]
        //PT8 via Behaviour Change Tx140148 and Tx222023 No Dx String
        public void Pt8ViaBehaviourChangeTx140148Tx222023NoDx()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Behaviour Change", TestScenerioSex.Male, TestScenerioAgeGroups.Adult);

            var outcomePage = questionPage
                .AnswerSuccessiveNo(3)
                .Answer(2)
                .Answer(3)
                .AnswerSuccessiveNo(16)
                .Answer(1)
                .AnswerForDispostion<OutcomePage>("No");

            outcomePage.VerifyOutcome(OutcomePage.BookCallBackText);
            outcomePage.VerifyDispositionCode("Dx35");
        }


        [Test]
        //PT8 via Headache Tx222027 and Tx222006 FA
        public void Pt8ViaHeadacheTx222027Tx222006Fa()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Headache", TestScenerioSex.Female, TestScenerioAgeGroups.Adult);

            var outcomePage = questionPage
                .AnswerYes()
                .AnswerSuccessiveByOrder(3, 4)
                .AnswerSuccessiveNo(3)
                .Answer(3)
                .AnswerSuccessiveNo(4)
                .Answer(3)
                .Answer(5)
                .Answer(4)
                .AnswerNo()
                .Answer(3)
                .Answer(7)
                .AnswerNo()
                .Answer(2)
                .AnswerForDispostion<OutcomePage>("No");

            outcomePage.VerifyOutcome(OutcomePage.BookCallBackText);
            outcomePage.VerifyDispositionCode("Dx35");
        }

        [Test]
        //PT8 via Headache Tx222027 and Tx222006 MC
        public void Pt8ViaHeadacheTx222027Tx222006Mc()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Headache", TestScenerioSex.Male, TestScenerioAgeGroups.Child);

            var outcomePage = questionPage
                .AnswerSuccessiveByOrder(3, 3)
                .AnswerSuccessiveNo(2)
                .Answer(3)
                .AnswerSuccessiveNo(4)
                .Answer(3)
                .Answer(5)
                .AnswerSuccessiveNo(2)
                .Answer(3)
                .Answer(7)
                .AnswerNo()
                .Answer(1)
                .AnswerForDispostion<OutcomePage>("No");

            outcomePage.VerifyOutcome(OutcomePage.BookCallBackText);
            outcomePage.VerifyDispositionCode("Dx35");
        }

        [Test]
        //PT8 via Tremor via Age variable and 2 strings Dx06 and Dx35
        public void Pt8ViaTremorViaAgeVariable2Strings()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Tremor", TestScenerioSex.Male, 13);

            var outcomePage = questionPage
                .AnswerNo()
                .Answer(3)
                .AnswerSuccessiveNo(3)
                .AnswerForDispostion<OutcomePage>("Yes");

            outcomePage.VerifyDispositionCode("Dx06");

            var genderPage = outcomePage.NavigateBackToGenderPage();

            genderPage.VerifyHeader();
            genderPage.SelectSexAndAge(TestScenerioSex.Male, 14);

            var searchpage = genderPage.NextPage();
            var questionInfoPage = searchpage.TypeSearchTextAndSelect("Tremor");
            var newQuestionPage = questionInfoPage.ClickIUnderstand();

            var newOutcomePage = newQuestionPage
                .AnswerNo()
                .Answer(3)
                .AnswerSuccessiveNo(3)
                .AnswerForDispostion<OutcomePage>("Yes");

            newOutcomePage.VerifyDispositionCode("Dx35");
        }

        [Test]
        //PT8 New jump to Dx11 in V15
        public void Pt8ViaTremorViaAgeVariabletoDx11()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(Driver, "Behaviour Change", TestScenerioSex.Male, 24);

            var outcomePage = questionPage
                .AnswerSuccessiveNo(3)
                .Answer(1)
                .Answer(3)
                .AnswerSuccessiveNo(8)
                .AnswerForDispostion<OutcomePage>("Yes");

            outcomePage.VerifyDispositionCode("Dx11");
        }
    }
}

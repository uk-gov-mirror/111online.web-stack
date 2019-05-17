﻿using NHS111.Web.Functional.Utils;
using NHS111.Web.Functional.Utils.ScreenShot;
using NUnit.Framework;

namespace NHS111.Web.Functional.Tests
{
    [TestFixture]
    [ScreenShotComparison]
    public class RecommendedServiceTests : BaseTests
    {
        [Test]
        public void ReferRingAndGoService()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "L12SA");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyServiceDetails();
            recommendedServicePage.VerifyOpeningTimes();
            recommendedServicePage.VerifyDistance();
            recommendedServicePage.VerifyActionLink("Refer me to this service");
            recommendedServicePage.CompareAndVerify("1");
        }

        [Test]
        public void ReferOnlyService()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "LS177NZ");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyServiceDetails();
            recommendedServicePage.VerifyActionLink("Use this service");
            recommendedServicePage.VerifyOpeningTimes();
        }

        [Test]
        public void PlaceToVisitService()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "SO408UU");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyServiceDetails();
            recommendedServicePage.VerifyOpeningTimes();
            recommendedServicePage.VerifyDistance();
            recommendedServicePage.VerifyNoActionLink();
            recommendedServicePage.CompareAndVerify("1");
        }

        [Test]
        public void PlaceToPhoneService()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "SO408UU");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyServiceDetails();
            recommendedServicePage.VerifyNoActionLink();
            recommendedServicePage.CompareAndVerify("1");
        }

        [Test]
        public void OtherServicesMoreThanOneService()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "LS177NZ");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyOtherServices();
            recommendedServicePage.CompareAndVerify("1");
        }

        [Test]
        public void OtherServicesNotShown()
        {
            var questionPage = TestScenerios.LaunchRecommendedServiceScenerio(Driver, "Emergency Prescription 111 Online", TestScenerioSex.Male, TestScenerioAgeGroups.Adult, "LS177NZ");

            questionPage.VerifyQuestion("Can you get in touch with your GP or usual pharmacy?");
            var recommendedServicePage = questionPage
                .Answer(2)
                .Answer<OutcomePage>(1)
                .ClickShowServices();

            recommendedServicePage.VerifyNoOtherServices();
            recommendedServicePage.CompareAndVerify("1");
        }
    }
}

﻿using System;
using System.Configuration;
using NHS111.Web.Functional.Utils.ScreenShot;
using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace NHS111.Web.Functional.Utils
{
    public class BaseTests
    {
        public IWebDriver Driver;

        [TestFixtureSetUp]
        public void InitTestFixture()
        {
            // Ideally we could have multiple size screenshots
            // for Visual Regression Test MVP this uses the same width as Andria's Selenium screenshots (1232px)
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--window-size=1232,1000"); // Ensure all screenshots are same size across build agents
            chromeOptions.AddArguments("--disable-gpu"); // Workaround for renderer timeout https://stackoverflow.com/questions/48450594/selenium-timed-out-receiving-message-from-renderer
            Driver = new ChromeDriver(chromeOptions);
        }

        [TestFixtureTearDown]
        public void TearDownTestFixture()
        {
            Driver.Quit();
        }

        [TearDown]
        public void TearDownTest()
        {
            if (TestContext.CurrentContext.Result.Status != TestStatus.Failed) return;

            //output the failed screenshot to results screen in Team City
            if(!ScreenShotMaker.CheckScreenShotExists(Driver.GetCurrentImageUniqueId()))
                ScreenShotMaker.MakeScreenShot(Driver.GetCurrentImageUniqueId());
            Console.WriteLine("##teamcity[testMetadata testName='{0}' name='Test screen' type='image' value='{1}']", TestContext.CurrentContext.Test.FullName, ScreenShotMaker.GetScreenShotFilename(Driver.GetCurrentImageUniqueId()));
        }

        public IScreenShotMaker ScreenShotMaker
        {
            get { return new ScreenShotMaker(Driver); }
        }

        public PostcodeProvider Postcodes = new PostcodeProvider();
        protected static readonly string BaseUrl = ConfigurationManager.AppSettings["TestWebsiteUrl"];
    }
}

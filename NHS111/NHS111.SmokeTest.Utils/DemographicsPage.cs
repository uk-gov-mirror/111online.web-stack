﻿
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace NHS111.SmokeTest.Utils
{
    public class DemographicsPage : LayoutPage
    {
        private string _headerText = "Tell us about you, or the person you're asking about";
        private string _ageValidationMessageTooOld = "Please enter a value less than or equal to 200.";
        private string _ageValidationMessageTooYoung = "Sorry, this service is not available for children under 5 years of age, for medical advice please call 111.";
        private string _sexValidationMessage = "Please select a sex";
        private string _ageNotEnteredValidationMessage = "'Age' must not be empty.";

        [FindsBy(How = How.CssSelector, Using = "h1")]
        private IWebElement Header { get; set; }

        [FindsBy(How = How.CssSelector, Using = "label[for='Male']")]
        private IWebElement MaleButton { get; set; }

        [FindsBy(How = How.CssSelector, Using = "label[for='Female']")]
        private IWebElement FemaleButton { get; set; }

        [FindsBy(How = How.Id, Using = "UserInfo_Demography_Age")]
        private IWebElement AgeInput { get; set; }

        [FindsBy(How = How.CssSelector, Using = "span[for='UserInfo_Demography_Age']")]
        private IWebElement AgeValidationMessageElement { get; set; }

        [FindsBy(How = How.CssSelector, Using = "span[data-valmsg-for='UserInfo.Demography.Gender']")]
        private IWebElement SexValidationMessageElement { get; set; }

        [FindsBy(How = How.ClassName, Using = "button--next")]
        private IWebElement NextButton { get; set; }

        public DemographicsPage(IWebDriver driver) : base(driver)
        {
        }

        public void SelectSexAndAge(string sex, int age)
        {
            SelectSex(sex);
            SetAge(age);
        }

        public void SelectSex(string sex)
        {
            if (sex == TestScenerioSex.Male)
                MaleButton.Click();
            else
                FemaleButton.Click();
        }

        public void SetAge(int age)
        {
            AgeInput.SendKeys(age.ToString());
        }

        public void VerifyHeader()
        {
            Assert.IsTrue(Header.Displayed);
            Assert.AreEqual(_headerText, Header.Text);
        }

        public SearchPage NextPage()
        {
            NextButton.Submit();
            return new SearchPage(Driver);
        }

        public void VerifyAgeInputBox(string sex, string age)
        {
            SelectSex(sex);
            AgeInput.SendKeys(age);
            NextButton.Submit();

            var searchPage = new SearchPage(Driver);
            searchPage.Verify();
        }

        public void VerifyNoSexValidation(int age)
        {
            SetAge(age);
            NextButton.Submit();

            Assert.IsTrue(SexValidationMessageElement.Displayed);
            Assert.AreEqual(_sexValidationMessage, SexValidationMessageElement.Text);
        }

        public void VerifyNoAgeValidation(string sex)
        {
            SelectSex(sex);
            NextButton.Submit();

            Assert.IsTrue(AgeValidationMessageElement.Displayed);
            Assert.AreEqual(_ageNotEnteredValidationMessage, AgeValidationMessageElement.Text);
        }

        public void VerifyTooOldAgeShowsValidation(string sex, int age)
        {
            SelectSex(sex);
            SetAge(age);
            NextButton.Submit();

            Assert.IsTrue(AgeValidationMessageElement.Displayed);
            Assert.AreEqual(_ageValidationMessageTooOld, AgeValidationMessageElement.Text);
        }

        public void VerifyTooYoungAgeShowsValidation(string sex, int age)
        {
            SelectSex(sex);
            SetAge(age);
            NextButton.Submit();

            Assert.IsTrue(AgeValidationMessageElement.Displayed);
            Assert.AreEqual(_ageValidationMessageTooYoung, AgeValidationMessageElement.Text);
        }

        public void VerifyTabbingOrder(int age)
        {
            HeaderLogo.SendKeys(Keys.Tab);
            var maleButton = Driver.SwitchTo().ActiveElement();
            maleButton.SendKeys(Keys.Space);
            maleButton.SendKeys(Keys.Tab);
            var ageInput = Driver.SwitchTo().ActiveElement();
            ageInput.SendKeys(age.ToString());
            ageInput.SendKeys(Keys.Tab);
            var nextButton = Driver.SwitchTo().ActiveElement();
            nextButton.Submit();

            var searchPage = new SearchPage(Driver);
            searchPage.Verify();
        }
    }
}

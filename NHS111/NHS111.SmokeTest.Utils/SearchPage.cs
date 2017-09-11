﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace NHS111.SmokeTest.Utils
{
    public class SearchPage
    {
        private readonly IWebDriver _driver;
        private const string _headerText = "Tell us the symptom you’re concerned about";

        [FindsBy(How = How.Id, Using = "SanitisedSearchTerm")]
        public IWebElement SearchTxtBox { get; set; }

        [FindsBy(How = How.ClassName, Using = "button--next")]
        public IWebElement GoButton { get; set; }

        [FindsBy(How = How.CssSelector, Using = ".content-container h1 label")]
        public IWebElement Header { get; set; }

        public SearchPage(IWebDriver driver)
        {
            _driver = driver;
            PageFactory.InitElements(_driver, this);
        }

        public void TypeSearchTextAndClickGo()
        {
            TypeSearchTextAndSelect("Headache");
            ClickGoButton();
        }

        public void Verify()
        {
            Assert.IsTrue(Header.Displayed);
            Assert.AreEqual(_headerText, Header.Text);
        }

        public IWebElement FindPathwayInCathgryList(string title, string pathwayId)
        {
           return _driver.FindElement(By.XPath(String.Format("//a[@data-title= \"{0}\"][@data-pathway-number= '{1}']", title, pathwayId)));
        }

        public void SelectCategory(string categoryTitle)
        {
              new WebDriverWait(_driver, new TimeSpan(0, 0, 5))
                  .Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy((By.Id(categoryTitle))));
             _driver.FindElement(By.Id(categoryTitle)).Click();
        }

        public void SelectPathway(string pathwayTitle)
        {
            new WebDriverWait(_driver, new TimeSpan(0, 0, 5))
                .Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy((By.XPath(String.Format("//a[@data-title= '{0}']", pathwayTitle)))));
            _driver.FindElement(By.XPath(String.Format("//a[@data-title= '{0}']", pathwayTitle))).Click();
        }
        public QuestionPage TypeSearchTextAndSelect(string pathway)
        {
            SearchTxtBox.Clear();
            SearchTxtBox.SendKeys(pathway);
            this.ClickGoButton();
            new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='results-list']/ul/li")));
            _driver.FindElement(By.XPath("//div[@class='results-list']/ul/li/a[@data-title='" + pathway + "']")).Click();
            return new QuestionPage(_driver);
        }

        public IEnumerable<IWebElement> GetHits()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='results-list']/ul/li")));
            return _driver.FindElements(By.XPath("//div[@class='results-list']/ul/li")); 
        }

        public void SearchByTerm(string term)
        {
            this.SearchTxtBox.Clear();
            this.SearchTxtBox.SendKeys(term);
            this.ClickGoButton();
        }
        public void VerifyTermHits(string expectedHitTitle, int maxRank)
        {
            var rank = 0;
            var linkText = "";
            foreach (var hit in this.GetHits().ToList())
            {
                rank++;
                var linkElements = hit.FindElements(By.TagName("a"));
                if (linkElements.Count > 0)
                {
                    linkText = linkElements.FirstOrDefault().Text.StripHTML().ToLower();
                    if(linkText == expectedHitTitle.ToLower()) break;
                }
            }

            Assert.AreEqual(expectedHitTitle.ToLower(), linkText);
            Assert.IsTrue(rank <= maxRank);
        }


        public QuestionPage ClickGoButton()
        {
            GoButton.Click();
            return new QuestionPage(_driver);
        }

        

    }

    public static class StringExtensionMethods
    {
        public static string StripHTML(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}

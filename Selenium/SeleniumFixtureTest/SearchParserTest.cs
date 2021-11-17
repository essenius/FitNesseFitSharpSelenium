// Copyright 2015-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SearchParserTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchParserByFunctionNullTest()
        {
            _ = new SearchParser("unknown", "abc").By;
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchParserCustomByExceptionTest()
        {
            _ = new SearchParser("trial", "").By;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SearchParserByFunctionTest()
        {
            const string locator = "abc";
            var searchParser = new SearchParser("AccessibilityID", locator);
            Assert.AreEqual(searchParser.By, MobileBy.AccessibilityId(locator));
            searchParser = new SearchParser("classname", locator);
            Assert.AreEqual(searchParser.By, By.ClassName(locator));
            searchParser = new SearchParser("CssSelector", locator);
            Assert.AreEqual(searchParser.By, By.CssSelector(locator));
            searchParser = new SearchParser("id", locator);
            Assert.AreEqual(searchParser.By, By.Id(locator));
            searchParser = new SearchParser("IOSCLASSCHAIN", locator);
            Assert.AreEqual(searchParser.By, MobileBy.IosClassChain(locator));
            searchParser = new SearchParser("iosNSpredicate", locator);
            Assert.AreEqual(searchParser.By, MobileBy.IosNSPredicate(locator));
            searchParser = new SearchParser("IOSUIAutomation", locator);
            Assert.AreEqual(searchParser.By, MobileBy.IosUIAutomation(locator));
            searchParser = new SearchParser("LINKTEXT", locator);
            Assert.AreEqual(searchParser.By, By.LinkText(locator));
            searchParser = new SearchParser("NaMe", locator);
            Assert.AreEqual(searchParser.By, By.Name(locator));
            searchParser = new SearchParser("PartialLINKTEXT", locator);
            Assert.AreEqual(searchParser.By, By.PartialLinkText(locator));
            searchParser = new SearchParser("Tagname", locator);
            Assert.AreEqual(searchParser.By, By.TagName(locator));
            searchParser = new SearchParser("TizenAutomation", locator);
            Assert.AreEqual(searchParser.By, MobileBy.TizenAutomation(locator));
            searchParser = new SearchParser("WindowsAutomation", locator);
            Assert.AreEqual(searchParser.By, MobileBy.WindowsAutomation(locator));
            searchParser = new SearchParser("XPath", locator);
            Assert.AreEqual(searchParser.By, By.XPath(locator));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchParserFindElement1NullTest() => _ = new SearchParser(null);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("  abc  ", "trial", "abc")]
        [DataRow("abc:def", "abc", "def")]
        [DataRow("abc : def", "abc", "def")]
        [DataRow("abc:def:ghi", "abc", "def:ghi")]
        [DataRow("abc : def:ghi", "abc", "def:ghi")]
        [DataRow("abc : def : ghi", "abc", "def : ghi")]
        [DataRow(":abc", "", "abc")]
        [DataRow("abc:", "abc", "")]
        [DataRow(":", "", "")]
        [DataRow("", "trial", "")]
        public void SearchParserFindElement1Test(string input, string expectedMethod, string expectedLocator)
        {
            var searchParser = new SearchParser(input);
            Assert.AreEqual(expectedMethod, searchParser.Method, "Method OK");
            Assert.AreEqual(expectedLocator, searchParser.Locator, "Locator OK");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchParserFindElement2NullTest() => _ = new SearchParser(null, null);

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("filled", "abc", "def", "abc", "def")]
        [DataRow("null", null, "def", "trial", "def")]
        [DataRow("empty", "", "def", "trial", "def")]
        public void SearchParserFindElement2Test(string testId, string methodIn, string locatorIn, string expectedMethod, string expectedLocator)
        {
            var searchParser = new SearchParser(methodIn, locatorIn);
            Assert.AreEqual(expectedMethod, searchParser.Method, $"{testId} Method OK");
            Assert.AreEqual(expectedLocator, searchParser.Locator, $"{testId} Locator OK");
        }
    }
}

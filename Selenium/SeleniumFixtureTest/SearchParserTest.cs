// Copyright 2015-2019 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SearchParserTest
    {

        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Forcing an exception")]
        public void SearchParserByFunctionNullTest()
        {
            var by = new SearchParser("unknown", "abc").By;
        }

        [TestMethod, TestCategory("Unit")]
        public void SearchParserByFunctionTest()
        {
            const string locator = "abc";
            var searchParser = new SearchParser("classname", locator);
            Assert.AreEqual(searchParser.By, By.ClassName(locator));
            searchParser = new SearchParser("CssSelector", locator);
            Assert.AreEqual(searchParser.By, By.CssSelector(locator));
            searchParser = new SearchParser("id", locator);
            Assert.AreEqual(searchParser.By, By.Id(locator));
            searchParser = new SearchParser("LINKTEXT", locator);
            Assert.AreEqual(searchParser.By, By.LinkText(locator));
            searchParser = new SearchParser("NaMe", locator);
            Assert.AreEqual(searchParser.By, By.Name(locator));
            searchParser = new SearchParser("PartialLINKTEXT", locator);
            Assert.AreEqual(searchParser.By, By.PartialLinkText(locator));
            searchParser = new SearchParser("Tagname", locator);
            Assert.AreEqual(searchParser.By, By.TagName(locator));
            searchParser = new SearchParser("XPath", locator);
            Assert.AreEqual(searchParser.By, By.XPath(locator));
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException))]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement", Justification = "forcing an exception")]
        public void SearchParserFindElement1NullTest()
        {
            new SearchParser(null);
        }

        [TestMethod, TestCategory("Unit"), DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "SearchParser.FindElement1", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml")]
        public void SearchParserFindElement1Test()
        {
            var input = TestContext.DataRow["input"].ToString();
            var expectedMethod = TestContext.DataRow["expectedMethod"].ToString();
            var expectedLocator = TestContext.DataRow["expectedLocator"].ToString();
            var searchParser = new SearchParser(input);
            Assert.AreEqual(expectedMethod, searchParser.Method, "Method OK");
            Assert.AreEqual(expectedLocator, searchParser.Locator, "Locator OK");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException))]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement", Justification = "forcing an exception")]
        public void SearchParserFindElement2NullTest()
        {
            new SearchParser(null, null);
        }

        [TestMethod, TestCategory("Unit")]
        public void SearchParserFindElement2Test()
        {
            var searchParser = new SearchParser("abc", "def");
            Assert.AreEqual("abc", searchParser.Method, "Method OK");
            Assert.AreEqual("def", searchParser.Locator, "Locator OK");
            searchParser = new SearchParser(null, "def");
            Assert.AreEqual("id", searchParser.Method, "Method OK");
            Assert.AreEqual("def", searchParser.Locator, "Locator OK");
            searchParser = new SearchParser(string.Empty, "def");
            Assert.AreEqual("id", searchParser.Method, "Method OK");
            Assert.AreEqual("def", searchParser.Locator, "Locator OK");
        }
    }
}
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
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SeleniumDeprecatedTest
    {
        private Selenium _selenium;

        [TestInitialize]
        public void SeleniumDeprecatedTestInitialize() => _selenium = new Selenium();

        [TestCleanup]
        public void SeleniumTestCleanup()
        {
            _selenium.Close();
            Selenium.SetProxyType("System");
        }

#pragma warning disable 612,618
// we don't want the warnings about obsolete functions here. They also need to be tested

        [TestMethod]
        [TestCategory("Deprecated")]
        public void SeleniumDeprecatedTests()
        {
            Selenium.ExceptionOnDeprecatedFunctions = false;
            _selenium.SetBrowser("Chrome Headless");
            Assert.IsTrue(_selenium.WaitForNoElement("NonExistingElement"));
            var htmlSource = _selenium.HtmlSource();
            Assert.AreEqual(htmlSource.Length, _selenium.LengthOfHtmlSource());
            Assert.IsTrue(_selenium.WaitUntilHtmlSourceIsLargerThan(10));
            _selenium.SetTimeoutSeconds(0.5);
            Assert.IsFalse(_selenium.WaitForHtmlSourceToChange());
            Selenium.ExceptionOnDeprecatedFunctions = true;
        }

        [TestMethod]
        [TestCategory("Deprecated")]
        [ExpectedException(typeof(NotSupportedException))]
        public void SeleniumDeprecatedNewRemoteBrowserAtAddressWithNameTest() =>
            _ = Selenium.NewRemoteBrowserAtAddressWithName("irrelevant", "irrelevant", "irrelevant");

        [TestMethod]
        [TestCategory("Deprecated")]
        [ExpectedException(typeof(NotSupportedException))]
        public void SeleniumDeprecatedSetRemoteBrowserAtAddressWithNameTest() =>
            _ = Selenium.SetRemoteBrowserAtAddressWithName("irrelevant", "irrelevant", "irrelevant");

        [TestMethod]
        [TestCategory("Deprecated")]
        [ExpectedExceptionWithMessage(typeof(WarningException), "Use of deprecated function 'HTML Source'. Replace by 'Page Source'")]
        public void SeleniumDeprecatedExceptionTest()
        {
            Selenium.ExceptionOnDeprecatedFunctions = true;
            var _ = _selenium.HtmlSource();
        }

#pragma warning restore 612,618
    }
}

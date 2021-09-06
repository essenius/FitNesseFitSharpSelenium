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
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumFixtureTest
{
    [TestClass]
    public sealed class ChromeFrameSwitchingBug : IDisposable
    {
        private IWebDriver _browserDriver;

        public void Dispose() => _browserDriver?.Dispose();

        [TestMethod]
        [TestCategory("Experiments")]
        public void FrameSwitchTestWithSeleniumApiDocumentation()
        {
            const string deprecatedText = "DEPRECATED";
            const string contentFrameName = "classFrame";
            _browserDriver = new ChromeDriver();
            _browserDriver.Navigate().GoToUrl(new Uri("https://www.selenium.dev/selenium/docs/api/java/index.html?overview-summary.html"));

            Thread.Sleep(2000);
            Debug.Print("___________________________________________________");
            Debug.Print(_browserDriver.PageSource);
            Debug.Print("___________________________________________________");
            var element = _browserDriver.FindElement(By.Name(contentFrameName));
            try
            {
                _browserDriver.FindElement(By.LinkText(deprecatedText));
                Assert.Fail("Found Deprecated before switching frames");
            }
            catch (NoSuchElementException)
            {
            }
            Debug.Print("found classFrame");
            _browserDriver.SwitchTo().Frame(element);
            Debug.Print("switched to classFrame");
            Debug.Print("___________________________________________________");
            Debug.Print(_browserDriver.PageSource);
            Debug.Print("___________________________________________________");
            //Thread.Sleep(TimeSpan.FromSeconds(2));
            Assert.IsNotNull(_browserDriver.FindElement(By.LinkText(deprecatedText)));
        }

        [TestCleanup]
        public void MyTestCleanup() => _browserDriver.Quit();
    }
}

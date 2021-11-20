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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class BrowserDriverCreatorTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void BrowserDriverCreatorGetDefaultServiceFirefoxTest()
        {
            using var service1 = BrowserDriverCreator.GetDefaultService<FirefoxDriverService>();
            Assert.IsInstanceOfType(service1, typeof(FirefoxDriverService));
            Assert.IsFalse(service1.IsRunning);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BrowserDriverCreatorGetDefaultServiceTest()
        {
            using (var service1 = BrowserDriverCreator.GetDefaultService<InternetExplorerDriverService>())
            {
                Assert.IsInstanceOfType(service1, typeof(InternetExplorerDriverService));
                Assert.IsFalse(service1.IsRunning);
            }
            try
            {
                BrowserDriverCreator.GetDefaultService<ChromeDriverService>(@"c:\");
                Assert.Fail("Expected exception didn't happen");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsNotNull(ex.InnerException, "ex.InnerException != null");
                Assert.IsTrue(ex.InnerException.Message.StartsWith(@"The file c:\chromedriver.exe does not exist"));
            }
        }

        private static object GetArgList(ICapabilities cap, string keyName)
        {
            var options = cap.GetCapability(keyName) as Dictionary<string, object>;
            Assert.IsNotNull(options);
            var argList = options.Values.ToList()[0];
            Assert.IsNotNull(argList, "ArgList is empty");
            return argList;
        }

        private static void ValidateChromeCapabilities(ICapabilities cap, string optionToCheck)
        {
            Assert.AreEqual(@"chrome", cap.GetCapability(CapabilityType.BrowserName));
            var argList = GetArgList(cap, @"goog:chromeOptions") as IReadOnlyCollection<object>;
            Assert.IsNotNull(argList, "argList cannot be mapped to IReadOnlyCollection ");
            Assert.IsTrue(argList.Count >= 1, "arg count ok");
            Assert.IsTrue(argList.Contains(optionToCheck), "option present");
        }

        private static void ValidateFirefoxCapabilities(ICapabilities cap)
        {
            Assert.AreEqual("firefox", cap.GetCapability(CapabilityType.BrowserName));
            var argList = GetArgList(cap, "moz:firefoxOptions") as Dictionary<string, object>;
            Assert.IsNotNull(argList, "ArgList cannot be mapped to Dictionary");
            Assert.IsTrue(argList.Count >= 4, "ff arg count ok");
            Assert.IsTrue(argList.ContainsKey(@"plugin.state.npctrl"), "ff silverlight enabled");
            Assert.IsTrue(argList.ContainsKey(@"network.negotiate-auth.trusted-uris"), "ff integrated authentication enabled");
        }
    }
}

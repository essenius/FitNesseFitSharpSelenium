// Copyright 2015-2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using SeleniumFixture.Model;

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
                var x = BrowserDriverCreator.GetDefaultService<ChromeDriverService>(@"c:\");
                x.Start();
                Assert.Fail("Expected exception didn't happen");
            }
            catch (Win32Exception ex)
            {
                Assert.IsTrue(ex.Message.StartsWith(@"An error occurred trying to start process 'c:\chromedriver.exe'"));
            }
        }

        /*
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

        */
    }
}

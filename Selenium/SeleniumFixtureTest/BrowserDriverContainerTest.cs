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

using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest

{
    [TestClass]
    public class BrowserDriverContainerTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void BrowserDriverMissingBrowserCleansUpAndRaisesStopTestException()
        {
            var driverCount = BrowserDriverContainer.DriverCount;
            BrowserDriverContainer.NewDriver("Chrome Headless");
            Assert.AreEqual(driverCount + 1, BrowserDriverContainer.DriverCount, "One more browser open");
            try
            {
                // Safari should not be installed on this machine. Should not be an issue since it's no longer maintained
                BrowserDriverContainer.NewDriver("Safari");
                Assert.Fail("No StopTestException raised");
            }
            catch (StopTestException)
            {
                Assert.AreEqual(0, BrowserDriverContainer.DriverCount, "Browsers should be closed");
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            "Can't run browser 'Opera' on Selenium server '*'")]
        public void BrowserDriverNonInstalledRemoteDriverRaisesStopTestException() =>
            BrowserDriverContainer.NewRemoteDriver("Opera", EndToEndTest.RemoteSelenium, new Dictionary<string, object>());

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            "Could not start browser: Safari")]
        public void BrowserDriverNonPresentDriverRaisesStopTestException() => BrowserDriverContainer.NewDriver("Safari");

        [TestMethod]
        [TestCategory("Unit")]
        public void BrowserDriverRemoveNonExistingDriverTest() => Assert.IsFalse(BrowserDriverContainer.RemoveDriver("bogus"));

        [TestMethod]
        [TestCategory("Integration")]
        public void BrowserDriverSetCurrentTest()
        {
            var browser1 = BrowserDriverContainer.NewDriver("chrome headless");
            var browser2 = BrowserDriverContainer.NewDriver("firefox headless");
            Assert.AreEqual(browser2, BrowserDriverContainer.CurrentId);
            BrowserDriverContainer.SetCurrent(browser1);
            Assert.AreEqual(browser1, BrowserDriverContainer.CurrentId);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("direct", true, 0)]
        [DataRow("manual", true, 1)]
        [DataRow("DIRECT", true, 0)]
        [DataRow("MaNuAl", true, 1)]
        [DataRow("proxyautoconfigure", true, 2)]
        [DataRow("autodetect", true, 4)]
        [DataRow("unspecified", false, 6)]
        [DataRow("SYSTEM", true, 5)]
        public void BrowserDriverSetProxyTypeTest(string input, bool expected, int proxyKind)
        {
            Assert.AreEqual(expected, BrowserDriverContainer.SetProxyType(input));
            var proxyField = typeof(BrowserDriverContainer).GetField("_proxy", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(proxyField);
            var proxy = proxyField.GetValue(null) as Proxy;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(proxyKind, (int)proxy.Kind);
        }

        [TestCleanup]
        public void BrowserDriverTestCleanup() => BrowserDriverContainer.CloseAllDrivers();

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            @"Can't run browser 'WrongBrowser' on Selenium server 'wrongaddress'")]
        public void BrowserDriverWrongAddressRaisesStopTestException() =>
            BrowserDriverContainer.NewRemoteDriver("WrongBrowser", @"wrongaddress", new Dictionary<string, object>());

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            "Unrecognized browser: WrongBrowser")]
        public void BrowserDriverWrongDriverRaisesStopTestException() => BrowserDriverContainer.NewDriver("WrongBrowser");

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            "Can't run browser 'WrongDriver' on Selenium server 'http://localhost'")]
        public void BrowserDriverWrongRemoteDriverRaisesStopTestException() =>
            BrowserDriverContainer.NewRemoteDriver("WrongDriver", "http://localhost", new Dictionary<string, object>());
    }
}

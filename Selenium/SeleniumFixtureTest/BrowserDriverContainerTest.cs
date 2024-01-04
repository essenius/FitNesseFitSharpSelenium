// Copyright 2015-2024 Rik Essenius
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
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest;

[TestClass]
public class BrowserDriverContainerTest
{
    [TestMethod]
    [TestCategory("Integration")]
    public void BrowserDriverMissingBrowserCleansUpAndRaisesStopTestException()
    {
        var driverCount = BrowserDriverContainer.DriverCount;
        BrowserDriverContainer.NewDriver("Chrome Headless", null);
        Assert.AreEqual(driverCount + 1, BrowserDriverContainer.DriverCount, "One more browser open");
        try
        {
            // Safari should not be installed on this machine. Should not be an issue since it's no longer maintained
            BrowserDriverContainer.NewDriver("Safari", null);
            Assert.Fail("No StopTestException raised");
        }
        catch (StopTestException)
        {
            Assert.AreEqual(0, BrowserDriverContainer.DriverCount, "Browsers should be closed");
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void BrowserDriverContainerNewOptionsTest()
    {
        var caps = new Dictionary<string, string>
        {
            { "string", "value" },
            { "int", "10" }
        };
        var options = BrowserDriverContainer.NewOptions("chrome headless", caps);
        var outCaps = options.ToCapabilities();
        Assert.AreEqual("chrome", options.BrowserName, "Browser name is Chrome");
        Assert.IsTrue(((ChromeOptions)options).Arguments.Contains("headless"),"is headless");
        Assert.AreEqual("value", outCaps.GetCapability("string"), "contains extra capability");
        Assert.AreEqual("chrome", outCaps.GetCapability(CapabilityType.BrowserName), "contains browser name as capability");
        var chromeOptions = outCaps.GetCapability("goog:chromeOptions") as Dictionary<string, object>;
        Assert.IsNotNull(chromeOptions, "chromeOptions not null");
        var args = chromeOptions["args"] as IReadOnlyCollection<object>;
        Assert.IsTrue(args?.Contains("headless"), "headless present");

        var ffOptions = BrowserDriverContainer.NewOptions("firefox");
        var ffOutCaps = ffOptions.ToCapabilities();
        Assert.AreEqual("firefox", ffOutCaps.GetCapability(CapabilityType.BrowserName));
        var ffOutOptions = ffOutCaps.GetCapability("moz:firefoxOptions") as Dictionary<string, object>;
        Assert.IsNotNull(ffOutOptions, "ArgList cannot be mapped to Dictionary");
        var ffPrefs = ffOutOptions["prefs"] as Dictionary<string, object>;
        Assert.IsNotNull(ffPrefs, "ff Prefs not null");
        Assert.IsTrue(ffPrefs.Count >= 4, "ff prefs count ok");
        Assert.IsTrue(ffPrefs.ContainsKey(@"plugin.state.npctrl"), "ff silverlight enabled");
        Assert.IsTrue(ffPrefs.ContainsKey(@"network.negotiate-auth.trusted-uris"), "ff integrated authentication enabled");

    }

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedExceptionWithMessage(typeof(StopTestException),
        "Can't run browser 'Opera' on Selenium server '*'")]
    public void BrowserDriverNonInstalledRemoteDriverRaisesStopTestException() =>
        BrowserDriverContainer.NewRemoteDriver("Opera", EndToEndTest.RemoteSelenium, null);

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedExceptionWithMessage(typeof(StopTestException),
        "Could not start browser: Safari")]
    public void BrowserDriverNonPresentDriverRaisesStopTestException() => BrowserDriverContainer.NewDriver("Safari", null);

    [TestMethod]
    [TestCategory("Unit")]
    public void BrowserDriverRemoveNonExistingDriverTest() => Assert.IsFalse(BrowserDriverContainer.RemoveDriver("bogus"));

    [TestMethod]
    [TestCategory("Integration")]
    public void BrowserDriverSetCurrentTest()
    {
        var browser1 = BrowserDriverContainer.NewDriver("chrome headless", null);
        var browser2 = BrowserDriverContainer.NewDriver("firefox headless", null);
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
    [DataRow(@"proxyautoconfigure", true, 2)]
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
        BrowserDriverContainer.NewRemoteDriver("WrongBrowser", @"wrongaddress", null);

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedExceptionWithMessage(typeof(StopTestException),
        "Unrecognized browser: WrongBrowser")]
    public void BrowserDriverWrongDriverRaisesStopTestException() => BrowserDriverContainer.NewDriver("WrongBrowser", null);

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedExceptionWithMessage(typeof(StopTestException),
        "Can't run browser 'WrongDriver' on Selenium server 'http://localhost'")]
    public void BrowserDriverWrongRemoteDriverRaisesStopTestException() =>
        BrowserDriverContainer.NewRemoteDriver("WrongDriver", "http://localhost", null);
}
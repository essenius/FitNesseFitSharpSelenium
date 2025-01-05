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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using DotNetWindowsRegistry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest;

[TestClass]
public class SeleniumTest
{
    private const string CookiesOkButton = "id:L2AGLb";
    private const string RejectCookiesButton = "id:W0wltc";
    private const string SearchBar = "name:q";
    private Selenium _selenium;

    private static void ExpectStopTestExceptionFor(Action action, string testId, string message)
    {
        try
        {
            action();
            Assert.Fail("No StopTestException raised for test " + testId + " [" + message + "]");
        }
        catch (StopTestException)
        {
            // ignore
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void SeleniumCommandTimeoutSecondsTest()
    {
        Selenium.CommandTimeoutSeconds = 0.1;
        Assert.AreEqual(0.1, Selenium.CommandTimeoutSeconds, "Returned the right timeout value");
        try
        {
            Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
            Assert.Fail("No StopTestException thrown while time out was expected");
        }
        catch (StopTestException e)
        {
            Debug.Assert(e.InnerException != null, "e.InnerException != null");
            var innerMessage = e.InnerException.Message;
            Assert.IsTrue(innerMessage.EndsWith("/session timed out after 0.1 seconds.", StringComparison.Ordinal),
                "Inner message reveals timeout.");
        }
        finally
        {
            Selenium.CommandTimeoutSeconds = 60;
        }
    }


    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumConfigTest()
    {
        var protectedMode = Selenium.Config("InternetExplorer.IgnoreProtectedModeSettings");
        Assert.IsTrue(protectedMode.Matches("true|false"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumDefaultSearchMethodTest()
    {
        Assert.AreEqual("trial", Selenium.DefaultSearchMethod, "Starting default is trial");
        Selenium.DefaultSearchMethod = "CssSelector";
        Assert.AreEqual("CssSelector", Selenium.DefaultSearchMethod, "New default is CssSelector");
        Selenium.DefaultSearchMethod = "id";
    }

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedExceptionWithMessage(typeof(StopTestException), "Edge browser can only work with system proxy")]
    public void SeleniumEdgeCantHandleCustomProxyTest()
    {
        try
        {
            Selenium.SetProxyTypeValue("Manual", "localhost:8888");
            _selenium.SetBrowser("microsoft edge");
        }
        finally
        {
            Selenium.SetProxyType("System");
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumGracefulMissingBrowserHandling()
    {
        _selenium.SetBrowser("none");
        Assert.IsTrue(string.IsNullOrEmpty(_selenium.PageSource), "PageSource without browser");
        Assert.IsTrue(string.IsNullOrEmpty(_selenium.Title()), "Title without browser");
        Assert.IsFalse(_selenium.Close(), "closing without browser");
        Assert.IsFalse(_selenium.ClosePage(), "close page without browser");
        Assert.IsFalse(_selenium.ElementExists("any"), "Element Exists without browser");
        Assert.AreEqual(0, _selenium.CountOfElements("any"), "Count Of Elements without browser");
        Assert.IsTrue(string.IsNullOrEmpty(_selenium.Title()), "Title without browser");
        try
        {
            _selenium.Open(new Uri("http://localhost"));
            Assert.Fail("No exception thrown for Open without browser");
        }
        catch (StopTestException)
        {
            // do nothing
        }
    }

    [TestMethod]
    [TestCategory("Experiments")]
    [SupportedOSPlatform("windows")]
    public void SeleniumIeTimeoutTest()
    {
        Selenium.CommandTimeoutSeconds = 10;
        Assert.IsTrue(_selenium.ProtectedModesAre("EQUAL"), "protected mode OK");
        _selenium.SetBrowser("ie");
        _selenium.Open(EndToEndTest.CreateTestPageUri());
        _selenium.Close();
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumIntegratedAuthenticationDomainTest()
    {
        Selenium.IntegratedAuthenticationDomain = @"seleniumhq.org";
        Assert.AreEqual(@"seleniumhq.org", Selenium.IntegratedAuthenticationDomain);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedException(typeof(StopTestException))]
    public void SeleniumInvalidBrowserTest() => Assert.IsFalse(_selenium.SetBrowser("InvalidBrowser"));

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedException(typeof(StopTestException))]
    public void SeleniumInvalidRemoteBrowserTest() => Assert.IsFalse(_selenium.SetRemoteBrowserAtAddress("InvalidBrowser", "http://localhost"));

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumKeyCodeTest()
    {
        var keycodeMethod = typeof(Selenium).GetMethod("KeyCode", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(keycodeMethod);
        Assert.AreEqual(66, keycodeMethod.Invoke(null, ["Enter"]));
        Assert.AreEqual(4, keycodeMethod.Invoke(null, ["4"]));
        Assert.IsNull(keycodeMethod.Invoke(null, ["NonExistingKeyCode"]));
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void SeleniumLocalChromeGoogleTest()
    {
        Assert.IsTrue(Selenium.SetProxyType("System"), "Set Proxy System");
        Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
        Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")), "Open Uri");
        Assert.IsTrue(_selenium.WaitForElement(CookiesOkButton));
        _selenium.ClickElement(CookiesOkButton);
        Assert.IsTrue(_selenium.WaitForElement("trial:q"));
        Assert.IsTrue(_selenium.Url.Contains("google."), "URL contains google");
        Assert.IsTrue(_selenium.SetElementTo(SearchBar, "Cheese"), "Set q to Cheese");
        Assert.IsTrue(_selenium.SubmitElement(SearchBar), "Submit");
    }

    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumLocalChromeNsTest()
    {
        Selenium.DefaultSearchMethod = "trial";
        const string cookiesOkButton = "cssSelector:button#onetrust-accept-btn-handler";
        Assert.IsTrue(Selenium.SetProxyType("System"), "Set Proxy System");
        Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
        Assert.IsTrue(_selenium.Open(new Uri("http://www.ns.nl")), "Open Uri");
        Assert.IsTrue(_selenium.WaitForElement(cookiesOkButton));
        _selenium.ClickElement(cookiesOkButton);
        Assert.IsTrue(_selenium.WaitForElement("van"));
        _selenium.SetElementTo(@"naar", @"Den Haag HS");
        _selenium.SendKeysToElement(@"Rotterdam Centraal{Enter}", "van");
        _selenium.WaitUntilElementIsClickable(@"Plannen");
        _selenium.ClickElement(@"Plannen");
        _selenium.WaitForElement(@"trial:rio-jp-advice-container-wrapper");

        _selenium.Close();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void SeleniumLocalFirefoxGoogleTest()
    {
        _selenium.SetTimeoutSeconds(30);
        var proxy = AppConfig.Get("Proxy");
        Assert.IsTrue(Selenium.SetProxyTypeValue("Manual", proxy));
        Assert.IsTrue(Selenium.SetProxyType("System"));
        Assert.IsTrue(_selenium.SetBrowser("Firefox"));
        Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")));
        _selenium.ClickElementIfVisible(RejectCookiesButton);
        Assert.IsTrue(_selenium.WaitUntilTitleMatches("Google"));
        Assert.IsTrue(_selenium.WaitForElement(SearchBar));
        Assert.IsTrue(_selenium.Url.Contains("www.google."), "url is something like www.google.");
        Assert.IsTrue(_selenium.SetElementTo(SearchBar, "Cheese"));
        Assert.IsTrue(_selenium.SubmitElement(SearchBar));
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void SeleniumLongPressElementUnsupportedTest()
    {
        _selenium.SetBrowser("Chrome");
        Assert.IsFalse(_selenium.PressKeyCode("a"));
        Assert.IsFalse(_selenium.LongPressKeyCode("a"));
    }

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedException(typeof(StopTestException))]
    public void SeleniumNonInstalledOperaTest() => _selenium.SetBrowser("Opera");

    [TestMethod]
    [TestCategory("Integration")]
    [ExpectedException(typeof(StopTestException))]
    public void SeleniumNonInstalledSafariTest() => _selenium.SetBrowser("Safari");

    [DataTestMethod]
    [TestCategory("Unit")]
    [DataRow("all off", new[] { false, false, false, false }, false, true)]
    [DataRow("all on", new[] { true, true, true, true }, true, true)]
    [DataRow("two on", new[] { false, true, false, true }, false, false)]
    public void SeleniumProtectedModesAreTest(string testId, bool[] zones, bool expectedAllOn, bool expectedAllSame)
    {
        // including deprecated functions here - don't want to duplicate the whole test. Hence, also the #pragma warning disable 618
        if (!OperatingSystem.IsWindows()) return;
        Selenium.ExceptionOnDeprecatedFunctions = false;

        var protectedMode = new ProtectedMode(new ZoneListFactoryMock(zones));

        var property = _selenium.GetType().GetField("_protectedMode", BindingFlags.NonPublic | BindingFlags.Instance);
        Debug.Assert(property != null, nameof(property) + " != null");
        property.SetValue(_selenium, protectedMode);

        if (expectedAllSame)
        {
            Assert.IsTrue(_selenium.ProtectedModesAre("Equal"), testId + " [AllSame - Are Equal]");
#pragma warning disable 618
            Assert.IsTrue(_selenium.ProtectedModesAreEqual(), testId + " [AllSame - AreEqual]");
#pragma warning restore 618
            if (expectedAllOn)
            {
                Assert.IsTrue(_selenium.ProtectedModesAre("On"), testId + " [All On - Are On]");
#pragma warning disable 618
                Assert.IsTrue(_selenium.ProtectedModeIsOn(), testId + " [All On - IsOn - Deprecated]");
#pragma warning restore 618

                ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("Off"), testId, "All On - IsOff");
            }
            else
            {
                Assert.IsTrue(_selenium.ProtectedModesAre("Off"), testId + " [All Off - IsOff]");
#pragma warning disable 618
                Assert.IsTrue(_selenium.ProtectedModeIsOff(), testId + " [All Off - IsOff - Deprecated]");
#pragma warning restore 618
                ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("On"), testId, "All Off - IsOn");
            }
        }
        else
        {
            ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("Equal"), testId, "NotAllSame - AreEqual");
            ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("Off"), testId, "NotAllSame - IsOff");
            ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("On"), testId, "NotAllSame - IsOn");
        }

        Selenium.ExceptionOnDeprecatedFunctions = true;

        var modes = _selenium.ProtectedModePerZone();
        Assert.AreEqual(4, modes.Count);
        Assert.AreEqual(3, modes[0].Count);
        // the rest is already tested in ProtectedModeTest
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumSearchDelimiterTest()
    {
        Assert.AreEqual(":", Selenium.SearchDelimiter, "Starting delimiter is :");
        Selenium.SearchDelimiter = "::";
        Assert.AreEqual("::", Selenium.SearchDelimiter, "New delimiter is ::");
        Selenium.SearchDelimiter = ":";
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumSetNonExistingDriverTest() => Assert.IsFalse(_selenium.SetDriver(@"nonexisting"));

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "No value specified for proxy type 'Manual'")]
    public void SeleniumSetProxyTypeMissingValueTest() => Selenium.SetProxyTypeValue("Manual", string.Empty);

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "No value expected for proxy type 'System'")]
    public void SeleniumSetProxyTypeSurplusValueTest() => Selenium.SetProxyTypeValue("System", "localhost:8088");

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumSetProxyTypeValueTest()
    {
        Assert.IsTrue(Selenium.SetProxyTypeValue("ProxyAutoConfigure", "http://localhost/my.pac"));

        var proxyField = typeof(BrowserDriverContainer).GetField("_proxy", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.IsNotNull(proxyField);
        var proxy = proxyField.GetValue(null) as Proxy;
        Assert.IsNotNull(proxy);

        Assert.AreEqual(ProxyKind.ProxyAutoConfigure, proxy.Kind);
        Assert.AreEqual("http://localhost/my.pac", proxy.ProxyAutoConfigUrl);
        Assert.IsTrue(Selenium.SetProxyTypeValue("Manual", "localhost:8080"));
        proxy = proxyField.GetValue(null) as Proxy;
        Assert.IsNotNull(proxy);
        Assert.AreEqual(ProxyKind.Manual, proxy.Kind);
        Assert.AreEqual("localhost:8080", proxy.HttpProxy);
        Assert.AreEqual("localhost:8080", proxy.SslProxy);
        Assert.IsFalse(Selenium.SetProxyType("Unspecified"));
        proxy = proxyField.GetValue(null) as Proxy;
        Assert.IsNotNull(proxy);
        Assert.AreEqual(ProxyKind.Unspecified, proxy.Kind);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "Unrecognized proxy type 'Bogus'")]
    public void SeleniumSetProxyWrongTypeTest() => Selenium.SetProxyType("Bogus");

    [TestCleanup]
    public void SeleniumTestCleanup()
    {
        _selenium.Close();
        Selenium.SetProxyType("System");
    }

    [TestInitialize]
    public void SeleniumTestInit() => _selenium = new Selenium();

    [TestMethod]
    [TestCategory("Unit")]
    public void SeleniumVersionTest()
    {
        Assert.AreEqual(ApplicationInfo.Version, Selenium.VersionInfo("short"), "Short version info OK");
        Assert.AreEqual(ApplicationInfo.ExtendedInfo, Selenium.VersionInfo("extended"), "Extended version info OK");
        Assert.AreEqual(ApplicationInfo.ApplicationName + " " + ApplicationInfo.Version, Selenium.VersionInfo(""),
            "Default version info OK");
    }

    [TestMethod]
    [TestCategory("Experiments")]
    [SupportedOSPlatform("windows")]
    public void SeleniumZoneTest()
    {
        // this test will fail if not all protected mode settings are the same
        var registry = new WindowsRegistry();
        var protectedModes = new ProtectedMode(new ZoneListFactory(registry));
        var same = protectedModes.AllAreSame();
        Assert.IsTrue(same, "All zones are equal");
    }
}
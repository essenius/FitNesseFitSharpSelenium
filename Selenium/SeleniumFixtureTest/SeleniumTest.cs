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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SeleniumTest
    {
        private Selenium _selenium;

        public TestContext TestContext { get; set; }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "test method")]
        private static void ExpectStopTestExceptionFor(Action action, string testId, string message)
        {
            try
            {
                action();
                Assert.Fail("No StopTestException raised for test " + testId + " [" + message + "]");
            }
            catch (StopTestException)
            {
                Debug.Print("Caught StopTestException");
            }
        }

        [TestMethod, TestCategory("Unit"), DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             @"protectedmode", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml")]
        public void SeleniumProtectedModesAreTest()
        {
            // including deprecated functions here - don't want to duplicate the whole test. Hence also the #pragma warning disable 618
            Selenium.ExceptionOnDeprecatedFunctions = false;

            var testId = TestContext.DataRow["testId"].ToString();
            bool[] zones =
            {
                TestContext.DataRow["zone1"].ToBool(),
                TestContext.DataRow["zone2"].ToBool(),
                TestContext.DataRow["zone3"].ToBool(),
                TestContext.DataRow["zone4"].ToBool()
            };
            var protectedMode = new ProtectedMode(new ZoneListFactoryMock(zones));
            var allOn = TestContext.DataRow["expectedAllOn"].ToBool();
            var allSame = TestContext.DataRow["expectedAllSame"].ToBool();

            var property = _selenium.GetType().GetField("_protectedMode", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(property != null, nameof(property) + " != null");
            property.SetValue(_selenium, protectedMode);

            if (allSame)
            {
                Debug.Print("All same");
                Assert.IsTrue(_selenium.ProtectedModesAre("Equal"), testId + " [AllSame - Are Equal]");
#pragma warning disable 618
                Assert.IsTrue(_selenium.ProtectedModesAreEqual(), testId + " [AllSame - AreEqual]");
#pragma warning restore 618
                if (allOn)
                {
                    Debug.Print("All on");
                    Assert.IsTrue(_selenium.ProtectedModesAre("On"), testId + " [All On - Are On]");
#pragma warning disable 618
                    Assert.IsTrue(_selenium.ProtectedModeIsOn(), testId + " [All On - IsOn - Deprecated]");
#pragma warning restore 618

                    ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("Off"), testId, "All On - IsOff");
                }
                else
                {
                    Debug.Print("all off");
                    Assert.IsTrue(_selenium.ProtectedModesAre("Off"), testId + " [All Off - IsOff]");
#pragma warning disable 618
                    Assert.IsTrue(_selenium.ProtectedModeIsOff(), testId + " [All Off - IsOff - Deprecated]");
#pragma warning restore 618
                    ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAre("On"), testId, "All Off - IsOn");
                }
            }
            else
            {
                Debug.Print("Not all same");
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

        [TestMethod, TestCategory("Integration")]
        public void SeleniumCommandTimeoutSecondsTest()
        {
            Selenium.CommandTimeoutSeconds = 1;
            Assert.AreEqual(1, Selenium.CommandTimeoutSeconds, "Returned the right timeout value");
            try
            {
                Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
                Assert.Fail("No StopTestException thrown while time out was expected");
            }
            catch (StopTestException e)
            {
                Debug.Assert(e.InnerException != null, "e.InnerException != null");
                var innerMessage = e.InnerException.Message;
                Assert.IsTrue(innerMessage.EndsWith("/session timed out after 1 seconds.", StringComparison.Ordinal),
                    "Inner message reveals timeout.");
            }
            finally
            {
                Selenium.CommandTimeoutSeconds = 60;
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumDefaultSearchMethodTest()
        {
            Assert.AreEqual("id", Selenium.DefaultSearchMethod, "Starting default is id");
            Selenium.DefaultSearchMethod = "CssSelector";
            Assert.AreEqual("CssSelector", Selenium.DefaultSearchMethod, "New default is CssSelector");
            Selenium.DefaultSearchMethod = "id";
        }

        [TestMethod, TestCategory("Unit")]
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
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumIntegratedAuthenticationDomainTest()
        {
            Selenium.IntegratedAuthenticationDomain = @"seleniumhq.org";
            Assert.AreEqual(@"seleniumhq.org", Selenium.IntegratedAuthenticationDomain);
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(StopTestException))]
        public void SeleniumInvalidBrowserTest() => Assert.IsFalse(_selenium.SetBrowser("InvalidBrowser"));

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumInvalidRemoteBrowserTest() => Assert.IsFalse(_selenium.SetRemoteBrowserAtAddress("InvalidBrowser", "http://localhost"));

        [TestMethod, TestCategory("Unit")]
        public void SeleniumKeyCodeTest()
        {
            var obj = new PrivateType(typeof(Selenium));

            Assert.AreEqual(66, (int)obj.InvokeStatic("KeyCode", "Enter"));
            Assert.AreEqual(4, (int)obj.InvokeStatic("KeyCode", "4"));
            Assert.IsNull(obj.InvokeStatic("KeyCode", "NonExistingKeyCode"));
        }

        [TestMethod, TestCategory("Integration")]
        public void SeleniumLocalChromeGoogleTest()
        {
            Assert.IsTrue(Selenium.SetProxyType("System"), "Set Proxy System");
            Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
            Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")), "Open Uri");
            Assert.IsTrue(_selenium.WaitForElement("name:q"));
            Assert.IsTrue(_selenium.Url.Contains("google."), "URL contains google");
            Assert.IsTrue(_selenium.SetElementTo("name:q", "Cheese"), "Set q to Cheese");
            Assert.IsTrue(_selenium.SubmitElement("name:q"), "Submit");
        }

        [TestMethod, TestCategory("Integration")]
        public void SeleniumLocalFirefoxGoogleTest()
        {
            _selenium.SetTimeoutSeconds(30);
            var proxy = AppConfig.Get("Proxy");
            Debug.Print("Proxy:" + proxy);
            Assert.IsTrue(Selenium.SetProxyTypeValue("Manual", proxy));
            Assert.IsTrue(Selenium.SetProxyType("System"));
            Assert.IsTrue(_selenium.SetBrowser("Firefox"));
            Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")));
            Assert.IsTrue(_selenium.WaitUntilTitleMatches("Google"));
            Assert.IsTrue(_selenium.WaitForElement("name:q"));
            Assert.IsTrue(_selenium.Url.Contains("www.google."), "url is something like www.google.");
            Assert.IsTrue(_selenium.SetElementTo("name:q", "Cheese"));
            Assert.IsTrue(_selenium.SubmitElement("name:q"));
        }

        [TestMethod, TestCategory("Integration")]
        public void SeleniumLongPressElementUnsupportedTest()
        {
            _selenium.SetBrowser("Chrome");
            Assert.IsFalse(_selenium.PressKeyCode("a"));
            Assert.IsFalse(_selenium.LongPressKeyCode("a"));
        }

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumNonInstalledOperaTest() => _selenium.SetBrowser("Opera");

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumNonInstalledSafariTest() => _selenium.SetBrowser("Safari");

        [TestMethod, TestCategory("Unit")]
        public void SeleniumSearchDelimiterTest()
        {
            Assert.AreEqual(":", Selenium.SearchDelimiter, "Starting delimiter is :");
            Selenium.SearchDelimiter = "::";
            Assert.AreEqual("::", Selenium.SearchDelimiter, "New delimiter is ::");
            Selenium.SearchDelimiter = ":";
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumSetNonExistingDriverTest() => Assert.IsFalse(_selenium.SetDriver(@"nonexisting"));

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException), "No value specified for proxy type 'Manual'")]
        public void SeleniumSetProxyTypeMissingValueTest() => Selenium.SetProxyTypeValue("Manual", string.Empty);

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException), "No value expected for proxy type 'System'")]
        public void SeleniumSetProxyTypeSurplusValueTest() => Selenium.SetProxyTypeValue("System", "localhost:8088");

        [TestMethod, TestCategory("Unit")]
        public void SeleniumSetProxyTypeValueTest()
        {
            Assert.IsTrue(Selenium.SetProxyTypeValue("ProxyAutoConfigure", "http://localhost/my.pac"));
            var proxy = new PrivateType(typeof(BrowserDriver)).GetStaticField("_proxy") as Proxy;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(ProxyKind.ProxyAutoConfigure, proxy.Kind);
            Assert.AreEqual("http://localhost/my.pac", proxy.ProxyAutoConfigUrl);
            Assert.IsTrue(Selenium.SetProxyTypeValue("Manual", "localhost:8080"));
            proxy = new PrivateType(typeof(BrowserDriver)).GetStaticField("_proxy") as Proxy;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(ProxyKind.Manual, proxy.Kind);
            Assert.AreEqual("localhost:8080", proxy.HttpProxy);
            Assert.AreEqual("localhost:8080", proxy.SslProxy);
            Assert.IsFalse(Selenium.SetProxyType("Unspecified"));
            proxy = new PrivateType(typeof(BrowserDriver)).GetStaticField("_proxy") as Proxy;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(ProxyKind.Unspecified, proxy.Kind);
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException), "Unrecognized proxy type 'Bogus'")]
        public void SeleniumSetProxyWrongTypeTest() => Selenium.SetProxyType("Bogus");

        [TestCleanup]
        public void SeleniumTestCleanup()
        {
            _selenium.Close();
            Selenium.SetProxyType("System");
        }

        [TestInitialize]
        public void SeleniumTestInit() => _selenium = new Selenium();

        [TestMethod, TestCategory("Unit")]
        public void SeleniumVersionTest()
        {
            Assert.AreEqual(ApplicationInfo.Version, Selenium.VersionInfo("short"), "Short version info OK");
            Assert.AreEqual(ApplicationInfo.ExtendedInfo, Selenium.VersionInfo("extended"), "Extended version info OK");
            Assert.AreEqual(ApplicationInfo.ApplicationName + " " + ApplicationInfo.Version, Selenium.VersionInfo(""),
                "Default version info OK");
        }
    }
}
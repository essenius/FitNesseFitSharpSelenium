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
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SeleniumTest
    {
        private Selenium _selenium;

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive"),
         SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "False positive")]
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
        public void ProtectedModeAllAreTest()
        {
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
                Assert.IsTrue(_selenium.ProtectedModesAreEqual(), testId + " [NotAllSame - AreEqual]");
                if (allOn)
                {
                    Debug.Print("All on");
                    Assert.IsTrue(_selenium.ProtectedModeIsOn(), testId + " [All On - IsOn]");
                    ExpectStopTestExceptionFor(() => _selenium.ProtectedModeIsOff(), testId, "All On - IsOff");
                }
                else
                {
                    Debug.Print("all off");
                    Assert.IsTrue(_selenium.ProtectedModeIsOff(), testId + " [All Off - IsOff]");
                    ExpectStopTestExceptionFor(() => _selenium.ProtectedModeIsOn(), testId, "All Off - IsOn");
                }
            }
            else
            {
                Debug.Print("Not all same");
                ExpectStopTestExceptionFor(() => _selenium.ProtectedModesAreEqual(), testId, "NotAllSame - AreEqual");
                ExpectStopTestExceptionFor(() => _selenium.ProtectedModeIsOff(), testId, "NotAllSame - IsOff");
                ExpectStopTestExceptionFor(() => _selenium.ProtectedModeIsOn(), testId, "NotAllSame - IsOn");
            }

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
            Assert.IsTrue(string.IsNullOrEmpty(_selenium.HtmlSource()), "HtmlSource without browser");
            Assert.IsTrue(string.IsNullOrEmpty(_selenium.Title()), "Title without browser");
            Assert.IsFalse(_selenium.Close(), "closing without browser");
            Assert.IsFalse(_selenium.ClosePage(), "close page without browser");
            Assert.IsFalse(_selenium.ElementExists("any"), "Element Exists without browser");
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
        public void SeleniumInvalidBrowserTest()
        {
            Assert.IsFalse(_selenium.SetBrowser("InvalidBrowser"));
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumInvalidProxy()
        {
            Assert.IsFalse(Selenium.SetProxyTypeValue("Wrong Proxy Type", "irrelevant value"));
        }

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumInvalidRemoteBrowserTest()
        {
            Assert.IsFalse(_selenium.SetRemoteBrowserAtAddress("InvalidBrowser", "http://localhost"));
        }

        [TestMethod, TestCategory("Integration")]
        public void SeleniumLocalChromeGoogleTest()
        {
            Assert.IsTrue(Selenium.SetProxyType("System"), "Set Proxy System");
            Assert.IsTrue(_selenium.SetBrowser("Chrome"), "Set Browser Chrome");
            Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")), "Open Uri");
            Assert.IsTrue(_selenium.SetElementTo("name:q", "Cheese"), "Set q to Cheese");
            Assert.IsTrue(_selenium.SubmitElement("name:q"), "Submit");
        }

        [TestMethod, TestCategory("Integration")]
        public void SeleniumLocalFirefoxGoogleTest()
        {
            var proxy = ConfigurationManager.AppSettings.Get("Proxy");
            Assert.IsTrue(Selenium.SetProxyTypeValue("Manual", proxy));
            Assert.IsTrue(Selenium.SetProxyType("System"));
            Assert.IsTrue(_selenium.SetBrowser("Firefox"));
            Assert.IsTrue(_selenium.Open(new Uri("http://www.google.com")));
            Assert.IsTrue(_selenium.Url.Contains("www.google."), "url is something like www.google.");
            Assert.IsTrue(_selenium.SetElementTo("name:q", "Cheese"));
            Assert.IsTrue(_selenium.SubmitElement("name:q"));
        }

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumNonInstalledOperaTest()
        {
            _selenium.SetBrowser("Opera");
        }

        [TestMethod, TestCategory("Integration"), ExpectedException(typeof(StopTestException))]
        public void SeleniumNonInstalledSafariTest()
        {
            _selenium.SetBrowser("Safari");
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumSearchDelimiterTest()
        {
            Assert.AreEqual(":", Selenium.SearchDelimiter, "Starting delimiter is :");
            Selenium.SearchDelimiter = "::";
            Assert.AreEqual("::", Selenium.SearchDelimiter, "New delimiter is ::");
            Selenium.SearchDelimiter = ":";
        }

        [TestMethod, TestCategory("Unit")]
        public void SeleniumSetNonExistingDriverTest()
        {
            Assert.IsFalse(_selenium.SetDriver(@"nonexisting"));
        }

        [TestCleanup]
        public void SeleniumTestCleanup()
        {
            _selenium.Close();
            Selenium.SetProxyType("System");
        }

        [TestInitialize]
        public void SeleniumTestInit()
        {
            _selenium = new Selenium();
        }

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
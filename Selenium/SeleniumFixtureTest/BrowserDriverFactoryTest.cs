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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using SeleniumFixture;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class BrowserDriverFactoryTest
    {
        private static object GetArgList(ICapabilities cap, string keyName)
        {
            var options = cap.GetCapability(keyName) as Dictionary<string, object>;
            Assert.IsNotNull(options);
            var argList = options.Values.ToList()[0];
            Assert.IsNotNull(argList, "ArgList is empty");
            return argList;
        }

        private static void ValidateChromeCapabilities(ICapabilities cap, bool headless)
        {
            Assert.AreEqual(@"chrome", cap.GetCapability(CapabilityType.BrowserName));
            var argList = GetArgList(cap, @"goog:chromeOptions") as IReadOnlyCollection<object>;
            Assert.IsNotNull(argList, "argList cannot be mapped to IReadOnlyCollection ");
            Assert.IsTrue(argList.Count >= (headless ? 2 : 1), "arg count ok");
            Assert.IsTrue(argList.Contains(@"test-type"), "test-type specified");
            Assert.AreEqual(headless, argList.Contains(@"headless"), "headless ok");
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

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive"),
         SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        /*
        [TestMethod, TestCategory("Unit")]
        public void BrowserDriverFactoryGetOptionsTest()
        {
            var factory = new BrowserDriverFactory(new Proxy { Kind = ProxyKind.System }, TimeSpan.FromSeconds(73));
            var noAdditionalCapabilities = new Dictionary<string, object>();

            var chromeCapabilities = factory.GetRemoteOptions("chrome", noAdditionalCapabilities).ToCapabilities();
            ValidateChromeCapabilities(chromeCapabilities, false);
            Assert.IsFalse(chromeCapabilities.HasCapability("javascriptEnabled"), "javascriptEnabled capability does not exist");

            var headlessCapabilities = factory.GetRemoteOptions("chrome headless", noAdditionalCapabilities).ToCapabilities();
            ValidateChromeCapabilities(headlessCapabilities, true);

            var edgeOptions = factory.GetRemoteOptions("edge", noAdditionalCapabilities);
            Assert.AreEqual("MicrosoftEdge", edgeOptions.BrowserName);
            Assert.AreEqual(PageLoadStrategy.Eager, edgeOptions.PageLoadStrategy);

            var ffCapabilities = factory.GetRemoteOptions("ff", noAdditionalCapabilities).ToCapabilities();
            ValidateFirefoxCapabilities(ffCapabilities);

            var ieOptions = factory.GetRemoteOptions("ie", noAdditionalCapabilities);
            Assert.AreEqual("internet explorer", ieOptions.BrowserName);
            Assert.AreEqual("windows", ieOptions.PlatformName);

            var operaOptions = factory.GetRemoteOptions("opera", noAdditionalCapabilities);
            Assert.AreEqual("opera", operaOptions.BrowserName);

            var safariOptions = factory.GetRemoteOptions("safari", noAdditionalCapabilities);
            Assert.AreEqual("safari", safariOptions.BrowserName);

            var noOptions = factory.GetRemoteOptions("none", noAdditionalCapabilities);
            Assert.IsNull(noOptions);

            var additionalCapabilities = new Dictionary<string, object> { { "javascriptEnabled", false } };
            var chromeOptions2 = factory.GetRemoteOptions("chrome", additionalCapabilities);
            Assert.IsNotNull(chromeOptions2);
            //Assert.IsFalse(chromeCapabilities2.GetCapability("javascriptEnabled").ToBool(), "Additional capabilities found");
        } */

        [TestMethod, TestCategory("Unit")]
        public void BrowserDriverFactoryGetDefaultServiceTest()
        {
            using (var service1 = BrowserDriverFactory.GetDefaultService<InternetExplorerDriverService>())
            {
                Assert.IsInstanceOfType(service1, typeof(InternetExplorerDriverService));
                Assert.IsFalse(service1.IsRunning);
            }
            try
            {
                BrowserDriverFactory.GetDefaultService<ChromeDriverService>(@"c:\");
                Assert.Fail("Expected exception didn't happen");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsNotNull(ex.InnerException, "ex.InnerException != null");
                Assert.IsTrue(ex.InnerException.Message.StartsWith(@"The file c:\chromedriver.exe does not exist"));
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void BrowserDriverFactoryGetDefaultServiceFirefoxTest()
        {
            using (var service1 = BrowserDriverFactory.GetDefaultService<FirefoxDriverService>())
            {
                Assert.IsInstanceOfType(service1, typeof(FirefoxDriverService));
                Assert.IsFalse(service1.IsRunning);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void BrowserDriverFactoryGetDesiredCapabilitiesTest()
        {
            var factory = new BrowserDriverFactory(new Proxy {Kind = ProxyKind.System}, TimeSpan.FromSeconds(60));
            var noAdditionalCapabilities = new Dictionary<string, object>();

            var chromeCapabilities = factory.GetDesiredCapabilities("chrome", noAdditionalCapabilities);
            ValidateChromeCapabilities(chromeCapabilities, false);
            Assert.IsFalse(chromeCapabilities.HasCapability(CapabilityType.IsJavaScriptEnabled),
                "javascriptEnabled capability does not exist");

            var headlessCapabilities = factory.GetDesiredCapabilities("chrome headless", noAdditionalCapabilities);
            ValidateChromeCapabilities(headlessCapabilities, true);

            var edgeCapabilities = factory.GetDesiredCapabilities("edge", noAdditionalCapabilities);
            Assert.AreEqual("MicrosoftEdge", edgeCapabilities.GetCapability(CapabilityType.BrowserName));
            Assert.AreEqual("eager", edgeCapabilities.GetCapability(CapabilityType.PageLoadStrategy));
            var ffCapabilities = factory.GetDesiredCapabilities("ff", noAdditionalCapabilities);
            ValidateFirefoxCapabilities(ffCapabilities);

            var ieCapabilities = factory.GetDesiredCapabilities("ie", noAdditionalCapabilities);
            Assert.AreEqual("internet explorer", ieCapabilities.GetCapability(CapabilityType.BrowserName));
            Assert.AreEqual("windows", ieCapabilities.GetCapability(CapabilityType.PlatformName));

            var operaCapabilities = factory.GetDesiredCapabilities("opera", noAdditionalCapabilities);
            Assert.AreEqual("opera", operaCapabilities.GetCapability(CapabilityType.BrowserName));

            var safariCapabilities = factory.GetDesiredCapabilities("safari", noAdditionalCapabilities);
            Assert.AreEqual("safari", safariCapabilities.GetCapability(CapabilityType.BrowserName));
            //Assert.AreEqual("ANY", safariCapabilities.Platform.ProtocolPlatformType);

            var noCapabilities = factory.GetDesiredCapabilities("none", noAdditionalCapabilities);
            Assert.IsNull(noCapabilities);

            var additionalCapabilities = new Dictionary<string, object> {{"javascriptEnabled", false}};
            var chromeCapabilities2 = factory.GetDesiredCapabilities("chrome", additionalCapabilities);
            Assert.IsFalse(chromeCapabilities2.GetCapability("javascriptEnabled").ToBool(), "Additional capabilities found");
        }

        [DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             @"standardizebrowsername", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml"), TestMethod,
         TestCategory("Unit")]
        public void BrowserDriverFactoryStandardizeBrowserNameTest()
        {
            var privateTarget = new PrivateType(typeof(BrowserDriverFactory));
            var input = TestContext.DataRow["input"].ToString();
            var expected = TestContext.DataRow["expected"].ToString();
            Assert.AreEqual(expected, privateTarget.InvokeStatic(@"StandardizeBrowserName", input));
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(StopTestException),
             "Internet Explorer requires a screen scaling of 100%. Set via Control Panel/Display Settings")]
        public void BrowserDriverFactoryWrongScreenSizeIdentifiedTest()
        {
            var factory = new BrowserDriverFactory(new Proxy {Kind = ProxyKind.System}, TimeSpan.FromSeconds(60));
            var factoryWrapper = new PrivateObject(factory);
            factoryWrapper.SetFieldOrProperty("_nativeMethods", new NativeMethodsMock());
            factory.CreateLocalDriver("IE");
        }
    }
}
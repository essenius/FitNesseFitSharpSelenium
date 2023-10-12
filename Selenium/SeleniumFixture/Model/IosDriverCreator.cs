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

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal class IosDriverCreator : BrowserDriverCreator
    {
        public IosDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "IOS";

        public override IWebDriver LocalDriver(object options) => null;

        public override DriverOptions Options() => new AppiumOptions { PlatformName = "iOS", Proxy = Proxy };

        public override IWebDriver RemoteDriver(string baseAddress, Dictionary<string, object> capabilities)
        {
            var uri = BaseUri(baseAddress);
            var options = RemoteOptions(capabilities);
            return new IOSDriver<AppiumWebElement>(uri, options, Timeout);
        }

        public override IWebDriver RemoteDriver(string baseAddress, DriverOptions options)
        {
            var uri = BaseUri(baseAddress);
            return new IOSDriver(uri, options, Timeout);
        }
    }
}

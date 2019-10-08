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
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal class WinAppDriverCreator : BrowserDriverCreator
    {
        public WinAppDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "WINAPP";

        public override IWebDriver LocalDriver() => null;

        public override DriverOptions Options() => WinAppOptions();

        private AppiumOptions WinAppOptions()
        {
            var options = new AppiumOptions {PlatformName = "Windows", Proxy = Proxy};
            return options;
        }

        public override IWebDriver RemoteDriver(string baseAddress, Dictionary<string, object> capabilities)
        {
            var uri = BaseUri(baseAddress);
            var options = WinAppOptions();
            options.AddAdditionalCapabilities(capabilities);
            return new WindowsDriver<WindowsElement>(uri, options, Timeout);
        }

        // Apparently the Windows driver doesn't expect /wd/hub, unlike all other drivers.
        protected override Uri BaseUri(string baseAddress) => new Uri(baseAddress);

    }
}

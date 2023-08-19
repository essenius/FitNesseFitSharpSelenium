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
using OpenQA.Selenium.Appium.Windows;


namespace SeleniumFixture.Model
{
    internal class WinAppDriverCreator : BrowserDriverCreator
    {
        public WinAppDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name => @"WINAPP";

        // Apparently the Windows driver doesn't expect /wd/hub, unlike all other drivers.

        //protected override string BasePath { get; set; } = "";

        public override IWebDriver LocalDriver(object options) => null;

        public override DriverOptions Options() => WinAppOptions();

        public override IWebDriver RemoteDriver(string baseAddress, DriverOptions options)
        {
            var uri = BaseUri(baseAddress);
            var appiumOptions = options as AppiumOptions;
            return new WindowsDriver(uri, appiumOptions, Timeout);
        }

        private AppiumOptions WinAppOptions()
        {
            var options = new AppiumOptions
            {
                PlatformName = "Windows", 
                Proxy = null,
                // TODO: add proxy back when Appium supports it
                DeviceName = "WindowsPC",
                AutomationName = "Windows"
            };
            return options;
        }
    }
}

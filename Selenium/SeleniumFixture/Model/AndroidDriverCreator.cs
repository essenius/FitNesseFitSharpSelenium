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
using OpenQA.Selenium.Appium.Android;

namespace SeleniumFixture.Model
{
    internal class AndroidDriverCreator : BrowserDriverCreator
    {
        public AndroidDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "ANDROID";

        public override IWebDriver LocalDriver() => null;

        // This command fails when using Appium with WebDriver 4.
        // Under the hood it uses DesiredCapabilities which is not allowed in WebDriver 4.
        public override DriverOptions Options() => new AppiumOptions {PlatformName = "Android", Proxy = Proxy};

        public override IWebDriver RemoteDriver(string baseAddress, Dictionary<string, object> capabilities)
        {
            var uri = BaseUri(baseAddress);
            var options = RemoteOptions(capabilities);
            return new AndroidDriver<AppiumWebElement>(uri, options, Timeout);
        }
    }
}

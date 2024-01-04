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
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace SeleniumFixture.Model;

internal class AndroidDriverCreator : BrowserDriverCreator
{
    private AppiumOptions _options;

    public AndroidDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
    {
    }

    public override string Name => "ANDROID";

    public override IWebDriver LocalDriver(object options) => null;

    public override DriverOptions Options()
    {
        if (_options != null) return _options;
        // disabled proxy since it's not supported in Appium
        // TODO: add proxy support when it's fixed
        _options = new AppiumOptions
        {
            PlatformName = "Android", 
            Proxy = null,
            AutomationName = "UiAutomator2"
        };
        return _options;
    }

    public override IWebDriver RemoteDriver(string baseAddress, DriverOptions options)
    {
        var uri = BaseUri(baseAddress);
        return new AndroidDriver(uri, options, Timeout);
    }
}
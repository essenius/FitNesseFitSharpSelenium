﻿// Copyright 2015-2024 Rik Essenius
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
using OpenQA.Selenium.Chrome;

namespace SeleniumFixture.Model;

internal class ChromeDriverCreator(Proxy proxy, TimeSpan timeout) : BrowserDriverCreator(proxy, timeout)
{
    public override string Name => "CHROME";

    protected virtual ChromeOptions ChromeOptions() => new() { Proxy = Proxy };

    public override IWebDriver LocalDriver(object options)
    {
        var driverFolder = ConfiguredFolder("ChromeWebDriver");
        ChromeDriverService driverService = null;
        IWebDriver driver;
        try
        {
            driverService = GetDefaultService<ChromeDriverService>(driverFolder);
            var chromeOptions = options == null ? ChromeOptions() : (ChromeOptions)options;
            driver = new ChromeDriver(driverService, chromeOptions, Timeout);
        }
        catch
        {
            // creating the driver failed. We need to dispose the service ourselves.
            driverService?.Dispose();
            throw;
        }
        return driver;
    }

    public override DriverOptions Options() => ChromeOptions();
}
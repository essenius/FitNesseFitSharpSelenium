// Copyright 2015-2021 Rik Essenius
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
using OpenQA.Selenium.Safari;

namespace SeleniumFixture.Model
{
    internal class SafariDriverCreator : BrowserDriverCreator
    {
        // didn't find a way to enable proxies or integrated authentication on Safari

        public SafariDriverCreator(TimeSpan timeout) : base(null, timeout)
        {
        }

        public override string Name { get; } = "SAFARI";

        public override IWebDriver LocalDriver(object options)
        {
            SafariDriverService driverService = null;
            IWebDriver driver;
            try
            {
                driverService = GetDefaultService<SafariDriverService>();
                var safariOptions = options == null ? SafariOptions() : (SafariOptions)options;
                driver = new SafariDriver(driverService, safariOptions, Timeout);
            }
            catch
            {
                driverService?.Dispose();
                throw;
            }
            return driver;
        }

        public override DriverOptions Options() => SafariOptions();

        private static SafariOptions SafariOptions()
        {
            var options = new SafariOptions();
            options.AddAdditionalOption("safari.options", "skipExtensionInstallation");
            return options;
        }
    }
}

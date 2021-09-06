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
using OpenQA.Selenium.IE;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal class InternetExplorerDriverCreator : BrowserDriverCreator
    {
        private readonly NativeMethods _nativeMethods = new();

        public InternetExplorerDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "IE";

        private static string EdgePath() => AppConfig.Get("InternetExplorer.EdgePath");

        private static bool IgnoreProtectedModeSetting()
        {
            var ignoreProtectedModeSettingsString = AppConfig.Get("InternetExplorer.IgnoreProtectedModeSettings");
            return !string.IsNullOrEmpty(ignoreProtectedModeSettingsString) &&
                   bool.Parse(ignoreProtectedModeSettingsString);
        }

        private InternetExplorerOptions InternetExplorerOptions()
        {
            var options = new InternetExplorerOptions
            {
                Proxy = Proxy,
                IntroduceInstabilityByIgnoringProtectedModeSettings = IgnoreProtectedModeSetting()
            };
            var edgePath = EdgePath();

            if (string.IsNullOrEmpty(edgePath)) return options;
            options.AddAdditionalCapability("ie.edgechromium", true);
            options.AddAdditionalCapability("ie.edgepath", edgePath);
            return options;
        }

        public override IWebDriver LocalDriver()
        {
            if (!_nativeMethods.ScreenScalingIs1()) throw new StopTestException(ErrorMessages.IEScreenScalingIssue);

            var driverFolder = Environment.GetEnvironmentVariable("IEWebDriver");
            InternetExplorerDriverService driverService = null;
            IWebDriver driver;
            try
            {
                driverService = GetDefaultService<InternetExplorerDriverService>(driverFolder);
                driver = new InternetExplorerDriver(driverService, InternetExplorerOptions(), Timeout);
            }
            catch
            {
                driverService?.Dispose();
                throw;
            }
            return driver;
        }

        protected override DriverOptions Options() => InternetExplorerOptions();
    }
}

﻿// Copyright 2015-2021 Rik Essenius
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
using System.IO;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;

namespace SeleniumFixture.Model
{
    internal class EdgeDriverCreator : BrowserDriverCreator
    {
        // we need this one to stay alive while the driver is alive
        private readonly EdgeDriverService _driverService;

        public EdgeDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
            var driverFolder = Environment.GetEnvironmentVariable("EdgeWebDriver");
            try
            {
                _driverService = driverFolder == null
                    ? EdgeDriverService.CreateChromiumService()
                    : EdgeDriverService.CreateChromiumService(driverFolder);
            }
            catch (DriverServiceNotFoundException)
            {
                // Ignore, we might not need the driver
            }
        }

        public override string Name => "EDGE";

        protected virtual EdgeOptions EdgeOptions()
        {
            if (_driverService == null) throw new DriveNotFoundException("Could not find Edge driver");
            // this is still the case in the new Edge - it ignores proxy settings in Options
            if (Proxy.Kind != ProxyKind.System) throw new StopTestException(ErrorMessages.EdgeNeedsSystemProxy);
            var options = new EdgeOptions { UseChromium = _driverService.UsingChromium };
            return options;
        }

        public override IWebDriver LocalDriver(object options)
        {
            var edgeOptions = options == null ? EdgeOptions() : (EdgeOptions)options;
            return new EdgeDriver(_driverService, edgeOptions, Timeout);
        }

        public override DriverOptions Options() => EdgeOptions();
    }
}

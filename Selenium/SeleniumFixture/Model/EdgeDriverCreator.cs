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
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace SeleniumFixture.Model
{
    internal class EdgeDriverCreator : BrowserDriverCreator
    {
        public EdgeDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "EDGE";
        public override IWebDriver LocalDriver()
        {
            EdgeDriverService driverService = null;
            IWebDriver driver;
            try
            {
                driverService = GetDefaultService<EdgeDriverService>();
                driver = new EdgeDriver(driverService, EdgeOptions(), Timeout);
            }
            catch
            {
                driverService?.Dispose();
                throw;
            }
            return driver;
        }

        private EdgeOptions EdgeOptions()
        {
            if (Proxy.Kind != ProxyKind.System) throw new StopTestException(ErrorMessages.EdgeNeedsSystemProxy);
            var options = new EdgeOptions { PageLoadStrategy = PageLoadStrategy.Eager };
            return options;
        }

        public override DriverOptions Options() => EdgeOptions();

    }
}

// Copyright 2015-2020 Rik Essenius
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

namespace SeleniumFixture.Model
{
    internal class BrowserDriverFactory
    {
        private const StringComparison IgnoreCase = StringComparison.InvariantCultureIgnoreCase;
        private readonly List<BrowserDriverCreator> _browserDriverCreators;

        public BrowserDriverFactory(Proxy proxy, TimeSpan timeout) =>
            _browserDriverCreators = new List<BrowserDriverCreator>
            {
                new AndroidDriverCreator(proxy, timeout),
                new ChromeDriverCreator(proxy, timeout),
                new HeadlessChromeDriverCreator(proxy, timeout),
                new EdgeDriverCreator(proxy, timeout),
                new HeadlessEdgeDriverCreator(proxy, timeout),
                new FireFoxDriverCreator(proxy, timeout),
                new HeadlessFirefoxDriverCreator(proxy, timeout),
                new InternetExplorerDriverCreator(proxy, timeout),
                new IosDriverCreator(proxy, timeout),
                new OperaDriverCreator(proxy, timeout),
                new SafariDriverCreator(timeout),
                new WinAppDriverCreator(proxy, timeout),
                new NoBrowserDriverCreator()
            };

        private BrowserDriverCreator BrowserDriverCreatorFor(string browserName)
        {
            var standardBrowserName = StandardizeBrowserName(browserName);
            foreach (var creator in _browserDriverCreators)
            {
                if (creator.Name.Equals(standardBrowserName, IgnoreCase)) return creator;
            }
            throw new StopTestException("Unrecognized browser: " + browserName);
        }

        public DriverOptions CreateOptions(string browserName)
        {
            var browserDriverCreator = BrowserDriverCreatorFor(browserName);
            return browserDriverCreator.Options();
        }

        public IWebDriver CreateLocalDriver(string browserName, object options)
        {
            var browserDriverCreator = BrowserDriverCreatorFor(browserName);
            return browserDriverCreator.LocalDriver(options);
        }

        public IWebDriver CreateRemoteDriver(string browserName, string baseAddress, Dictionary<string, object> capabilities)
        {
            var browserDriverCreator = BrowserDriverCreatorFor(browserName);
            return browserDriverCreator.RemoteDriver(baseAddress, capabilities);
        }

        public IWebDriver CreateRemoteDriver(string browserName, string baseAddress, DriverOptions options)
        {
            var browserDriverCreator = BrowserDriverCreatorFor(browserName);
            return browserDriverCreator.RemoteDriver(baseAddress, options);
        }

        private static string StandardizeBrowserName(string browserName)
        {
            var browserInUpperCase = browserName.Replace(" ", string.Empty).ToUpperInvariant();
            return browserInUpperCase switch
            {
                "GOOGLECHROME" => "CHROME",
                "GOOGLECHROMEHEADLESS" => "CHROMEHEADLESS",
                "MICROSOFTEDGE" => "EDGE",
                "MSEDGE" => "EDGE",
                "FF" => "FIREFOX",
                "FFHEADLESS" => "FIREFOXHEADLESS",
                "INTERNETEXPLORER" => "IE",
                _ => browserInUpperCase
            };
        }
    }
}

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
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal class BrowserDriverFactory
    {
        // This is a workaround for the absence of an IDriverService interface
        // T is the driver service to be instantiated (e.g. ChromeDriverService)
        internal static T GetDefaultService<T>(string driverFolder = null)
        {
            var serviceType = typeof(T);
            var typeList = new List<Type>();
            var parameterList = new List<object>();
            if (!string.IsNullOrEmpty(driverFolder))
            {
                typeList.Add(typeof(string));
                parameterList.Add(driverFolder);
            }

            var methodInfo = serviceType.GetMethod("CreateDefaultService", typeList.ToArray());
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            return (T)methodInfo.Invoke(serviceType, parameterList.ToArray());
        }

        public static string IntegratedAuthenticationDomain { get; set; } =
            ConfigurationManager.AppSettings.Get("Firefox.IntegratedAuthenticationDomain");

        private static bool InternetExplorerIgnoreProtectedModeSetting()
        {
            var ignoreProtectedModeSettingsString =
                ConfigurationManager.AppSettings.Get("InternetExplorer.IgnoreProtectedModeSettings");
            return !string.IsNullOrEmpty(ignoreProtectedModeSettingsString) &&
                   bool.Parse(ignoreProtectedModeSettingsString);
        }

        private static string StandardizeBrowserName(string browserName)
        {
            var browserInUpperCase = browserName.Replace(" ", string.Empty).ToUpperInvariant();
            switch (browserInUpperCase)
            {
                case @"GOOGLECHROME":
                    return @"CHROME";
                case @"GOOGLECHROMEHEADLESS":
                    return @"CHROMEHEADLESS";
                case @"INTERNETEXPLORER":
                    return @"IE";
                case @"FF":
                    return @"FIREFOX";
                case @"FFHEADLESS":
                    return @"FIREFOXHEADLESS";
                default:
                    return browserInUpperCase;
            }
        }

        private readonly Dictionary<string, Func<IWebDriver>> _localDrivers;
        private readonly Proxy _proxy;
        private readonly Dictionary<string, Func<DriverOptions>> _remoteOptions;
        private readonly TimeSpan _timeout;

        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local", Justification = "Changed in tests")]
        private NativeMethods _nativeMethods = new NativeMethods();

        public BrowserDriverFactory(Proxy proxy, TimeSpan timeout)
        {
            _proxy = proxy;
            _timeout = timeout;
            _localDrivers = new Dictionary<string, Func<IWebDriver>>
            {
                {@"CHROME", () => CreateLocalChromeDriver(false)},
                {@"CHROMEHEADLESS", () => CreateLocalChromeDriver(true)},
                {@"EDGE", CreateLocalEdgeDriver},
                {@"FIREFOX", () => CreateLocalFirefoxDriver(false)},
                {@"FIREFOXHEADLESS", () => CreateLocalFirefoxDriver(true)},
                {@"IE", CreateLocalInternetExplorerDriver},
                // I was unable to install Opera on my machine, so I could not test this. 
                {@"OPERA", CreateLocalOperaDriver},
                {@"SAFARI", CreateLocalSafariDriver},
                {@"NONE", () => null}
            };

            _remoteOptions = new Dictionary<string, Func<DriverOptions>>
            {
                {@"CHROME", () => GetChromeOptions(false)},
                {@"CHROMEHEADLESS", () => GetChromeOptions(true)},
                {@"EDGE", GetEdgeOptions},
                {@"FIREFOX", () => GetFirefoxOptions(false)},
                {@"FIREFOXHEADLESS", () => GetFirefoxOptions(true)},
                {@"IE", GetInternetExplorerOptions},
                {@"OPERA", () => new OperaOptions {Proxy = _proxy}},
                {@"SAFARI", GetSafariOptions}, // remote still possible
                {@"NONE", () => null}
            };
        }

        // I tried to make these methods smarter (eliminate redundancy) but that is not easy
        // with all the hard dependencies that browser drivers have on their services and options 

        private IWebDriver CreateLocalChromeDriver(bool headless)
        {
            var driverFolder = Environment.GetEnvironmentVariable("ChromeWebDriver");
            var driverService = GetDefaultService<ChromeDriverService>(driverFolder);
            IWebDriver driver;
            try
            {
                driver = new ChromeDriver(driverService, GetChromeOptions(headless), _timeout);
            }
            catch
            {
                // creating the driver failed. We need to dispose the service ourselves.
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        public IWebDriver CreateLocalDriver(string browserName)
        {
            var standardBrowserName = StandardizeBrowserName(browserName);
            if (_localDrivers.ContainsKey(standardBrowserName))
            {
                return _localDrivers[standardBrowserName]();
            }
            throw new StopTestException("Unrecognized browser: " + browserName);
        }

        private IWebDriver CreateLocalEdgeDriver()
        {
            var driverService = GetDefaultService<EdgeDriverService>();
            IWebDriver driver;
            try
            {
                driver = new EdgeDriver(driverService, GetEdgeOptions(), _timeout);
            }
            catch
            {
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        private IWebDriver CreateLocalFirefoxDriver(bool headless)
        {
            var driverFolder = Environment.GetEnvironmentVariable("GeckoWebDriver");
            var driverService = GetDefaultService<FirefoxDriverService>(driverFolder);
            IWebDriver driver;
            try
            {
                driver = new FirefoxDriver(driverService, GetFirefoxOptions(headless), _timeout);
            }
            catch
            {
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        private IWebDriver CreateLocalInternetExplorerDriver()
        {
            if (!_nativeMethods.ScreenScalingIs1())
            {
                throw new StopTestException("Internet Explorer requires a screen scaling of 100%. Set via Control Panel/Display Settings");
            }

            var driverFolder = Environment.GetEnvironmentVariable("IEWebDriver");
            var driverService = GetDefaultService<InternetExplorerDriverService>(driverFolder);
            IWebDriver driver;
            try
            {
                driver = new InternetExplorerDriver(driverService, GetInternetExplorerOptions(), _timeout);
            }
            catch
            {
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        private IWebDriver CreateLocalOperaDriver()
        {
            var driverService = GetDefaultService<OperaDriverService>();
            IWebDriver driver;
            try
            {
                driver = new OperaDriver(driverService, GetOperaOptions(), _timeout);
            }
            catch
            {
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        private IWebDriver CreateLocalSafariDriver()
        {
            var driverService = GetDefaultService<SafariDriverService>();
            IWebDriver driver;
            try
            {
                driver = new SafariDriver(driverService, GetSafariOptions(), _timeout);
            }
            catch
            {
                driverService.Dispose();
                throw;
            }
            return driver;
        }

        public IWebDriver CreateRemoteDriver(string browserName, string baseAddress, Dictionary<string, object> capabilities)
        {
            //var driverOptions = GetRemoteOptions(browserName, capabilities);
            var desiredCapabilities = GetDesiredCapabilities(browserName, capabilities);
            var uri = new Uri(baseAddress + "/wd/hub");
            var result = new RemoteWebDriver(uri, desiredCapabilities, _timeout);
            return result;
        }

        private ChromeOptions GetChromeOptions(bool headless)
        {
            var options = new ChromeOptions {Proxy = _proxy};

            // Get rid of the warning bar "You are using an unsupported command-line flag" 
            options.AddArgument("test-type");
            options.AddArgument("enable-automation");
            if (headless)
            {
                // see https://bugs.chromium.org/p/chromium/issues/detail?id=737678 for why disable-gpu
                options.AddArguments("headless", "disable-gpu");
            }

            return options;
        }

        // We cannot stop using the deprecated DesiredCapabilities at this point (hence the pragma disable 618) because
        // we want the flexibility to specify non-predefined capabilities to enable external services as BrowserStack.
        // Using the AddAdditionalCapabilities adds to the options rather than a separate capability.
#pragma warning disable 618
        private DesiredCapabilities DesiredCapabilitiesFor(string browserName)
        {
            var standardBrowserName = StandardizeBrowserName(browserName);
            if (_remoteOptions.ContainsKey(standardBrowserName))
            {
                var driverOptions = _remoteOptions[standardBrowserName]();
                // We need to get a bit clever here. Different browsers return different underlying types, and ICapabilities has no way to iterate through the properties.
                // So first we try DesiredCapabilities, and if that doesn't work we take the ReadOnlyDesiredCapabilities instead, and add all capabilities to a new DesiredCapabilities.
                if (driverOptions?.ToCapabilities() is DesiredCapabilities desiredCapabilities) return desiredCapabilities;
                var cap = (driverOptions?.ToCapabilities() as ReadOnlyDesiredCapabilities)?.ToDictionary();
                if (cap == null) return null;
                desiredCapabilities = new DesiredCapabilities();
                foreach (var entry in cap.Keys)
                {
                    desiredCapabilities.SetCapability(entry, cap[entry]);
                }
                return desiredCapabilities;
            }
            // If we didn't find an entry in the options, revert to 
            var platform = new Platform(PlatformType.Any);
            return new DesiredCapabilities(browserName, string.Empty, platform);

#pragma warning restore 618
        }

        internal ICapabilities GetDesiredCapabilities(string browserName, Dictionary<string, object> capabilities)
        {
            var desiredCapabilities = DesiredCapabilitiesFor(browserName);

            foreach (var entry in capabilities.Keys)
            {
#pragma warning disable 618
                desiredCapabilities.SetCapability(entry, capabilities[entry]);
#pragma warning restore 618
            }
            return desiredCapabilities;
        }

        private EdgeOptions GetEdgeOptions()
        {
            if (_proxy.Kind != ProxyKind.System)
            {
                throw new StopTestException("Edge browser can only work with system proxy");
            }

            var options = new EdgeOptions {PageLoadStrategy = PageLoadStrategy.Eager};
            return options;
        }

        private FirefoxOptions GetFirefoxOptions(bool headless)
        {
            var options = new FirefoxOptions {Proxy = _proxy};

            if (headless)
            {
                options.AddArgument("--headless");
            }

            // As of FF49 Firefox can trust Root authorities in the windows certificate store. 
            // For more details see https://bugzilla.mozilla.org/show_bug.cgi?id=1265113
            // Unfortunately, the feature is off by default. This setting enables it.
            // For more details see https://bugzilla.mozilla.org/show_bug.cgi?id=1314010
            options.SetPreference("security.enterprise_roots.enabled", true);

            //var wiaDomain = ConfigurationManager.AppSettings.Get("Firefox.IntegratedAuthenticationDomain");

            if (!string.IsNullOrEmpty(IntegratedAuthenticationDomain))
            {
                options.SetPreference(@"network.automatic-ntlm-auth.trusted-uris", IntegratedAuthenticationDomain);
                options.SetPreference(@"network.negotiate-auth.delegation-uris", IntegratedAuthenticationDomain);
                options.SetPreference(@"network.negotiate-auth.trusted-uris", IntegratedAuthenticationDomain);
            }

            // workaround for bug, see: https://github.com/mozilla/geckodriver/issues/1068
            options.SetPreference(@"security.sandbox.content.level", 5);

            // enable Silverlight
            options.SetPreference(@"plugin.state.npctrl", 2);
            options.AcceptInsecureCertificates = true;
            return options;
        }

        private InternetExplorerOptions GetInternetExplorerOptions()
        {
            var options = new InternetExplorerOptions
            {
                Proxy = _proxy,
                IntroduceInstabilityByIgnoringProtectedModeSettings = InternetExplorerIgnoreProtectedModeSetting()
            };
            return options;
        }

        private OperaOptions GetOperaOptions() => new OperaOptions {Proxy = _proxy};

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local", Justification = "Consistency with other GetOptions methods")]
        private SafariOptions GetSafariOptions()
        {
            // didn't find a way to enable proxies or integrated authentication on Safari
            var options = new SafariOptions();
            options.AddAdditionalCapability("safari.options", "skipExtensionInstallation");
            return options;
        }

        // does not work right - AddAdditionalCapabilities adds to the goog:ChromeOptions structure rather
        // than creating its own capability. For now we revert to using the deprecated DesiredCapabilities
        /* internal DriverOptions GetRemoteOptions(string browserName, Dictionary<string, object> capabilities)
        {
            var standardBrowserName = StandardizeBrowserName(browserName);
            if (!_remoteOptions.ContainsKey(standardBrowserName)) return null;

            var driverOptions = _remoteOptions[standardBrowserName]();
            if (capabilities != null)
            {
                foreach (var entry in capabilities.Keys)
                {
                    driverOptions.AddAdditionalCapability(entry, capabilities[entry]);
                }
            }
            return driverOptions;
        } */
    }
}
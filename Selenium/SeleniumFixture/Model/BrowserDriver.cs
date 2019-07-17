﻿// Copyright 2015-2019 Rik Essenius
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static System.Globalization.CultureInfo;
using System.Reflection;
using ImageHandler;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SeleniumFixture.Model
{
    /// <summary>
    ///     Container for the WebDrivers so they can be shared by different fixtures.
    ///     Also includes the remote and local browser configuration
    ///     Supports IE, Chrome and Firefox. For FireFox, enables integrated authentication
    /// </summary>
    internal static class BrowserDriver
    {
        private static int _idCounter = 1;
        private static Proxy _proxy = new Proxy {Kind = ProxyKind.System};
        private static TimeSpan _timeout = TimeSpan.FromSeconds(60);
        private static readonly Dictionary<string, IWebDriver> Drivers = new Dictionary<string, IWebDriver>();

        public static double CommandTimeoutSeconds
        {
            get => _timeout.TotalSeconds;
            set => _timeout = TimeSpan.FromSeconds(value);
        }

        public static IWebDriver Current { get; set; }

        public static string CurrentId { get; private set; }

        public static int DriverCount => Drivers.Count;

        private static string UniqueId => _idCounter++.ToString(InvariantCulture);

        private static string AddDriver(IWebDriver newDriver)
        {
            var id = UniqueId;
            Drivers.Add(id, newDriver);
            return id;
        }

        internal static void CloseAllDrivers()
        {
            foreach (var webDriver in Drivers) webDriver.Value?.Quit();

            Drivers.Clear();
            CurrentId = string.Empty;
            Current = null;
        }

        private static IWebDriver GetDriver(string driverId) => !Drivers.ContainsKey(driverId) ? null : Drivers[driverId];

        // this works for all drivers that implement RemoteWebDriver, which is the case for all drivers we use
        private static bool HasQuit(IWebDriver driver) => ((RemoteWebDriver)driver).SessionId == null;

        public static string NewDriver(string browserName)
        {
            try
            {
                Current = new BrowserDriverFactory(_proxy, _timeout).CreateLocalDriver(browserName);
                CurrentId = AddDriver(Current);
                return CurrentId;
            }
            catch (Exception exception) when (exception is WebDriverException || exception is Win32Exception ||
                                              exception is InvalidOperationException || exception is TargetInvocationException)
            {
                CloseAllDrivers();
                throw new StopTestException("Could not start browser: " + browserName, exception);
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Desired behavior. Returning exception to FitSharp")]
        public static string NewRemoteDriver(string browserName, string baseAddress, Dictionary<string, object> capabilities)
        {
            try
            {
                Current = new BrowserDriverFactory(_proxy, _timeout).CreateRemoteDriver(browserName, baseAddress, capabilities);
            }
            catch (Exception e)
            {
                CloseAllDrivers();
                var browser = "browser '" + browserName + "'";
                var address = "Selenium server '" + baseAddress + "'";
                var message = "Can't run " + browser + " on " + address;
                throw new StopTestException(message, e);
            }
            ((RemoteWebDriver)Current).FileDetector = new LocalFileDetector();

            CurrentId = AddDriver(Current);
            return CurrentId;
        }

        public static bool RemoveDriver(string driverId)
        {
            var driver = GetDriver(driverId);
            if (driver == null) return false;
            if (!HasQuit(driver)) driver.Quit();
            var isRemoved = Drivers.Remove(driverId);
            if (!isRemoved || driverId != CurrentId) return isRemoved;
            Current = null;
            CurrentId = string.Empty;
            return true;
        }

        public static bool SetCurrent(string driverId)
        {
            var driver = GetDriver(driverId);
            if (driver == null) return false;
            CurrentId = driverId;
            Current = driver;
            return true;
        }

        public static bool SetProxyType(string proxyType)
        {
            if (!Enum.TryParse(proxyType, true, out ProxyKind proxyKind)) throw new ArgumentException($"Unrecognized proxy type '{proxyType}'");
            // can't update proxy in all cases, so create a new one.
            _proxy = new Proxy {Kind = proxyKind};
            return proxyKind != ProxyKind.Unspecified;
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases", Justification = "no need to process the others")]
        public static bool SetProxyValue(string proxyType, string proxyValue)
        {
            if (!SetProxyType(proxyType)) return false;
            if (string.IsNullOrEmpty(proxyValue)) throw new ArgumentException($"No value specified for proxy type '{proxyType}'");
            switch (_proxy.Kind)
            {
                case ProxyKind.Manual:
                    _proxy.HttpProxy = proxyValue;
                    _proxy.SslProxy = proxyValue;
                    return true;
                case ProxyKind.ProxyAutoConfigure:
                    _proxy.ProxyAutoConfigUrl = proxyValue;
                    return true;
                default:
                    throw new ArgumentException($"No value expected for proxy type '{proxyType}'");
            }
        }

        public static Snapshot TakeScreenshot()
        {
            var screenshot = ((ITakesScreenshot)Current).GetScreenshot();
            return new Snapshot(screenshot.AsByteArray);
        }
    }
}
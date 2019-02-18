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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private static string AddDriver(IWebDriver newDriver)
        {
            var id = UniqueId;
            Drivers.Add(id, newDriver);
            return id;
        }

        internal static void CloseAllDrivers()
        {
            foreach (var webDriver in Drivers)
            {
                webDriver.Value?.Quit();
            }

            Drivers.Clear();
            CurrentId = string.Empty;
            Current = null;
        }

        public static double CommandTimeoutSeconds
        {
            get => _timeout.TotalSeconds;
            set => _timeout = TimeSpan.FromSeconds(value);
        }

        private static string CreateArchiveFileName(string existingFileName, string timestamp) => Path.Combine(
            Path.GetDirectoryName(existingFileName) ?? string.Empty,
            Path.GetFileNameWithoutExtension(existingFileName) + "_" + timestamp +
            Path.GetExtension(existingFileName));

        public static IWebDriver Current { get; set; }

        public static string CurrentId { get; private set; }

        public static int DriverCount => Drivers.Count;

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
            if (!HasQuit(driver))
            {
                driver.Quit();
            }
            var isRemoved = Drivers.Remove(driverId);
            if (!isRemoved || driverId != CurrentId) return isRemoved;
            Current = null;
            CurrentId = string.Empty;
            return true;
        }

        private static void RenameExistingFile(string existingFileName)
        {
            if (!File.Exists(existingFileName)) return;
            var timestamp = File.GetLastWriteTime(existingFileName)
                .ToString(@"yyyyMMddHHmmssffffff", CultureInfo.InvariantCulture);
            File.Move(existingFileName, CreateArchiveFileName(existingFileName, timestamp));
        }

        internal static string ScreenshotPath(string fileName, string defaultPath)
        {
            defaultPath = string.IsNullOrEmpty(defaultPath) ? "." : defaultPath;
            var file = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(file) || file == ".")
            {
                fileName = Path.GetRandomFileName();
            }

            if (!fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) &&
                !fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase))
            {
                fileName += ".jpg";
            }

            return Path.IsPathRooted(fileName) ? fileName : Path.Combine(defaultPath, fileName);
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
            const string unspecified = "Unspecified";
            // try and convert casing of the desired proxy to the proper CamelCase format
            var valueToUse =
                Enum.GetNames(typeof(ProxyKind))
                    .FirstOrDefault(enumName => enumName.Equals(proxyType, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(valueToUse)) valueToUse = unspecified;

            // now we know we have a valid value. Set the corresponding proxy type.
            var conversionSucceeded = Enum.TryParse(valueToUse, out ProxyKind proxyTypeId);
            Debug.Assert(conversionSucceeded);
            _proxy = new Proxy {Kind = proxyTypeId};
            return valueToUse != unspecified;
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases", Justification = "no need to process the others")]
        public static void SetProxyValue(string proxyValue)
        {
            switch (_proxy.Kind)
            {
                case ProxyKind.Manual:
                    _proxy.HttpProxy = proxyValue;
                    _proxy.SslProxy = proxyValue;
                    break;
                case ProxyKind.ProxyAutoConfigure:
                    _proxy.ProxyAutoConfigUrl = proxyValue;
                    break;
            }
        }

        public static Snapshot TakeScreenshot()
        {
            var screenshot = ((ITakesScreenshot)Current).GetScreenshot();
            return new Snapshot(screenshot.AsByteArray);
        }

        [Obsolete("Deprecated - use TakeScreenshot without parameter")]
        public static string TakeScreenShot(string fileNameToSaveTo)
        {
            fileNameToSaveTo = ScreenshotPath(fileNameToSaveTo,
                Environment.GetEnvironmentVariable("TestOutputDirectory") ?? string.Empty);
            var folder = Path.GetDirectoryName(fileNameToSaveTo);

            Debug.Assert(folder != null, "folder != null");
            if (!Directory.Exists(folder))
            {
                throw new FileNotFoundException("Folder not found: " + folder);
            }

            var screenshot = TakeScreenshot();
            RenameExistingFile(fileNameToSaveTo);
            return screenshot.Save(fileNameToSaveTo);
        }

        private static string UniqueId => _idCounter++.ToString(CultureInfo.InvariantCulture);
    }
}
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixture
{
    // Session handling methods of the Selenium script table fixture for FitNesse

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification =
        "Can't change the type name - would be a breaking change")]
    public sealed partial class Selenium
    {
        private const int DefaultTimeoutInSeconds = 3;

        private BrowserStorage _browserStorage;
        private ProtectedMode _protectedMode;

        private BrowserStorage BrowserStorage
        {
            get
            {
                if (_browserStorage == null) UseWebStorage(StorageType.Local);
                return _browserStorage;
            }
        }

        /// <summary>Command Timeout value in seconds (i.e. the one specified in the driver constructor). Only works for local drivers</summary>
        public static double CommandTimeoutSeconds
        {
            get => BrowserDriverContainer.CommandTimeoutSeconds;
            set => BrowserDriverContainer.CommandTimeoutSeconds = value;
        }

        /// <returns>the current driver object</returns>
        public IWebDriver Driver { get; private set; }

        /// <returns>the id of the current driver</returns>
        public string DriverId { get; private set; }

        /// <summary>Set the number of seconds for implicit wait (default 0 = disable)</summary>
        public static double ImplicitWaitSeconds { get; set; }

        /// <summary>Domain where Integrated Authentication is to be used</summary>
        public static string IntegratedAuthenticationDomain
        {
            get => FireFoxDriverCreator.IntegratedAuthenticationDomain;
            set => FireFoxDriverCreator.IntegratedAuthenticationDomain = value;
        }

        [SupportedOSPlatform("windows")]
        private ProtectedMode ProtectedMode => _protectedMode ??= new ProtectedMode(new ZoneListFactory());

        internal double TimeoutInSeconds { get; private set; } = DefaultTimeoutInSeconds;

        /// <summary>Get/set a Web Store. Sets all key-value pairs, but doesn't delete existing content. Clear any existing values beforehand</summary>
        public Dictionary<string, string> WebStorage
        {
            get { return BrowserStorage.KeySet.ToDictionary(key => key, key => BrowserStorage[key]); }
            set
            {
                BrowserStorage.Clear();
                AddToWebStorage(value);
            }
        }

        /// <summary>Add a set of key/value pairs to a web store</summary>
        public void AddToWebStorage(Dictionary<string, string> dictionaryToAdd)
        {
            if (dictionaryToAdd == null) return;
            foreach (var key in dictionaryToAdd.Keys)
            {
                BrowserStorage[key] = dictionaryToAdd[key];
            }
        }

        internal bool AreAllProtectedModes(bool mode) => !OperatingSystem.IsWindows() || ProtectedMode.AllAre(mode);

        /// <summary>Clear a web store (local or session)</summary>
        public bool ClearWebStorage() => BrowserStorage.Clear();

        /// <summary>Closes this browser session</summary>
        public bool Close()
        {
            if (Driver == null) return false;
            BrowserDriverContainer.RemoveDriver(DriverId);
            Driver = null;
            DriverId = string.Empty;
            return true;
        }

        /// <summary>Return the input. Useful to set symbols</summary>
        /// <remarks>Exists in CommonFunctions too, but didn't want to take a dependency on that just for this</remarks>
        public static object Echo(object input) => input;

        /// <summary>Execute JavaScript asynchronously (in the browser)</summary>
        public object ExecuteAsyncScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteAsyncScript(script);
        }

        /// <summary>Execute JavaScript (in the browser)</summary>
        public object ExecuteScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script);
        }

        /// <summary>
        ///     Execute JavaScript using parameters. If a parameter has a locator format (with colon) then it is substituted by the
        ///     element
        /// </summary>
        /// <remarks>You can refer to them via arguments[0-i] in the script</remarks>
        public object ExecuteScriptWithParameters(string script, Collection<string> args)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            var argsToUse = new List<object>();
            foreach (var locator in args ?? new Collection<string>())
            {
                if (locator.Contains(SearchParser.Delimiter))
                {
                    argsToUse.Add(FindElement(locator));
                }
                else
                {
                    argsToUse.Add(locator);
                }
            }
            return scriptExecutor.ExecuteScript(script, argsToUse.ToArray());
        }

        /// <summary>Execute JavaScript using parameters. No substitution of elements is attempted</summary>
        /// <remarks>You can refer to them via arguments[0-i] in the script</remarks>
        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global",
            Justification = "Would not be visible to FitSharp")]
        public object ExecuteScriptWithPlainParameters(string script, Collection<object> args)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script, args.ToArray());
        }

        /// <summary>Get a value from a web store (local or session)</summary>
        public string GetFromWebStorage(string key) => BrowserStorage[BrowserStorage.FindFirstKeyLike(key)];

        /// <summary>Find the first item matching a glob pattern (with *?)</summary>
        public string GetKeyLikeFromWebStorage(string key) => BrowserStorage.FindFirstKeyLike(key);

        /// <summary>Creates a new browser instance and makes it current. See also <seealso cref="SetBrowser" /></summary>
        /// <param name="browserName">can be Chrome, Chrome Headless, IE, Edge, Firefox, Firefox Headless, Opera</param>
        /// <returns>an ID</returns>
        public string NewBrowser(string browserName)
        {
            DriverId = BrowserDriverContainer.NewDriver(browserName);
            Driver = BrowserDriverContainer.Current;
            _browserStorage = null; // force re-initialization on next call
            return DriverId;
        }

        /// <summary>See <see cref="SetRemoteBrowserAtAddress" /></summary>
        /// <returns>the driver ID </returns>
        public string NewRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, new Dictionary<string, object>());

        /// <summary>See <see cref="SetRemoteBrowserAtAddressWithCapabilities" /></summary>
        /// <returns>the driver ID</returns>
        public string NewRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress,
            Dictionary<string, object> capabilities)
        {
            DriverId = BrowserDriverContainer.NewRemoteDriver(browserName, baseAddress, capabilities);
            Driver = BrowserDriverContainer.Current;
            _browserStorage = null;
            return DriverId;
        }

        /// <summary>Debug function to see what the current protected mode settings are and what they come from</summary>
        [SupportedOSPlatform("windows")]
        internal Collection<Collection<object>> ProtectedModePerZone() => ProtectedMode.State;

        /// <summary>
        ///     Check if Protected Mode for all security zones meet the condition, and throw a StopTestException if not.
        ///     If you use Internet Explorer, it is important that all zones have the same protected mode setting
        /// </summary>
        /// <param name="condition">ON, OFF or EQUAL</param>
        public bool ProtectedModesAre(string condition)
        {
            // if we are not on Windows, we don't care about protected modes (no Internet Explorer) so all is good
            if (!OperatingSystem.IsWindows()) return true;
            var ok = condition?.ToUpperInvariant() switch
            {
                "ON" => ProtectedMode.AllAre(true),
                "OFF" => ProtectedMode.AllAre(false),
                "EQUAL" => ProtectedMode.AllAreSame(),
                _ => throw new ArgumentException($"Unknown condition '{condition}. Valid are On, Off or Equal.")
            };
            if (!ok) throw new StopTestException("Protected modes are not all " + condition.ToLowerInvariant());
            return true;
        }

        /// <summary>Remove an item from web storage (local or session) via its key</summary>
        public bool RemoveFromWebStorage(string key) => _browserStorage.RemoveItem(key);

        /// <summary>Resets the timeout to the default value</summary>
        public void ResetTimeout() => TimeoutInSeconds = DefaultTimeoutInSeconds;

        /// <summary>Sets the browser to be used</summary>
        /// <param name="browserName">can be Chrome, Chrome Headless, IE, Edge, Firefox, Firefox Headless, Opera</param>
        /// <returns>whether the operation succeeded</returns>
        public bool SetBrowser(string browserName) => !string.IsNullOrEmpty(NewBrowser(browserName));

        /// <summary>Set the current browser driver using its ID (returned earlier by NewBrowser)</summary>
        public bool SetDriver(string driverId)
        {
            if (!BrowserDriverContainer.SetCurrent(driverId)) return false;
            DriverId = driverId;
            Driver = BrowserDriverContainer.Current;
            _browserStorage = null;
            StoreWindowHandles();
            return true;
        }

        /// <summary>Set a key/value pair in a web store</summary>
        public void SetInWebStorageTo(string key, string value) => BrowserStorage[key] = value;

        /// <summary>Sets the http and SSL proxy for the test</summary>
        /// <param name="proxyType">Direct, System, or AutoDetect</param>
        public static bool SetProxyType(string proxyType) => BrowserDriverContainer.SetProxyType(proxyType);

        /// <summary>
        ///     Sets the http and SSL proxy for the test. Type is  Manual (hostname.com:8080) or ProxyAutoConfigure
        ///     (http://host/pacfile)
        /// </summary>
        /// <param name="proxyType">Manual or ProxyAutoConfigure</param>
        /// <param name="proxyValue">hostname.com:8080 with Manual, or http://host/pacfile withProxyAutoConfigure</param>
        public static bool SetProxyTypeValue(string proxyType, string proxyValue) =>
            BrowserDriverContainer.SetProxyValue(proxyType, proxyValue);

        /// <summary>Use a remote Selenium server (address including port). Raises a StopTestException if unable to connect</summary>
        /// <returns>true</returns>
        public bool SetRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddress(browserName, baseAddress));

        /// <summary>Use a remote Selenium server (address including port) with a dictionary of desired capabilities</summary>
        /// <remarks>Raises a StopTestException if unable to connect</remarks>
        /// <returns></returns>
        public bool SetRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress,
            Dictionary<string, object> capabilities) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, capabilities));

        /// <summary>Set the default timeout for all wait commands (except page loads). Default value is 3 seconds</summary>
        public void SetTimeoutSeconds(double timeoutInSeconds) => TimeoutInSeconds = timeoutInSeconds;

        /// <summary>Select either Local or Session storage (to work on other Web Storage functions)</summary>
        public void UseWebStorage(StorageType storageType) =>
            _browserStorage = BrowserStorageFactory.Create(Driver, storageType);

        /// <param name="qualifier">SHORT, EXTENDED or empty</param>
        /// <returns>
        ///     the version info of the fixture. SHORT: just the version, EXTENDED: name, version, description, copyright. Anything
        ///     else: name, version
        /// </returns>
        public static string VersionInfo(string qualifier) => ApplicationInfo.VersionInfo(qualifier + string.Empty);

        /// <summary>Checks whether the current version (x.y.z) is at least a minimally required version</summary>
        public static bool VersionIsAtLeast(string version) => ApplicationInfo.VersionIsAtLeast(version);

        /// <summary>Wait a specified number of seconds (can be fractions). Note: this seems to impact iframe context, so use with care</summary>
        public static void WaitSeconds(double seconds)
        {
            if (seconds > 0) Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }
    }
}

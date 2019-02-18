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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using SeleniumFixture.Model;

// Justification: FitNesse entry point

namespace SeleniumFixture
{
    /// <summary>
    ///     Session handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Used by FitSharp")]
    public sealed partial class Selenium
    {
        private const int DefaultTimeoutInSeconds = 3;

        /// <summary>
        ///     The number of seconds for the command timeout (i.e. the one specified in the driver constructor)
        ///     Only works for local drivers.
        /// </summary>
        public static double CommandTimeoutSeconds
        {
            get => BrowserDriver.CommandTimeoutSeconds;
            set => BrowserDriver.CommandTimeoutSeconds = value;
        }

        /// <summary>
        ///     The number of seconds for implicit wait (0 = disable)
        /// </summary>
        public static double ImplicitWaitSeconds { get; set; } = 0;

        /// <summary>
        ///     Configure domain where integrated authentication is to be used
        /// </summary>
        public static string IntegratedAuthenticationDomain
        {
            get => BrowserDriverFactory.IntegratedAuthenticationDomain;
            set => BrowserDriverFactory.IntegratedAuthenticationDomain = value;
        }

        /// <summary>
        ///     Set the proxy type. Default is System. If set to Manual or ProxyAutoConfigure, use SetProxyTypeValue instead.
        ///     Only works before setting a browser.
        /// </summary>
        /// <param name="proxyType">string representation of proxy type: Manual, Direct, System, ProxyAutoConfigure, AutoDetect</param>
        /// <returns>whether or not the operation succeeded</returns>
        public static bool SetProxyType(string proxyType) => BrowserDriver.SetProxyType(proxyType);

        /// <summary>
        ///     Set proxy type and value. Use for types Manual and ProxyAutoConfigure.
        ///     Can also be used for the other types, but then the value parameter is ignored.
        ///     Only works before setting a browser.
        /// </summary>
        /// <param name="proxyType">string representation of proxy type: Manual or ProxyAutoConfigure</param>
        /// <param name="proxyValue">example for Manual: hostname.com:8080; for ProxyAutoConfigure: http://hostname.com/pacfile </param>
        /// <returns>whether or not the operation succeeded</returns>
        public static bool SetProxyTypeValue(string proxyType, string proxyValue)
        {
            if (!BrowserDriver.SetProxyType(proxyType)) return false;
            BrowserDriver.SetProxyValue(proxyValue);
            return true;
        }

        /// <summary>
        ///     Return the version info of the fixture
        /// </summary>
        /// <param name="qualifier">
        ///     SHORT: just the version, EXTENDED: name, version, description, copyright. Anything else: name,
        ///     version
        /// </param>
        /// <returns></returns>
        public static string VersionInfo(string qualifier) => ApplicationInfo.VersionInfo(qualifier);

        /// <summary>
        ///     Wait a specified number of seconds (can be fractions)
        /// </summary>
        /// <param name="seconds">the time to wait</param>
        public static void WaitSeconds(double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            //Note: this seems to impact the iframe context.....
        }

        private BrowserStorage _browserStorage;
        private ProtectedMode _protectedMode;

        private BrowserStorage BrowserStorage
        {
            get
            {
                if (_browserStorage == null)
                {
                    UseWebStorage(StorageType.Local);
                }

                return _browserStorage;
            }
        }

        /// <summary>
        ///     Returns the current web driver
        /// </summary>
        public IWebDriver Driver { get; private set; }

        /// <summary>
        ///     Return the ID of the current web driver
        /// </summary>
        public string DriverId { get; private set; }

        private ProtectedMode ProtectedMode => _protectedMode ?? (_protectedMode = new ProtectedMode(new ZoneListFactory()));

        //for testing
        internal double TimeoutInSeconds { get; private set; } = DefaultTimeoutInSeconds;

        /// <summary>
        ///     Get or set a dictionary of (local/session) storage items
        ///     Set sets all the key-value pairs in the input, but doesn't delete existing content
        /// </summary>
        /// <returns>a dictionary containing the local storage item values</returns>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "API for FitNesse")]
        public Dictionary<string, string> WebStorage
        {
            get { return BrowserStorage.KeySet.ToDictionary(key => key, key => BrowserStorage[key]); }
            set
            {
                BrowserStorage.Clear();
                AddToWebStorage(value);
            }
        }

        /// <summary>
        ///     Add key/value pairs to web store (local or session)
        /// </summary>
        /// <param name="dictionaryToAdd">key/value pairs to be added</param>
        public void AddToWebStorage(Dictionary<string, string> dictionaryToAdd)
        {
            if (dictionaryToAdd == null) return;
            foreach (var key in dictionaryToAdd.Keys)
            {
                BrowserStorage[key] = dictionaryToAdd[key];
            }
        }

        // for testing only
        internal bool AreAllProtectedModes(bool mode) => ProtectedMode.AllAre(mode);

        /// <summary>
        ///     Clear a web storage repository (local or session)
        /// </summary>
        /// <returns>whether or not it succeeded (fails if no storage functionality)</returns>
        public bool ClearWebStorage() => BrowserStorage.Clear();

        /// <summary>
        ///     Closes this browser session.
        /// </summary>
        public bool Close()
        {
            if (Driver == null) return false;

            //Driver.Quit();
            BrowserDriver.RemoveDriver(DriverId);
            Driver = null;
            DriverId = string.Empty;
            return true;
        }

        /// <summary>
        ///     Execute an asynchronous piece of JavaScript
        /// </summary>
        /// <param name="script">the JavaScript to execute</param>
        /// <returns>the result of the script</returns>
        public object ExecuteAsyncScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteAsyncScript(script);
        }

        /// <summary>
        ///     Execute a JavaScript
        /// </summary>
        /// <param name="script">the JavaScript to execute</param>
        /// <returns>the result of the script</returns>
        public object ExecuteScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script);
        }

        /// <summary>
        ///     Execute a JavaScript with element parameters
        /// </summary>
        /// <param name="script">the JavaScript to execute</param>
        /// <param name="args">
        ///     A list of element locators with explicit search method.
        ///     If a parameter value contains the delimiter (colon), it is converted to an element
        ///     If a parameter does not contain a colon, it is used as-is.
        ///     You can refer to them via arguments[0-i] in the script
        /// </param>
        /// <returns>the return value of the script</returns>
        public object ExecuteScriptWithParameters(string script, Collection<string> args)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            var argsToUse = new List<object>();
            foreach (var locator in args ?? new Collection<string>())
            {
                if (locator.Contains(SearchParser.Delimiter))
                {
                    argsToUse.Add(Driver.FindElement(new SearchParser(locator).By));
                }
                else
                {
                    argsToUse.Add(locator);
                }
            }

            return scriptExecutor.ExecuteScript(script, argsToUse.ToArray());
        }

        /// <summary>
        ///     Execute a JavaScript with (simple) parameters
        /// </summary>
        /// <param name="script">the JavaScript to execute</param>
        /// <param name="args">A list of arguments. You can refer to them via arguments[0-i] in the script</param>
        /// <returns>the result of the script</returns>
        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "FitSharp cannot parse IEnumerables")]
        public object ExecuteScriptWithPlainParameters(string script, Collection<object> args)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script, args.ToArray());
        }

        /// <summary>
        ///     Get an item from web storage
        /// </summary>
        /// <param name="key">The key of the item to be retrieved</param>
        /// <returns>the value of the item, or null if not found</returns>
        public string GetFromWebStorage(string key) => BrowserStorage[BrowserStorage.FindFirstKeyLike(key)];

        /// <summary>
        ///     Find the first item matching a glob pattern (with *?)
        /// </summary>
        /// <param name="key">The pattern of the item to be matched</param>
        /// <returns>the key of the item, or null if not found</returns>
        public string GetKeyLikeFromWebStorage(string key) => BrowserStorage.FindFirstKeyLike(key);

        /// <summary>
        ///     Creates a new browser instance and makes it current
        /// </summary>
        /// <param name="browserName">The browser to be used</param>
        /// <returns>Driver ID</returns>
        public string NewBrowser(string browserName)
        {
            DriverId = BrowserDriver.NewDriver(browserName);
            Driver = BrowserDriver.Current;
            _browserStorage = null; // force re-initialization on next call
            return DriverId;
        }

        /// <summary>
        ///     Just like SetRemoteBrowser, but returns the driver ID instead of a boolean. Uses default application name
        ///     'FitNesSelenium'
        /// </summary>
        /// <param name="browserName">The browser to be used</param>
        /// <param name="baseAddress">the address that the browser driver can be found at (including port)</param>
        /// <returns>Driver ID</returns>
        public string NewRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, new Dictionary<string, object>());

        /// <summary>
        ///     Just like SetRemoteBrowser, but returns the driver ID instead of a boolean
        /// </summary>
        /// <param name="browserName">The browser to be used</param>
        /// <param name="baseAddress">the address that the browser driver can be found at (including port)</param>
        /// <param name="capabilities">Dictionary of capabilities desired</param>
        /// <returns>Driver ID</returns>
        public string NewRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress,
            Dictionary<string, object> capabilities)
        {
            DriverId = BrowserDriver.NewRemoteDriver(browserName, baseAddress, capabilities);
            Driver = BrowserDriver.Current;
            _browserStorage = null;
            return DriverId;
        }

        /// <summary>
        ///     Throw a StopTestException if not all protected modes are false
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StopTestException"></exception>
        public bool ProtectedModeIsOff()
        {
            if (!ProtectedMode.AllAre(false)) throw new StopTestException("Protected mode is not off for all zones");
            return true;
        }

        /// <summary>
        ///     Throw a StopTestException if not all protected modes are true
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StopTestException"></exception>
        public bool ProtectedModeIsOn()
        {
            if (!ProtectedMode.AllAre(true)) throw new StopTestException("Protected mode is not on for all zones");
            return true;
        }

        /// <summary>
        ///     Debug function to see what the current protected mode settings are and where they come from.
        /// </summary>
        /// <returns>List of protected mode settings per zone, each in a list [id, isProtected, where found]</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "FitNesse interface spec")]
        public Collection<Collection<object>> ProtectedModePerZone() => ProtectedMode.State;

        /// <summary>
        ///     Throw a StopTestException if not all protected modes are equal
        /// </summary>
        /// <returns></returns>
        public bool ProtectedModesAreEqual()
        {
            if (!ProtectedMode.AllAreSame()) throw new StopTestException("Protected modes are not all the same");
            return true;
        }

        /// <summary>
        ///     Remove an item from web storage (local or session)
        /// </summary>
        /// <param name="key">Key of item to be removed</param>
        /// <returns>whether or not successful</returns>
        public bool RemoveFromWebStorage(string key) => _browserStorage.RemoveItem(key);

        /// <summary>
        ///     Resets the timeout to the default value
        /// </summary>
        public void ResetTimeout()
        {
            TimeoutInSeconds = DefaultTimeoutInSeconds;
        }

        /// <summary>
        ///     Sets the browser to be used for the web tests
        /// </summary>
        /// <param name="browserName">Name of the browser. Can be IE, Chrome, Firefox</param>
        /// <returns>whether or not the operation succeeded</returns>
        public bool SetBrowser(string browserName) => !string.IsNullOrEmpty(NewBrowser(browserName));

        /// <summary>
        ///     Set the current browser driver
        /// </summary>
        /// <param name="driverId">The driver ID (returned earlier in the NewBrowser command)</param>
        /// <returns>whether or not the operation succeeded</returns>
        public bool SetDriver(string driverId)
        {
            if (!BrowserDriver.SetCurrent(driverId)) return false;
            DriverId = driverId;
            Driver = BrowserDriver.Current;
            _browserStorage = null;
            StoreWindowHandles();
            return true;
        }

        /// <summary>
        ///     Set the value of a (local/session) storage item
        /// </summary>
        /// <param name="key">The key of the item to be set</param>
        /// <param name="value">the new value of the item</param>
        /// <returns>true if successful, false if not</returns>
        public void SetInWebStorageTo(string key, string value)
        {
            BrowserStorage[key] = value;
        }

        /// <summary>
        ///     Use Remote driver for browser on a certain address(including port)
        /// </summary>
        /// <param name="browserName">The browser to be used</param>
        /// <param name="baseAddress">the address that the browser driver can be found at (including port)</param>
        /// <returns>whether or not the operation succeeded</returns>
        public bool SetRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddress(browserName, baseAddress));

        /// <summary>
        ///     Use Remote driver for browser on a certain address (including port) and specify desired capabilities.
        /// </summary>
        /// <param name="browserName"></param>
        /// <param name="baseAddress"></param>
        /// <param name="capabilities"></param>
        /// <returns></returns>
        public bool SetRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress,
            Dictionary<string, object> capabilities) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, capabilities));

        /// <summary>
        ///     Set the default timeout for all Wait commands (except page loads)
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout value in seconds</param>
        public void SetTimeoutSeconds(double timeoutInSeconds)
        {
            TimeoutInSeconds = timeoutInSeconds;
            // Made all waits explicit so no need for implicit waits, and better not mix the two
            //browserDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(timeoutInSeconds));
        }

        /// <summary>
        ///     Set which storage type (Local or Session) the storage functions use.
        /// </summary>
        /// <param name="storageType">The storage type to use</param>
        public void UseWebStorage(StorageType storageType)
        {
            _browserStorage = BrowserStorageFactory.Create(Driver, storageType);
        }
    }
}
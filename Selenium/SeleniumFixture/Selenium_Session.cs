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

using OpenQA.Selenium;
using SeleniumFixture.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace SeleniumFixture
{
    /// <summary>
    ///     Session handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Used by FitSharp"),
     SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification =
         "Can't change the type name - would be a breaking change")]
    public sealed partial class Selenium
    {
        private const string BrowserChoices = "Browser names can be Chrome, Chrome Headless, IE, Edge, Firefox, Firefox Headless, Opera";
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

        [Documentation("Command Timeout value in seconds (i.e. the one specified in the driver constructor). Only works for local drivers")]
        public static double CommandTimeoutSeconds
        {
            get => BrowserDriver.CommandTimeoutSeconds;
            set => BrowserDriver.CommandTimeoutSeconds = value;
        }

        [Documentation("Returns the current driver object")]
        public IWebDriver Driver { get; private set; }

        [Documentation("Returns the id of the current driver")]
        public string DriverId { get; private set; }

        [Documentation("Set the number of seconds for implicit wait (0 = disable)")]
        public static double ImplicitWaitSeconds { get; set; } = 0;

        [Documentation("Domain where Integrated Authentication is to be used")]
        public static string IntegratedAuthenticationDomain
        {
            get => BrowserDriverFactory.IntegratedAuthenticationDomain;
            set => BrowserDriverFactory.IntegratedAuthenticationDomain = value;
        }

        private ProtectedMode ProtectedMode => _protectedMode ?? (_protectedMode = new ProtectedMode(new ZoneListFactory()));

        internal double TimeoutInSeconds { get; private set; } = DefaultTimeoutInSeconds;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "API for FitNesse"),
         Documentation("Get/set a Web Store. Sets all key-value pairs, but doesn't delete existing content. Clear any existing values beforehand")]
        public Dictionary<string, string> WebStorage
        {
            get { return BrowserStorage.KeySet.ToDictionary(key => key, key => BrowserStorage[key]); }
            set
            {
                BrowserStorage.Clear();
                AddToWebStorage(value);
            }
        }

        [Documentation("Add a set of key/value pairs to a web store")]
        public void AddToWebStorage(Dictionary<string, string> dictionaryToAdd)
        {
            if (dictionaryToAdd == null) return;
            foreach (var key in dictionaryToAdd.Keys) BrowserStorage[key] = dictionaryToAdd[key];
        }

        internal bool AreAllProtectedModes(bool mode) => ProtectedMode.AllAre(mode);

        [Documentation("Clear a web store (local or session)")]
        public bool ClearWebStorage() => BrowserStorage.Clear();

        [Documentation("Closes this browser session")]
        public bool Close()
        {
            if (Driver == null) return false;
            BrowserDriver.RemoveDriver(DriverId);
            Driver = null;
            DriverId = string.Empty;
            return true;
        }

        [Documentation("Execute JavaScript asynchronously (in the browser)")]
        public object ExecuteAsyncScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteAsyncScript(script);
        }

        [Documentation("Execute JavaScript (in the browser)")]
        public object ExecuteScript(string script)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script);
        }

        [Documentation("Execute JavaScript using parameters. If a parameter has a locator format (with colon), " +
                       "then it is substituted by the element. You can refer to them via arguments[0-i] in the script")]
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

        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "FitSharp cannot parse IEnumerables"), Documentation(
             "Execute JavaScript using parameters. No substitution of elements is attempted. " +
             "You can refer to them via arguments[0-i] in the script")]
        public object ExecuteScriptWithPlainParameters(string script, Collection<object> args)
        {
            var scriptExecutor = (IJavaScriptExecutor)Driver;
            return scriptExecutor.ExecuteScript(script, args.ToArray());
        }

        [Documentation("Get a value from a web store (local or session)")]
        public string GetFromWebStorage(string key) => BrowserStorage[BrowserStorage.FindFirstKeyLike(key)];

        [Documentation("Find the first item matching a glob pattern (with *?)")]
        public string GetKeyLikeFromWebStorage(string key) => BrowserStorage.FindFirstKeyLike(key);

        [Documentation("Creates a new browser instance and makes it current. Returns an ID. " + BrowserChoices)]
        public string NewBrowser(string browserName)
        {
            DriverId = BrowserDriver.NewDriver(browserName);
            Driver = BrowserDriver.Current;
            _browserStorage = null; // force re-initialization on next call
            return DriverId;
        }

        [Documentation("Specifies that the test will be run at a remote Selenium server. Just like SetRemoteBrowserAtAddress, " +
                       "but returns the driver ID instead of a boolean." + BrowserChoices)]
        public string NewRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, new Dictionary<string, object>());

        [Documentation("Just like SetRemoteBrowserAtAddressWithCapabilities, but returns the driver ID instead of a boolean." + BrowserChoices)]
        public string NewRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress, Dictionary<string, object> capabilities)
        {
            DriverId = BrowserDriver.NewRemoteDriver(browserName, baseAddress, capabilities);
            Driver = BrowserDriver.Current;
            _browserStorage = null;
            return DriverId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Need lower case")]
        [Documentation("Check if Protected Mode for all security zones meet the condition (ON/OFF/EQUAL), and throw a StopTestException if not. " +
                       "If you use Internet Explorer, it is important that all zones have the same protected mode setting")]
        public bool ProtectedModesAre(string condition)
        {
            bool ok;
            switch (condition.ToUpperInvariant())
            {
                case "ON": ok = ProtectedMode.AllAre(true);
                    break;
                case "OFF": ok = ProtectedMode.AllAre(false);
                    break;
                case "EQUAL": ok = ProtectedMode.AllAreSame();
                    break;
                default:
                    throw new ArgumentException($"Unknown condition '{condition}. Valid are On, Off or Equal.");
            }
            if (!ok) throw new StopTestException("Protected modes are not all " + condition.ToLowerInvariant());
            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "FitNesse interface spec"),
         Documentation("Debug function to see what the current protected mode settings are and what they come from")]
        internal Collection<Collection<object>> ProtectedModePerZone() => ProtectedMode.State;

        [Documentation("Remove an item from web storage (local or session) via its key")]
        public bool RemoveFromWebStorage(string key) => _browserStorage.RemoveItem(key);

        [Documentation("Resets the timeout to the default value")]
        public void ResetTimeout() => TimeoutInSeconds = DefaultTimeoutInSeconds;

        [Documentation("Sets the browser to be used. " + BrowserChoices)]
        public bool SetBrowser(string browserName) => !string.IsNullOrEmpty(NewBrowser(browserName));

        [Documentation("Set the current browser driver using its ID (returned earlier by NewBrowser)")]
        public bool SetDriver(string driverId)
        {
            if (!BrowserDriver.SetCurrent(driverId)) return false;
            DriverId = driverId;
            Driver = BrowserDriver.Current;
            _browserStorage = null;
            StoreWindowHandles();
            return true;
        }

        [Documentation("Set a key/value pair in a web store")]
        public void SetInWebStorageTo(string key, string value) => BrowserStorage[key] = value;

        [Documentation("Sets the http and SSL proxy for the test. Type can be Direct, System, AutoDetect.")]
        public static bool SetProxyType(string proxyType) => BrowserDriver.SetProxyType(proxyType);

        [Documentation("Sets the http and SSL proxy for the test. Type is  Manual (hostname.com:8080) or ProxyAutoConfigure (http://host/pacfile).")]
        public static bool SetProxyTypeValue(string proxyType, string proxyValue) => BrowserDriver.SetProxyValue(proxyType, proxyValue);

        [Documentation("Use a remote Selenium server (address including port). " +
                       "Raises a StopTestException if unable to connect. " + BrowserChoices)]
        public bool SetRemoteBrowserAtAddress(string browserName, string baseAddress) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddress(browserName, baseAddress));

        [Documentation("Use a remote Selenium server (address including port) with a dictionary of desired capabilities. " +
                       "Raises a StopTestException if unable to connect. " + BrowserChoices)]
        public bool SetRemoteBrowserAtAddressWithCapabilities(string browserName, string baseAddress,
            Dictionary<string, object> capabilities) =>
            !string.IsNullOrEmpty(NewRemoteBrowserAtAddressWithCapabilities(browserName, baseAddress, capabilities));

        [Documentation("Set the default timeout for all wait commands (except page loads). Default value is 3 seconds")]
        public void SetTimeoutSeconds(double timeoutInSeconds) => TimeoutInSeconds = timeoutInSeconds;

        [Documentation("Select either Local or Session storage (to work on other Web Storage functions)")]
        public void UseWebStorage(StorageType storageType) => _browserStorage = BrowserStorageFactory.Create(Driver, storageType);

        [Documentation("Returns the version info of the fixture. " +
                       "SHORT: just the version, EXTENDED: name, version, description, copyright. Anything else: name, version")]
        public static string VersionInfo(string qualifier) => ApplicationInfo.VersionInfo(qualifier);

        [Documentation("Wait a specified number of seconds (can be fractions). Note: this seems to impact iframe context, so use with care.")]
        public static void WaitSeconds(double seconds)
        {
            if (seconds > 0) Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }
    }
}
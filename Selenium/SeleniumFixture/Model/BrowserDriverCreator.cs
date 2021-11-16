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
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal abstract class BrowserDriverCreator
    {
        protected readonly Proxy Proxy;
        protected readonly TimeSpan Timeout;

        protected BrowserDriverCreator(Proxy proxy, TimeSpan timeout)
        {
            Proxy = proxy;
            Timeout = timeout;
        }

        public abstract string Name { get; }

        protected static Uri BaseUri(string baseAddress) => new(baseAddress);

        // We cannot stop using the deprecated DesiredCapabilities at this point (hence the pragma disable 618) because
        // we want the flexibility to specify non-predefined capabilities to enable external services as BrowserStack.
        // Using the AddAdditionalCapabilities adds to the options rather than a separate capability.
#pragma warning disable 618
        private DesiredCapabilities DesiredCapabilities() => DesiredCapabilities(Options());
        /*{
            var options = Options();
            // We need to get a bit clever here. Different browsers return different underlying types, and ICapabilities has no way to iterate
            // through the properties. So first we try DesiredCapabilities, and if that doesn't work we take the ReadOnlyDesiredCapabilities
            // instead, and add all capabilities to a new DesiredCapabilities.
            if (options?.ToCapabilities() is DesiredCapabilities desiredCapabilities) return desiredCapabilities;
            var cap = (options?.ToCapabilities() as ReadOnlyDesiredCapabilities)?.ToDictionary();
            if (cap == null) return null;
            desiredCapabilities = new DesiredCapabilities();
            foreach (var entry in cap.Keys)
            {
                desiredCapabilities.SetCapability(entry, cap[entry]);
            }
            return desiredCapabilities;
        }*/

        internal static DesiredCapabilities DesiredCapabilities(DriverOptions options)
        {
            // We need to get a bit clever here. Different browsers return different underlying types, and ICapabilities has no way to iterate
            // through the properties. So first we try DesiredCapabilities, and if that doesn't work we take the ReadOnlyDesiredCapabilities
            // instead, and add all capabilities to a new DesiredCapabilities.
            if (options?.ToCapabilities() is DesiredCapabilities desiredCapabilities) return desiredCapabilities;
            var cap = (options?.ToCapabilities() as ReadOnlyDesiredCapabilities)?.ToDictionary();
            if (cap == null) return null;
            desiredCapabilities = new DesiredCapabilities();
            foreach (var entry in cap.Keys)
            {
                desiredCapabilities.SetCapability(entry, cap[entry]);
            }
            return desiredCapabilities;
#pragma warning restore 618
        }

        internal ICapabilities DesiredCapabilities(Dictionary<string, object> capabilities)
        {
            var desiredCapabilities = DesiredCapabilities();

            foreach (var entry in capabilities.Keys)
            {
#pragma warning disable 618
                desiredCapabilities.SetCapability(entry, capabilities[entry]);
#pragma warning restore 618
            }
            return desiredCapabilities;
        }

        // This is a workaround for the absence of an IDriverService interface, using reflection
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

        // I tried to make these methods smarter (eliminate redundancy) e.g. via generics, but that is not easy with all the hard dependencies
        // that browser drivers have on their services and options. So I decided to live with the redundancy for now.
        // So, all the driver creators currently have their own implementation.
        public abstract IWebDriver LocalDriver(object options);

        public abstract DriverOptions Options();

        public virtual IWebDriver RemoteDriver(string baseAddress, Dictionary<string, object> capabilities)
        {
            var uri = BaseUri(baseAddress);
            var desiredCapabilities = DesiredCapabilities(capabilities);
            var result = new RemoteWebDriver(uri, desiredCapabilities, Timeout);
            return result;
        }

        public virtual IWebDriver RemoteDriver(string baseAddress, DriverOptions options)
        {
            var uri = BaseUri(baseAddress);
            var desiredCapabilities = DesiredCapabilities(options);
            var result = new RemoteWebDriver(uri, desiredCapabilities, Timeout);
            return result;
        }


        // AddAdditionalCapabilities adds to the goog:ChromeOptions (etc.) structure rather than creating its own capability.
        // This is aligned with W3C, but too many services don't adhere to that yet.
        // For now we only use it for Appium, and for remote Selenium we use the (deprecated) DesiredCapabilities
        internal DriverOptions RemoteOptions(Dictionary<string, object> capabilities)
        {
            var options = Options();
            options.AddAdditionalCapabilities(capabilities);
            return options;
        }
    }
}

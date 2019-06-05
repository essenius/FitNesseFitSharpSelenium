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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

namespace SeleniumFixture.Utilities
{
    internal static class WebObjectExtensions
    {
        private static double _lastSetImplicitWaitSeconds;

        public static ReadOnlyCollection<IWebElement> FindElements(this IEnumerable<IWebElement> sourceElements, By by)
        {
            ReadOnlyCollection<IWebElement> returnValue = null;
            foreach (var element in sourceElements)
            {
                returnValue = element.FindElements(by);
                if (returnValue.Count > 0) break;
            }
            return returnValue;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Enable use in extension method")]
        public static double GetImplicitWait(this IWebDriver driver) => _lastSetImplicitWaitSeconds;
        // Currently this only works for firefox: (int)driver.Manage().Timeouts().ImplicitWait.TotalSeconds;

        private static IJavaScriptExecutor GetJavaScriptExecutor(IWebElement element)
        {
            var wrappedElement = element as IWrapsDriver;
            var driver = wrappedElement?.WrappedDriver;
            return driver as IJavaScriptExecutor;
        }

        public static string GetValueBy(this IWebElement element, string method) =>
            method.Equals("value", StringComparison.OrdinalIgnoreCase)
                ? element.GetAttribute("value")
                : element.Text;

        private static bool IsBrowser(this IWebDriver driver, string browserName) =>
            driver is IHasCapabilities driverWithCapabilities &&
            browserName.Equals(driverWithCapabilities.Capabilities.GetCapability("browserName").ToString(), StringComparison.OrdinalIgnoreCase);

        public static bool IsChrome(this IWebDriver driver) => driver.IsBrowser("chrome");

        public static bool IsFirefox(this IWebDriver driver) => driver.IsBrowser("firefox");

        public static bool IsIe(this IWebDriver driver) => driver.IsBrowser("internet explorer");

        public static bool IsEdge(this IWebDriver driver) => driver.IsBrowser("microsoftedge");

        public static bool SetAttribute(this IWebElement element, string attributeName, string value)
        {
            var javascript = GetJavaScriptExecutor(element);
            if (javascript == null) return false;
            javascript.ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);", element, attributeName,
                value);
            return value.Equals(element.GetAttribute(attributeName));
        }

        public static void SetImplicitWait(this IWebDriver driver, double seconds)
        {
            _lastSetImplicitWaitSeconds = seconds;
            Debug.Assert(driver != null, "driver != null");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(seconds);
        }

        public static bool SetInnerHtml(this IWebElement element, string value)
        {
            var javascript = GetJavaScriptExecutor(element);
            if (javascript == null) return false;
            javascript.ExecuteScript("arguments[0].innerHTML = arguments[1];", element, value);
            return value.Equals(element.Text);
        }
    }
}
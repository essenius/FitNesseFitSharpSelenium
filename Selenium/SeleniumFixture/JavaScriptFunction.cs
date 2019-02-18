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

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixture
{
    /// <summary>
    ///     Dynamic table fixture to allow testing of several input parameters for JavaScript functions
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class JavaScriptFunction
    {
        /// <summary>
        ///     Documentation dictionary for use by FixtureExplorer
        /// </summary>
        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Dynamic Decision Table Fixture to test JavaScript functions"},
            {nameof(Get), "Dynamic Decision Table Interface (get column value)"},
            {nameof(Reset), "Dynamic Decision Table Interface (reset row)"},
            {nameof(Set), "Dynamic Decision Table Interface (set column value)"}
        };

        private readonly ArrayList _paramList = new ArrayList();

        /// <summary>
        ///     The function to be called with the already provided set values as parameters
        /// </summary>
        /// <param name="requestedValue">Function to be called</param>
        /// <returns>Return value of the function</returns>
        public object Get(string requestedValue)
        {
            var scriptExecutor = (IJavaScriptExecutor)BrowserDriver.Current;

            var parameters = string.Join(", ", _paramList.ToArray());
            var script = "return " + requestedValue + "(" + parameters + ");";
            Debug.Print("Script: " + script);
            return scriptExecutor.ExecuteScript(script);
        }

        /// <summary>
        ///     Reset the class so it is ready for the next line
        /// </summary>
        /// <exception cref="NoNullAllowedException"></exception>
        public void Reset()
        {
            if (BrowserDriver.Current == null)
            {
                throw new NoNullAllowedException("Set browser first using the Selenium script");
            }

            _paramList.Clear();
        }

        /// <summary>
        ///     Set the parameters for the function to be tested
        /// </summary>
        /// <param name="name">Parameter name (ignored, for test case clarity only)</param>
        /// <param name="value">Parameter value</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name", Justification = "FitSharp spec"),
         SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "FitSharp spec")]
        public void Set(string name, object value)
        {
            var delimiter = value is string ? "'" : string.Empty;
            _paramList.Add(delimiter + value + delimiter);
        }
    }
}
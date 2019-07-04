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
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixture
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp"),
     Documentation("Dynamic Decision Table Fixture to test JavaScript functions")]
    public class JavaScriptFunction
    {
        private readonly ArrayList _paramList = new ArrayList();

        [Documentation("Dynamic Decision Table Interface (get column value)")]
        public object Get(string requestedValue)
        {
            var scriptExecutor = (IJavaScriptExecutor)BrowserDriver.Current;
            var parameters = string.Join(", ", _paramList.ToArray());
            var script = "return " + requestedValue + "(" + parameters + ");";
            return scriptExecutor.ExecuteScript(script);
        }

        [Documentation("Dynamic Decision Table Interface (reset row so it's ready for the next line)")]
        public void Reset()
        {
            if (BrowserDriver.Current == null) throw new NoNullAllowedException("Set browser first using the Selenium script");
            _paramList.Clear();
        }

        [Documentation("Dynamic Decision Table Interface (set column value). Name is for clarity only - ignored")]
        public void Set(string name, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            var delimiter = value is string ? "'" : string.Empty;
            _paramList.Add(delimiter + value + delimiter);
        }
    }
}
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
using OpenQA.Selenium;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture.Model
{
    internal class JavaScriptBrowserStorage : BrowserStorage
    {
        private readonly IJavaScriptExecutor _javaScriptExecutor;
        private readonly string _javaScriptStore;

        public JavaScriptBrowserStorage(IWebDriver browserDriver, StorageType storageType) : base(browserDriver)
        {
            _javaScriptExecutor = (IJavaScriptExecutor)browserDriver;
            _javaScriptStore = storageType == StorageType.Local ? "window.localStorage" : "window.sessionStorage";
        }

        public override IEnumerable<string> KeySet
        {
            get
            {
                var count = Convert.ToInt32(CallViaJavascript("length"), InvariantCulture);
                var result = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    var key = CallViaJavascript(FormattableString.Invariant($"key({i});"));
                    result.Add(key.ToString());
                }

                return result;
            }
        }

        private object CallViaJavascript(string scriptSnippet) =>
            _javaScriptExecutor.ExecuteScript(
                FormattableString.Invariant($"return {_javaScriptStore}.{scriptSnippet};"));

        public override bool Clear()
        {
            CallViaJavascript("clear()");
            return true;
        }

        public override string GetItem(string key) =>
            CallViaJavascript(FormattableString.Invariant($"getItem('{key}')")).ToString();

        public override bool RemoveItem(string key)
        {
            CallViaJavascript(FormattableString.Invariant($"removeItem('{key}')"));
            return true;
        }

        public override void SetItem(string key, string value) =>
            CallViaJavascript(FormattableString.Invariant($"setItem('{key}','{value}')"));
    }
}

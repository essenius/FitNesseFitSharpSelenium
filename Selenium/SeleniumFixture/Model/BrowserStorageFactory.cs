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

using System.Diagnostics;
using OpenQA.Selenium;
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    internal static class BrowserStorageFactory
    {
        public static BrowserStorage Create(IWebDriver browserDriver, StorageType storageType)
        {
            Debug.Assert(browserDriver != null, "browserDriver != null");
            var javaScriptExecutor = (IJavaScriptExecutor)browserDriver;
            var javaScriptSupportsStorage =
                javaScriptExecutor.ExecuteScript("return typeof(Storage) !== 'undefined';").ToBool();
            if (javaScriptSupportsStorage) return new JavaScriptBrowserStorage(browserDriver, storageType);
            return new NoBrowserStorage(browserDriver);
        }
    }
}

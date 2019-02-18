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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Html5;

namespace SeleniumFixture.Model
{
    // This is a hack - no common interface for session storage and local storage.

    internal class NativeBrowserStorage : BrowserStorage
    {
        private readonly IWebStorage _storageDriver;
        private readonly StorageType _storageType;

        public NativeBrowserStorage(IWebDriver browserDriver, StorageType storageType)
            : base(browserDriver)
        {
            var hasStorage = (IHasWebStorage)browserDriver;
            _storageDriver = hasStorage.WebStorage;
            _storageType = storageType;
        }

        public override IEnumerable<string> KeySet => CallMethod("KeySet") as IEnumerable<string>;

        public object CallMethod(string methodName, params object[] parameterList)
        {
            // kludge to work around ILocalStorage and ISessionStorage not having a common interface
            var type = _storageType == StorageType.Local ? typeof(ILocalStorage) : typeof(ISessionStorage);
            var typeList = parameterList.Select(entry => entry.GetType()).ToArray();
            var methodInfo = type.GetMethod(methodName, typeList);
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            return _storageType == StorageType.Local
                ? methodInfo.Invoke(_storageDriver.LocalStorage, parameterList)
                : methodInfo.Invoke(_storageDriver.SessionStorage, parameterList);
        }

        public override bool Clear()
        {
            CallMethod("Clear");
            return true;
        }

        public override string GetItem(string key) => CallMethod("GetItem", key).ToString();

        public override bool RemoveItem(string key)
        {
            CallMethod("RemoveItem", key);
            return true;
        }

        public override void SetItem(string key, string value) => CallMethod("SetItem", key, value);
    }
}
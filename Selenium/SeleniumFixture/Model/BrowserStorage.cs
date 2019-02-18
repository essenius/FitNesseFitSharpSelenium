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
using SeleniumFixture.Utilities;

namespace SeleniumFixture.Model
{
    /// <summary>
    ///     Storage Type: Local or Session
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        ///     Local storage
        /// </summary>
        Local,

        /// <summary>
        ///     Session storage
        /// </summary>
        Session
    }

    internal abstract class BrowserStorage
    {
        protected BrowserStorage(IWebDriver browserDriver)
        {
            Debug.Assert(browserDriver != null, "browserDriver != null");
        }

        public string this[string key]
        {
            get => GetItem(key);
            set => SetItem(key, value);
        }

        public abstract IEnumerable<string> KeySet { get; }

        public abstract bool Clear();

        public string FindFirstKeyLike(string glob) => KeySet.FirstOrDefault(entry => entry.IsLike(glob));

        public abstract string GetItem(string key);

        public abstract bool RemoveItem(string key);

        public abstract void SetItem(string key, string value);
    }
}
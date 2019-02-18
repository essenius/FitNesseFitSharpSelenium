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
using System.Globalization;
using OpenQA.Selenium;

namespace SeleniumFixture.Model
{
    internal class SearchParser
    {
        public static string DefaultMethod { get; set; } = "id";

        public static string Delimiter { get; set; } = ":";

        public SearchParser(string searchCriterion)
        {
            if (searchCriterion == null)
            {
                throw new ArgumentNullException(nameof(searchCriterion),
                    "SearchParser requires a non-null search criterion");
            }

            if (searchCriterion.Contains(Delimiter))
            {
                Method =
                    searchCriterion.Substring(0, searchCriterion.IndexOf(Delimiter, StringComparison.Ordinal)).Trim();
                Locator =
                    searchCriterion.Substring(searchCriterion.IndexOf(Delimiter, StringComparison.Ordinal) + Delimiter.Length).Trim();
            }
            else
            {
                Method = DefaultMethod;
                Locator = searchCriterion.Trim();
            }
        }

        public SearchParser(string method, string locator)
        {
            Locator = locator ?? throw new ArgumentNullException(nameof(locator), "SearchParser requires a non-null locator");
            Method = string.IsNullOrEmpty(method) ? DefaultMethod : method;
        }

        public By By => ByFunction(Locator);

        private Func<string, By> ByFunction
        {
            get
            {
                switch (Method.ToUpper(CultureInfo.CurrentCulture))
                {
                    case @"CLASSNAME":
                        return By.ClassName;
                    case @"CSSSELECTOR":
                        return By.CssSelector;
                    case @"ID":
                        return By.Id;
                    case @"LINKTEXT":
                        return By.LinkText;
                    case @"NAME":
                        return By.Name;
                    case @"PARTIALLINKTEXT":
                        return By.PartialLinkText;
                    case @"TAGNAME":
                        return By.TagName;
                    case @"XPATH":
                        return By.XPath;
                    default:
                        throw new ArgumentException("Could not understand search method: " + Method);
                }
            }
        }

        public string Locator { get; }
        public string Method { get; }
    }
}
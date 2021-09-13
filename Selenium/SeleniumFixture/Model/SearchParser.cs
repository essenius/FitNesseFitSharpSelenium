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
using OpenQA.Selenium.Appium;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture.Model
{
    internal class SearchParser
    {
        private readonly Dictionary<string, Func<string, By>> _byMapping = new()
        {
            { @"ACCESSIBILITYID", MobileBy.AccessibilityId },
            { @"ANDROIDUIAUTOMATOR", MobileBy.AndroidUIAutomator },
            { @"CLASSNAME", By.ClassName },
            { @"CONTENT", CustomBy.Content },
            { @"CSSSELECTOR", By.CssSelector },
            { @"ID", By.Id },
            { @"IOSCLASSCHAIN", MobileBy.IosClassChain },
            { @"IOSNSPREDICATE", MobileBy.IosNSPredicate },
            { @"IOSUIAUTOMATION", MobileBy.IosUIAutomation },
            { @"LABEL", CustomBy.Label },
            { @"LINKTEXT", By.LinkText },
            { @"NAME", By.Name },
            { @"PARTIALCONTENT", CustomBy.PartialContent },
            { @"PARTIALLINKTEXT", By.PartialLinkText },
            { @"TAGNAME", By.TagName },
            { @"TIZENAUTOMATION", MobileBy.TizenAutomation },
            { @"TRIAL", CustomBy.Trial },
            { @"WINDOWSAUTOMATION", MobileBy.WindowsAutomation },
            { @"XPATH", By.XPath }
        };

        public SearchParser(string searchCriterion)
        {
            if (searchCriterion == null)
            {
                throw new ArgumentNullException(nameof(searchCriterion), ErrorMessages.NullSearchCriterion);
            }

            if (searchCriterion.Contains(Delimiter))
            {
                Method = searchCriterion[..searchCriterion.IndexOf(Delimiter, StringComparison.Ordinal)].Trim();
                Locator = searchCriterion[(searchCriterion.IndexOf(Delimiter, StringComparison.Ordinal) + Delimiter.Length)..].Trim();
            }
            else
            {
                Method = DefaultMethod;
                Locator = searchCriterion.Trim();
            }
        }

        public SearchParser(string method, string locator)
        {
            Locator = locator ?? throw new ArgumentNullException(nameof(locator), ErrorMessages.NullLocator);
            Method = string.IsNullOrEmpty(method) ? DefaultMethod : method;
        }

        public By By => ByFunction(Locator);

        private Func<string, By> ByFunction
        {
            get
            {
                var key = Method.ToUpper(CurrentCulture);
                if (_byMapping.ContainsKey(key)) return _byMapping[key];
                throw new ArgumentException("Could not understand search method: " + Method);
            }
        }

        public static string DefaultMethod { get; set; } = "trial";

        public static string Delimiter { get; set; } = ":";

        public string Locator { get; }
        public string Method { get; }
    }
}

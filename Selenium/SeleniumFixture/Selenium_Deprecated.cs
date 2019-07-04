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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixture
{
    /// <summary>
    ///     Deprecated and experimental methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Entry point for FitSharp")]
    public sealed partial class Selenium
    {
        private const string ApplicationNameObsoleteMessage =
            "use of Application Name is no longer supported since the W3C spec is enforced. Use New/Set Remote Browser At Address";

        [Documentation("Check for using deprecated functions by throwing an exception if a deprecated function is used.")]
        public static bool ExceptionOnDeprecatedFunctions { get; set; }

        private static void HandleDeprecatedFunction(string fixtureName, string fixtureReplacement)
        {
            if (ExceptionOnDeprecatedFunctions)
            {
                throw new WarningException("Use of deprecated function '" + fixtureName + "'. Replace by '" + fixtureReplacement + "'");
            }
        }

        [Obsolete("Use PageSource instead"), Documentation("Return the HTML source of the current page in context")]
        public string HtmlSource()
        {
            HandleDeprecatedFunction("HTML Source", "Page Source");
            return PageSource;
        }

        [Obsolete("Use LengthOfPageSource instead"), Documentation("Return the length of the current HTML page source")]
        public int LengthOfHtmlSource()
        {
            HandleDeprecatedFunction("Length Of HTML Source", "Length Of Page Source");
            return LengthOfPageSource;
        }

        [Obsolete(ApplicationNameObsoleteMessage)]
        public static string NewRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string name) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + name + ")");

        [Obsolete(ApplicationNameObsoleteMessage)]
        public static bool SetRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string applicationName) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + applicationName + ")");

        [Obsolete("Use WaitForPageSourceToChange"),
         Documentation("Wait for the HTML source to change. Can happen with dynamic pages")]
        public bool WaitForHtmlSourceToChange()
        {
            HandleDeprecatedFunction("Wait For HTML Source To Change", "Wait For Page Source To Change");
            return WaitForPageSourceToChange();
        }

        [Obsolete("use WaitUntilElementDoesNotExist instead")]
        public bool WaitForNoElement(string searchCriterion)
        {
            HandleDeprecatedFunction("Wait For No Element", "Wait Until Element Does Not Exist");
            return WaitFor(drv =>
            {
                try
                {
                    return drv.FindElement(new SearchParser(searchCriterion).By) == null;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
        }

        [Obsolete("Use WaitUntilPageSourceIsLargerThan"),
         Documentation("Wait until the HTML source has the specified minimum length. Useful when pages are built dynamically and asynchronously")]
        public bool WaitUntilHtmlSourceIsLargerThan(int thresholdLength)
        {
            HandleDeprecatedFunction("Wait Until HTML Source Is Larger Than", "Wait Until Page Source Is Larger Than");
            return WaitUntilPageSourceIsLargerThan(thresholdLength);
        }
    }
}
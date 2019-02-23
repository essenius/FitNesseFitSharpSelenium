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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ImageHandler;
using OpenQA.Selenium;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture
{
    /// <summary>
    ///     Page handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is the interface class")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by Fitsharp"),
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Fitsharp")]
    public sealed partial class Selenium
    {
        [Documentation("Returns the number of open pages (tabs, newly opened windows) for this browser instance")]
        public int PageCount => Driver.WindowHandles.Count;

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "FitSharp needs it as a string"),
         Documentation("Url of the current page")]
        public string Url => Driver.Url;

        [Documentation("Closes the current browser page. Does not close the browser itself if it's not the last page")]
        public bool ClosePage()
        {
            if (Driver == null) return false;
            Driver.Close();
            // TODO: check if closing the last page is handled appropriately
            return true;
        }

        [Documentation("Return the HTML source of the current page in context")]
        public string HtmlSource() => Driver != null ? Driver.PageSource : string.Empty;

        [Documentation("Return the length of the current HTML page source")]
        public int LengthOfHtmlSource() => HtmlSource().Length;

        [Documentation(
            "Send keys using the .Net Framework Forms.SendKeys.SendWait function. Executes locally, so does not work on remote Selenium servers." +
            "Can be useful for context menus, although the Chrome driver does not send keypresses there. " +
            " The syntax of the keys is slightly different than the Selenium syntax used in Send Keys To Element. " +
            "Primarily, control, alt and shift do not toggle, but only work on the following item. See MSDN SendKeys documentation")]
        public static void NativeSendKeys(string keys) => System.Windows.Forms.SendKeys.SendWait(keys);

        [Documentation("Opens the specified URL in the browser and wait for it to load.")]
        public bool Open(Uri url)
        {
            if (Driver == null) throw new StopTestException("Please set a browser before opening a page");
            Driver.SetImplicitWait(ImplicitWaitSeconds);
            Driver.Navigate().GoToUrl(url);
            StoreWindowHandles();
            return true;
        }

        [Documentation("Reload the current page")]
        public bool ReloadPage()
        {
            Driver.Navigate().Refresh();
            return true;
        }

        [Documentation("Take a screenshot and return it rendered as an image (html img). " +
                       "Note: may return black if you run the browser driver from within a service")]
        public static string Screenshot()
        {
            var snap = BrowserDriver.TakeScreenshot();
            return snap.Rendering;
        }

        [Documentation("Take a screenshot and return it as an object")]
        public static Snapshot ScreenshotObject() => BrowserDriver.TakeScreenshot();

        private bool TextExists(string textToSearch, bool caseInsensitive) => Regex.IsMatch(
            Driver.FindElement(By.CssSelector("body")).Text,
            "^[\\s\\S]*" + Regex.Escape(textToSearch) + "[\\s\\S]*$", caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);

        [Documentation("Check if a certain text exists on the page")]
        public bool TextExists(string textToSearch) => TextExists(textToSearch, false);

        [Documentation("Check if a certain text exists on the page (case insensitive search)")]
        public bool TextExistsIgnoringCase(string textToSearch) => TextExists(textToSearch, true);

        [Documentation("Get the title of the current page")]
        public string Title() => Driver != null ? Driver.Title : string.Empty;

        [Documentation("Wait for the HTML source to change. Can happen with dynamic pages")]
        public bool WaitForHtmlSourceToChange()
        {
            var currentSource = HtmlSource();
            return WaitFor(drv => HtmlSource() != currentSource);
        }

        [Documentation("Waits for a page to load, using default timeout")]
        public bool WaitForPageToLoad() => WaitUntilElementIsVisible("XPath://*[not (.='')]");

        private bool WaitForText(string textToSearch, bool caseInsensitive) => WaitFor(drv => TextExists(textToSearch, caseInsensitive));

        [Documentation("Waits for a certain text to be present (case sensitive search)")]
        public bool WaitForText(string textToSearch) => WaitForText(textToSearch, false);

        [Documentation("Waits for a certain text to be present (case insensitive search)")]
        public bool WaitForTextIgnoringCase(string textToSearch) => WaitForText(textToSearch, true);

        [Documentation("Wait until the HTML source has the specified minimum length. Useful when pages are built dynamically and asynchronously")]
        public bool WaitUntilHtmlSourceIsLargerThan(int thresholdLength) => WaitFor(drv => LengthOfHtmlSource() > thresholdLength);

        [Documentation("Wait until a called JavaScript function returns a value that is not false or null")]
        public bool WaitUntilScriptReturnsTrue(string script) => WaitFor(drv => ExecuteScript(script) is bool result && result);

        [Documentation("Wait for a title to appear, using a regular expression to search")]
        public bool WaitUntilTitleMatches(string regexPattern) => WaitFor(d => new Regex(regexPattern).IsMatch(d.Title));
    }
}
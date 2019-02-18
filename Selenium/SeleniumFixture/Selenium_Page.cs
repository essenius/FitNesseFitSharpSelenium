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
using OpenQA.Selenium.Support.UI;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture
{
    /// <summary>
    ///     Page handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by Fitsharp"),
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Fitsharp")]
    public sealed partial class Selenium
    {
        /// <summary>
        ///     Take a screenshot and return it rendered as an image
        /// </summary>
        /// <returns>the rendered image (html img)</returns>
        public static string Screenshot()
        {
            var snap = BrowserDriver.TakeScreenshot();
            return snap.Rendering;
        }

        /// <summary>
        ///     Take a screenshot and return it as an object
        /// </summary>
        /// <returns>the image as an object</returns>
        public static Snapshot ScreenshotObject() => BrowserDriver.TakeScreenshot();

        /// <summary>
        ///     Returns the number of open pages for this browser instance
        /// </summary>
        public int PageCount => Driver.WindowHandles.Count;

        /// <summary>
        ///     Get the Url of the current page
        /// </summary>
        /// <returns>the URL of the current page</returns>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "FitSharp needs it as a string")]
        public string Url => Driver.Url;

        /// <summary>
        ///     Close the current browser page (don't quit the browser if it's not the last page)
        /// </summary>
        /// <returns>whether or not it succeeded</returns>
        public bool ClosePage()
        {
            if (Driver == null) return false;
            Driver.Close();
            // TODO: check if closing the last page is handled appropriately
            return true;
        }

        /// <summary>
        ///     Return the current HTML page source
        /// </summary>
        /// <returns>html source of the current page in context (can also be just an iframe)</returns>
        public string HtmlSource() => Driver != null ? Driver.PageSource : string.Empty;

        /// <summary>
        ///     Return the length of the current HTML page source
        /// </summary>
        /// <returns>length of the page source string in characters</returns>
        public int LengthOfHtmlSource() => HtmlSource().Length;

        /// <summary>
        ///     Opens the specified URL in the browser and wait for it to load.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>true</returns>
        public bool Open(Uri url)
        {
            if (Driver == null) throw new StopTestException("Please set a browser before opening a page");
            Driver.SetImplicitWait(ImplicitWaitSeconds);
            Driver.Navigate().GoToUrl(url);
            //BrowserDriver.ShimPageForBrowser(Driver);

            StoreWindowHandles();
            return true;
        }

        /// <summary>
        ///     Reload the current page
        /// </summary>
        /// <returns></returns>
        public bool ReloadPage()
        {
            Driver.Navigate().Refresh();
            return true;
        }

        /// <summary>
        ///     Get the title of the current page.
        /// </summary>
        /// <returns>Title of the current page</returns>
        public string Title() => Driver != null ? Driver.Title : string.Empty;

        private T WaitFor<T>(Func<IWebDriver, T> conditionToWaitFor)
        {
            // Do not mix implicit wait with explicit wait
            var implicitWait = Driver.GetImplicitWait();
            Driver.SetImplicitWait(0);
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(TimeoutInSeconds));
            try
            {
                return wait.Until(conditionToWaitFor);
            }
            catch (WebDriverTimeoutException)
            {
                return default(T);
            }
            finally
            {
                Driver.SetImplicitWait(implicitWait);
            }
        }

        /// <summary>
        ///     Wait for the HTML source to change
        /// </summary>
        /// <returns>true if source changed, false if it timed out before changing</returns>
        public bool WaitForHtmlSourceToChange()
        {
            var currentSource = HtmlSource();
            return WaitFor(drv => HtmlSource() != currentSource);
        }

        /// <summary>
        ///     Waits for a page to load with default timeout.
        /// </summary>
        /// <returns>whether or not the page was loaded successfully (true/false)</returns>
        public bool WaitForPageToLoad() => WaitUntilElementIsVisible("XPath://*[not (.='')]");

        /// <summary>
        ///     Wait until the length of the HTML source is more than the specified number of characters
        /// </summary>
        /// <param name="thresholdLength">the length used as threshold</param>
        /// <returns>true if successful, false if timed out</returns>
        public bool WaitUntilHtmlSourceIsLargerThan(int thresholdLength)
        {
            return WaitFor(drv => LengthOfHtmlSource() > thresholdLength);
        }

        /// <summary>
        ///     Wait until the provided script returns a 'true' value
        /// </summary>
        public bool WaitUntilScriptReturnsTrue(string script)
        {
            return WaitFor(drv => ExecuteScript(script) is bool result && result);
        }

        /// <summary>
        ///     Wait for a title to appear, using a regular expression to search.
        /// </summary>
        /// <param name="regexPattern">The regular expression to match the title with</param>
        /// <returns>whether or no the pattern was matched before the timeout</returns>
        public bool WaitUntilTitleMatches(string regexPattern)
        {
            return WaitFor(d => new Regex(regexPattern).IsMatch(d.Title));
        }
    }
}
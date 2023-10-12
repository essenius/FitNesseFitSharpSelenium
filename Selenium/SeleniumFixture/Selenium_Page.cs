// Copyright 2015-2023 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;
using static System.FormattableString;

namespace SeleniumFixture
{
    // Page handling methods of the Selenium script table fixture for FitNesse
    // Only using XML documentation for Selenium here as it is a partial class, and we don't want multiple.

    /// <summary>
    ///     Selenium Fixture for FitSharp
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "This is the interface class")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by Fitsharp")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Fitsharp")]
    public sealed partial class Selenium
    {
        /// <returns>the length of the current page source</returns>
        public int LengthOfPageSource => PageSource.Length;

        /// <returns>the number of open pages (tabs, newly opened windows) for this browser instance</returns>
        public int PageCount => Driver.WindowHandles.Count;

        /// <returns>the page source of the current page in context</returns>
        public string PageSource => Driver != null ? Driver.PageSource : string.Empty;

        ///<summary>Url of the current page</summary>
        public string Url => Driver.Url;

        /// <summary>Closes the current browser page. Does not close the browser itself if it's not the last page</summary>
        public bool ClosePage()
        {
            if (Driver == null) return false;
            Driver.Close();
            // TODO: check if closing the last page is handled appropriately
            return true;
        }

        /// <summary>
        ///     Long press a key on an Android via a keycode (number or field name). Returns false if not run on an Android or the
        ///     keycode is not recognized
        /// </summary>
        public bool LongPressKeyCode(string keyCodeIn)
        {
            if (Driver is not AndroidDriver androidDriver) return false;
            var keyCode = KeyCode(keyCodeIn);
            if (keyCode == null) return false;
            androidDriver.LongPressKeyCode(keyCode.Value);
            return true;
        }

        /// <summary>Opens the specified URL in the browser and wait for it to load.</summary>
        public bool Open(Uri url)
        {
            if (Driver == null) throw new StopTestException(ErrorMessages.NoBrowserSpecified);
            Driver.SetImplicitWait(ImplicitWaitSeconds);
            Driver.Navigate().GoToUrl(url);
            StoreWindowHandles();
            return true;
        }

        //TODO implement meta states
        /// <summary>
        ///     Press a key on an Android via a keycode (number/field name). Returns false if not run on an Android or the keycode is
        ///     not recognized
        /// </summary>
        public bool PressKeyCode(string keyCodeIn)
        {
            if (Driver is not AndroidDriver androidDriver) return false;
            var keyCode = KeyCode(keyCodeIn);
            if (keyCode == null) return false;
            androidDriver.PressKeyCode(keyCode.Value);
            return true;
        }

        /// <summary>Reload the current page</summary>
        public bool ReloadPage()
        {
            Driver.Navigate().Refresh();
            return true;
        }

        /// <summary>
        ///     Take a screenshot and return it rendered as an html img. May return black if you run the browser driver from within a
        ///     service
        /// </summary>
        public static string Screenshot() => ScreenshotObject().Rendering;

        /// <summary>Take a screenshot and return it as an object</summary>
        public static Image ScreenshotObject() => BrowserDriverContainer.TakeScreenshot();

        /// <summary>Scroll up, down, left or right</summary>
        public bool Scroll(string direction)
        {
            var screenSize = Driver.Manage().Window.Size;
            var startX = (int)(screenSize.Width * 0.5);
            var startY = (int)(screenSize.Height * 0.5);
            var endX = startX;
            var endY = startY;
            // do this before the iOS check so we know the parameter value is right
            switch (direction?.ToUpperInvariant())
            {
                case "UP":
                    endY = (int)(screenSize.Height * 0.9);
                    break;
                case "DOWN":
                    endY = (int)(screenSize.Height * 0.1);
                    break;
                case "LEFT":
                    endX = (int)(screenSize.Width * 0.9);
                    break;
                case "RIGHT":
                    endX = (int)(screenSize.Width * 0.1);
                    break;
                default:
                    throw new ArgumentException($"Direction '{direction}' should be Up, Down, Left or Right");
            }
            switch (Driver)
            {
                case IOSDriver iosDriver:
                {
                    var scrollObject = new Dictionary<string, string> { { "direction", direction.ToLowerInvariant() } };
                    iosDriver.ExecuteScript("mobile: scroll", scrollObject);
                    return true;
                }
                case AndroidDriver androidDriver:
                    new Actions(androidDriver).MoveToLocation(startX, startY).ClickAndHold().MoveToLocation(endX, endY).Release().Perform();
                    return true;
            }

            // default - a browser
            var xPixels = endX - startX;
            var yPixels = endY - startY;
            ((IJavaScriptExecutor)Driver).ExecuteScript(Invariant($"window.scrollBy({xPixels}, {yPixels})"));
            return true;
        }

        private bool TextExists(string textToSearch, bool caseInsensitive)
        {
            var textOnPage = string.Empty;
            try
            {
                textOnPage = TextInElement("CssSelector:body");
            }
            catch (WebDriverException)
            {
                // includes not found and invalid 'by'. So, we now assume we're on native mobile.
                // Find all elements with text attributes or text nodes, and aggregate these values
                // This caters for all Android texts I could think of, and probably for iOS too (still to be tested)
                var textElements = Driver.FindElements(By.XPath("/" +
                                                                "/*" +
                                                                "[string(@text) or text()]"));
                textOnPage = textElements.Aggregate(textOnPage, (current, entry) => current + entry.Text + "\r\n");
            }
            return Regex.IsMatch(
                textOnPage,
                "^[\\s\\S]*" + Regex.Escape(textToSearch) + "[\\s\\S]*$",
                caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        /// <summary>Check if a certain text exists on the page</summary>
        public bool TextExists(string textToSearch) => TextExists(textToSearch, false);

        /// <summary>Check if a certain text exists on the page (case insensitive search)</summary>
        public bool TextExistsIgnoringCase(string textToSearch) => TextExists(textToSearch, true);

        /// <summary>Get the title of the current page</summary>
        public string Title() => Driver != null ? Driver.Title : string.Empty;

        /// <summary>Wait for the HTML source to change. Can happen with dynamic pages</summary>
        public bool WaitForPageSourceToChange()
        {
            var currentSource = PageSource;
            return WaitFor(_ => PageSource != currentSource);
        }

        /// <summary>Waits for a page to load, using default timeout</summary>
        public bool WaitForPageToLoad() => WaitUntilElementIsVisible("XPath://*[not (.='')]");

        private bool WaitForText(string textToSearch, bool caseInsensitive) =>
            WaitFor(_ => TextExists(textToSearch, caseInsensitive));

        /// <summary>Waits for a certain text to be present (case sensitive search)</summary>
        public bool WaitForText(string textToSearch) => WaitForText(textToSearch, false);

        /// <summary>Waits for a certain text to be present (case insensitive search)</summary>
        public bool WaitForTextIgnoringCase(string textToSearch) => WaitForText(textToSearch, true);

        /// <summary>Wait until the page source has the specified minimum length. Useful when pages are built dynamically and asynchronously</summary>
        public bool WaitUntilPageSourceIsLargerThan(int thresholdLength) =>
            WaitFor(_ => LengthOfPageSource > thresholdLength);

        /// <summary>Wait until a called JavaScript function returns a value that is not false or null</summary>
        public bool WaitUntilScriptReturnsTrue(string script) => WaitFor(_ => ExecuteScript(script) is true);

        /// <summary>Wait for a title to appear, using a regular expression to search</summary>
        public bool WaitUntilTitleMatches(string regexPattern) =>
            WaitFor(d => new Regex(regexPattern).IsMatch(d.Title));
    }
}

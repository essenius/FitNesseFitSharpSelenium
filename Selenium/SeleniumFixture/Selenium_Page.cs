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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ImageHandler;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.MultiTouch;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;
using static System.FormattableString;

namespace SeleniumFixture
{
    /// <summary>
    ///     Page handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is the interface class"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by Fitsharp"),
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Fitsharp")]
    public sealed partial class Selenium
    {
        [Documentation("Return the length of the current page source")]
        public int LengthOfPageSource => PageSource.Length;

        [Documentation("Returns the number of open pages (tabs, newly opened windows) for this browser instance")]
        public int PageCount => Driver.WindowHandles.Count;

        [Documentation("Return the page source of the current page in context")]
        public string PageSource => Driver != null ? Driver.PageSource : string.Empty;

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

        [Documentation(
            "Long press a key on an Android via a keycode (number or field name). Returns false if not run on an Android or the keycode is not recognised")]
        public bool LongPressKeyCode(string keyCodeIn)
        {
            if (!(Driver is AndroidDriver<AppiumWebElement> androidDriver)) return false;
            var keyCode = KeyCode(keyCodeIn);
            if (keyCode == null) return false;
            androidDriver.LongPressKeyCode(keyCode.Value);
            return true;
        }

        [Documentation(
            "Send keys using the .Net Framework Forms.SendKeys.SendWait function. Executes locally, so does not work on remote Selenium servers." +
            "Can be useful for context menus, although the Chrome driver does not send keypresses there. " +
            " The syntax of the keys is slightly different than the Selenium syntax used in Send Keys To Element. " +
            "Primarily, control, alt and shift do not toggle, but only work on the following item. See MSDN SendKeys documentation")]
        public static void NativeSendKeys(string keys) => System.Windows.Forms.SendKeys.SendWait(keys);

        [Documentation("Opens the specified URL in the browser and wait for it to load.")]
        public bool Open(Uri url)
        {
            if (Driver == null) throw new StopTestException(ErrorMessages.NoBrowserSpecified);
            Driver.SetImplicitWait(ImplicitWaitSeconds);
            Driver.Navigate().GoToUrl(url);
            StoreWindowHandles();
            return true;
        }

        //TODO implement metastates
        [Documentation("Press a key on an Android via a keycode (number/field name). Returns false if not run on an Android or the keycode is not recognised")]
        public bool PressKeyCode(string keyCodeIn)
        {
            if (!(Driver is AndroidDriver<AppiumWebElement> androidDriver)) return false;
            var keyCode = KeyCode(keyCodeIn);
            if (keyCode == null) return false;
            androidDriver.PressKeyCode(keyCode.Value);
            return true;
        }

        [Documentation("Reload the current page")]
        public bool ReloadPage()
        {
            Driver.Navigate().Refresh();
            return true;
        }

        [Documentation("Take a screenshot and return it rendered as an html img. May return black if you run the browser driver from within a service")]
        public static string Screenshot()
        {
            var snap = BrowserDriverContainer.TakeScreenshot();
            return snap.Rendering;
        }

        [Documentation("Take a screenshot and return it as an object")]
        public static Snapshot ScreenshotObject() => BrowserDriverContainer.TakeScreenshot();

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "False positive")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Need lower case")]
        [Documentation("Scroll up, down, left or right")]
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
            if (Driver is IOSDriver<IOSElement> iosDriver)
            {
                var scrollObject = new Dictionary<string, string> {{"direction", direction.ToLowerInvariant()}};
                iosDriver.ExecuteScript("mobile: scroll", scrollObject);
                return true;
            }

            if (Driver is AndroidDriver<AppiumWebElement> androidDriver)
            {
                new TouchAction(androidDriver).Press(startX, startY).MoveTo(endX, endY).Release().Perform();
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
                var textElements = Driver.FindElements(By.XPath("//*[string(@text) or string(text())]"));
                foreach (var entry in textElements)
                {
                    textOnPage += entry.Text + "\r\n";
                }
            }
            return Regex.IsMatch(
                textOnPage,
                "^[\\s\\S]*" + Regex.Escape(textToSearch) + "[\\s\\S]*$", caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        [Documentation("Check if a certain text exists on the page")]
        public bool TextExists(string textToSearch) => TextExists(textToSearch, false);

        [Documentation("Check if a certain text exists on the page (case insensitive search)")]
        public bool TextExistsIgnoringCase(string textToSearch) => TextExists(textToSearch, true);

        [Documentation("Get the title of the current page")]
        public string Title() => Driver != null ? Driver.Title : string.Empty;

        [Documentation("Wait for the HTML source to change. Can happen with dynamic pages")]
        public bool WaitForPageSourceToChange()
        {
            var currentSource = PageSource;
            return WaitFor(drv => PageSource != currentSource);
        }

        [Documentation("Waits for a page to load, using default timeout")]
        public bool WaitForPageToLoad() => WaitUntilElementIsVisible("XPath://*[not (.='')]");

        private bool WaitForText(string textToSearch, bool caseInsensitive) => WaitFor(drv => TextExists(textToSearch, caseInsensitive));

        [Documentation("Waits for a certain text to be present (case sensitive search)")]
        public bool WaitForText(string textToSearch) => WaitForText(textToSearch, false);

        [Documentation("Waits for a certain text to be present (case insensitive search)")]
        public bool WaitForTextIgnoringCase(string textToSearch) => WaitForText(textToSearch, true);

        [Documentation("Wait until the page source has the specified minimum length. Useful when pages are built dynamically and asynchronously")]
        public bool WaitUntilPageSourceIsLargerThan(int thresholdLength) => WaitFor(drv => LengthOfPageSource > thresholdLength);

        [Documentation("Wait until a called JavaScript function returns a value that is not false or null")]
        public bool WaitUntilScriptReturnsTrue(string script) => WaitFor(drv => ExecuteScript(script) is bool result && result);

        [Documentation("Wait for a title to appear, using a regular expression to search")]
        public bool WaitUntilTitleMatches(string regexPattern) => WaitFor(d => new Regex(regexPattern).IsMatch(d.Title));
    }
}
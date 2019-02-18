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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

[assembly: CLSCompliant(true)]

namespace SeleniumFixture
{
    /// <summary>
    ///     Text handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "WebDriver access class for FitSharp")]
    public sealed partial class Selenium
    {
        /// <summary>
        ///     Documentation, in use by the FixtureExplorer
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FixtureExplorer")]
        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Selenium WebDriver fixture"},
            {nameof(AcceptAlert), "Accept an alert, confirm or prompt dialog (press OK)"},
            {nameof(AddToWebStorage), "Add a set of key/value pairs to a web store"},
            {nameof(AlertIsPresent), "Check whether an alert, confirm or prompt box is active"},
            {nameof(AllOptionsOfElementBy), "Return a list of all options in a select element"},
            {nameof(AttributeOfElement), "Return an attribute value of a certain element"},
            {nameof(CachedWindowHandles), "Return the cached window handles (debugging)"},
            {nameof(ClearElement), "Empty the contents of an element"},
            {nameof(ClearWebStorage), "Clear a web store (local or session)"},
            {nameof(ClickElement), "Click a specific element"},
            {nameof(ClickElementWithModifier), "Click a specific element with a modifier key (Alt, Shift, Control - in Selenium SendKey format)" },
            {nameof(Close), "Closes this browser session"},
            {nameof(ClosePage), "Closes the current browser page. Does not close the browser itself"},
            {nameof(CommandTimeoutSeconds), "Command Timeout value in seconds"},
            {nameof(CssPropertyOfElement), "Return a CSS property of a certain element"},
            {
                nameof(CurrentWindowName),
                "Return the internal name (handle) of the current browser window. This can be used later in a Select Window command"
            },
            {
                nameof(DefaultSearchMethod),
                "Default search method for finding elements: ClassName, CssSelector, Id, LinkText, Name, PartialLinkText, Tagname, XPath"
            },
            {nameof(DeselectOptionInElement), "Deselect an option in a select element"},
            {nameof(DismissAlert), "Dismiss an alert, confirm or prompt dialog (press Cancel)"},
            {nameof(DoubleClickElement), "Double click an element"},
            {nameof(DragElementAndDropOnElement), "Drag an element and drop it onto another element"},
            {nameof(DragElementAndDropOnElementInDriver), "Drag an element and drop it onto another element in another driver"},
            {nameof(Driver), "The current driver object"},
            {nameof(DriverId), "The id of the current driver"},
            {nameof(ElementExists), "Returns whether a certain element exists on the page"},
            {nameof(ElementHasAttribute), "Returns whether a certain element has the specified attribute"},
            {nameof(ElementIsClickable), "Returns whether a certain element can be clicked (i.e. is enabled and displayed)" },
            {nameof(ElementIsVisible), "Returns whether a certain element is visible on the page"},
            {nameof(ExceptionOnDeprecatedFunctions), "SetException On Deprecated Functions"},
            {nameof(ExecuteAsyncScript), "Execute JavaScript asynchronously (in the browser)"},
            {nameof(ExecuteScript), "Execute JavaScript (in the browser)"},
            {
                nameof(ExecuteScriptWithParameters),
                "Execute JavaScript using parameters. If a parameter has a locator format (with colon), then it is substituted by the element"
            },
            {nameof(ExecuteScriptWithPlainParameters), "Execute JavaScript using parameters. No substitution of elements is attempted" },
            {nameof(GetFromWebStorage), "Get a value from a web store (local or session)"},
            {nameof(GetKeyLikeFromWebStorage), "Find the first item matching a glob pattern (with *?)"},
            {nameof(HtmlSource), "Return the HTML source of the current page"},
            {nameof(ImplicitWaitSeconds), "Set the number of seconds for implicit wait (0 = disable)"},
            {nameof(IntegratedAuthenticationDomain), "Domain where Integrated Authentication is to be used"},
            {nameof(LengthOfHtmlSource), "Return the length of the HTML source"},
            {nameof(MaximizeWindow), "Maximize browser window"},
            {nameof(MoveTo), "Move the cursor to a certain element"},
            {
                nameof(NativeSendKeys), "Send keys using the .Net Framework Forms.SendKeys.SendWait function." +
                                        " The syntax of the keys is slightly different than the Selenium syntax used in Send Keys To Element. " +
                                        "Primarily, control, alt and shift do not toggle, but only work on the following item. See MSDN SendKeys documentation"
            },
            {nameof(NewBrowser), "Creates a new browser instance and makes it current"},
            {
                nameof(NewRemoteBrowserAtAddress),
                "Just like SetRemoteBrowserAtAddress, but returns the driver ID instead of a boolean. Uses default application name 'FitNesSelenium'"
            },
            {
                nameof(NewRemoteBrowserAtAddressWithCapabilities),
                "Just like SetRemoteBrowserAtAddressWithCapabilities, but returns the driver ID instead of a boolean"
            },
#pragma warning disable 618
            {nameof(NewRemoteBrowserAtAddressWithName), "Deprecated. Use NewRemoteBrowserAtAddress"},
#pragma warning restore 618
            {nameof(Open), "Opens the specified URL in the browser and wait for it to load."},
            {nameof(PageCount), "Returns the number of open pages (tabs, newly opened windows) for this browser instance" },
            {
                nameof(ProtectedModesAreEqual),
                "Check if Protected Mode for all security zones have the same value, and throw a StopTestException if not"
            },
            {nameof(ReloadPage), "Reload the current page"},
            {nameof(RemoveFromWebStorage), "Remove an item from web storage (local or session)"},
            {nameof(RespondToAlert), "Provide a response to a prompt and confirm (press OK)"},
            {nameof(RightClickElement), "Right click an element (a.k.a. context click)"},
            {
                nameof(Screenshot),
                "Take a screenshot and return it rendered as an image. Note: may return black if you run the browser driver from within a service"
            },
            {nameof(ScreenshotObject), "Take a screenshot and return it as an object"},
            {nameof(SearchDelimiter), " Delimiter for method and locator in element searches (default is ':')"},
            {
                nameof(SelectWindow),
                "Selects a window using a window handle (which was returned using Wait For New Window Name or Current Window Name). " +
                "If no handle is specified, it will select the window that was used for the Open command."
            },
            {nameof(SelectOptionInElement), "Select an option in a select element"},
            {nameof(SelectedOptionInElement), "Return the fist selected item text of a listbox (single or multi-value)"},
            {
                nameof(SelectedOptionInElementBy),
                "Returns the selected option for single-select elements, or the first selected option for multi-select elements"
            },
            {nameof(SelectedOptionsInElement), "Return the selected item texts of a multi-select listbox"},
            {
                nameof(SelectedOptionsInElementBy),
                "Returns the selected option for single-select elements, or the first selected option for multi-select elements"
            },
            {nameof(SendKeysTo), "Emulate typing keys into an element"},
            {
                nameof(SendKeysToElementIfTypeIs),
                "Emulate typing keys into an element, to be used for input elements that are potentially unknown by browsers " +
                "(e.g. date, time) and for which a fall back to basic text elements may be done. Return true if successful, null if type not recognized"
            },
            {nameof(SetAttributeOfElementTo), "Set an attribute value of a certain element. Uses JavaScript"},
            {
                nameof(SetBrowser),
                "Sets the browser to be used for the web tests. Valid values are Chrome, Firefox, IE/Internet Explorer, PhantomJs"
            },
            {nameof(SetDriver), "Set the current browser driver using its ID (from NewBrowser)"},
            {nameof(SetElementTo), "Sets the value of a certain element (via SendKeys)"},
            {nameof(SetElementChecked), "Check an element (i.e. select it if it was not already selected)"},
            {nameof(SetElementUnchecked), "Uncheck an element"},
            {nameof(SetElementChecked) + "`2", "Check/uncheck an element"},
            {nameof(SetInWebStorageTo), "Set a key/value pair in a web store"},
            {
                nameof(SetProxyType),
                "Sets the http and SSL proxy for the test. This is a system wide setting, so coordinate carefully and revert to original values after the test"
            },
            {
                nameof(SetProxyTypeValue),
                "Sets the http and SSL proxy for the test. This is a system wide setting, so coordinate carefully and revert to original values after the test"
            },
            {
                nameof(SetRemoteBrowserAtAddress),
                "Specifies that the test will be run at a remote Selenium server. This command will raise a StopTestException in case " +
                "it was unable to connect to the remote browser. FitNesse will then stop processing the rest of the test case. " +
                "Valid browsers are Chrome, Firefox, IE/Internet Explorer, PhantomJs"
            },
            {
                nameof(SetRemoteBrowserAtAddressWithCapabilities),
                "Specifies that the test will be run at a remote Selenium server with a dictionary of desired capabilities. " +
                "applicationName needs to be set explicitly if needed."
            },
#pragma warning disable 618
            {nameof(SetRemoteBrowserAtAddressWithName), "Deprecated. Use SetRemoteBrowserAtAddress instead"},
#pragma warning restore 618
            {
                nameof(SetTextInElementTo),
                "Set the innerHTML of a certain element (via JavaScript). Overwrites all content or other elements that may be in there."
            },
            {nameof(SetTimeoutSeconds), "Set the default timeout for all wait commands. Default value is 3 seconds"},
            {nameof(SetWindowSizeX), "Set the size of a browser window (width x height)"},
            {
                nameof(StoreWindowHandles),
                "Stores all the window handles that the browser driver handles, including the main window."
            },
            {nameof(StoreWindowHandles) + "`1", "Stores all the window handles that the browser driver handles"},
            {nameof(SubmitElement), "Submit a form via an element"},
            {nameof(SwitchToDefaultContext), "Switch to the root html context (i.e. leave frame context)"},
            {nameof(SwitchToFrame), "Switch context to a frame in the current html page"},
            {nameof(TextExists), "Check if a certain text exists on the page"},
            {nameof(TextExistsIgnoringCase), "Check if a certain text exists on the page (case insensitive search)"},
            {nameof(TextInElement), "Return the text of a certain element"},
            {nameof(TextInElementMatches), "Check if the text in a certain element matches with a regular expression"},
            {nameof(Title), "Get the title of the current page"},
            {nameof(UseWebStorage), "Select either Local or Session storage (to work on other Web Storage functions)"},
            {nameof(UploadFileInElement), "Upload a file into an element suited for that"},
            {nameof(Url), "Url of the current page"},
            {nameof(VersionInfo), "Displays version info of the fixture"},
            {nameof(WaitForElement), "Waits for an element to be present on the page"},
            {nameof(WaitForHtmlSourceToChange), "Wait for the HTML source to change"},
            {
                nameof(WaitForNewWindowName),
                "After clicking a link that is known to open a new window, wait for that new window to appear"
            },
#pragma warning disable 618
            {nameof(WaitForNoElement), "Deprecated. Use WaitUntilElementDoesNotExist"},
#pragma warning restore 618
            {nameof(WaitForPageToLoad), "Waits for a page to load, using default  timeout"},
            {nameof(WaitForText), "Waits for a certain text to be present (case sensitive search)"},
            {nameof(WaitForTextIgnoringCase), "Waits for a certain text to be present (case insensitive search)"},
            {
                nameof(WaitSeconds),
                "Wait a specified number of seconds (can be fractions). Note: this seems to impact iframe context, so use with care."
            },
            {nameof(WaitUntilElementDoesNotExist), "Wait until an element does not exist on the page"},
            {nameof(WaitUntilIsClickable), "Wait until an element is clickable on the page"},
            {nameof(WaitUntilElementIsInvisible), "Wait until an element is invisible on the page"},
            {nameof(WaitUntilElementIsNotClickable), "Wait until an element is not clickable on the page"},
            {nameof(WaitUntilElementIsVisible), "Wait until an element is visible on the page"},
            {
                nameof(WaitUntilHtmlSourceIsLargerThan),
                "Wait until the HTML source has the specified minimum length. Useful when pages are built up dynamically and asynchronously"
            },
            {
                nameof(WaitUntilScriptReturnsTrue),
                "Wait until a called JavaScript function returns a value that is not false or null"
            },
            {
                nameof(WaitUntilTextInElementMatches),
                "Wait until the text in an element matches the regular expression provided"
            },
            {nameof(WaitUntilTitleMatches), "Wait for a title to appear, using a regular expression to search"},
            {nameof(WebStorage), "Get/set a Web Store. Clear any existing values beforehand"},
            {nameof(WindowHandles), "Returns the window handles (for debugging purposes)"},
            {nameof(WindowHeight), "The height of the browser window"},
            {nameof(WindowWidth), "The width of the browser window"}
        };

        /// <summary>
        ///     Send Keys using .Net's Forms.SendKeys.SendWait function, not specifically to an element.
        ///     Can be useful in e.g. context menus.
        ///     (there is a bug in the Chrome driver that causes keypresses not to be sent to the context menu)
        ///     Notice that this has a somewhat different syntax than the Selenium function used in Send Keys To Element
        ///     The main difference is that shift, ctrl and alt do not toggle, but just work on the next item.
        ///     Doesn't work on remote Selenium servers - it executes the command locally.
        /// </summary>
        /// <param name="keys">the keys typed. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s</param>
        public static void NativeSendKeys(string keys)
        {
            System.Windows.Forms.SendKeys.SendWait(keys);
        }

        private bool TextExists(string textToSearch, bool caseInsensitive) => Regex.IsMatch(
            Driver.FindElement(By.CssSelector("body")).Text,
            "^[\\s\\S]*" + Regex.Escape(textToSearch) + "[\\s\\S]*$", caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);

        /// <summary>
        ///     Check whether a certain text is present on the page
        /// </summary>
        /// <param name="textToSearch">The text to search</param>
        /// <returns>whether or not the text is present on the page</returns>
        public bool TextExists(string textToSearch) => TextExists(textToSearch, false);

        /// <summary>
        ///     Check whether a certain text is present on the page, ignoring case differences
        /// </summary>
        /// <param name="textToSearch">The text to search.</param>
        /// <returns>whether or not the text is present on the page</returns>
        public bool TextExistsIgnoringCase(string textToSearch) => TextExists(textToSearch, true);

        private bool WaitForText(string textToSearch, bool caseInsensitive)
        {
            return WaitFor(drv => TextExists(textToSearch, caseInsensitive));
        }

        /// <summary>
        ///     Waits for a certain text to be present (case sensitive search).
        /// </summary>
        /// <param name="textToSearch">The text to search.</param>
        /// <returns>whether or not the text was found before the timeout</returns>
        public bool WaitForText(string textToSearch) => WaitForText(textToSearch, false);

        /// <summary>
        ///     Waits for a certain text to be present (using case insensitive search).
        /// </summary>
        /// <param name="textToSearch">The text to search.</param>
        /// <returns>whether or not the text was found</returns>
        public bool WaitForTextIgnoringCase(string textToSearch) => WaitForText(textToSearch, true);
    }
}
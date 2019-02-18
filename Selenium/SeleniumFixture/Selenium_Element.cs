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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture
{
    /// <summary>
    ///     Element handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public sealed partial class Selenium
    {
        private static void ChooseAction(bool choice, Action trueAction, Action falseAction)
        {
            if (choice)
            {
                trueAction();
                return;
            }
            falseAction();
        }

        /// <summary>
        ///     Default search method for finding elements: ClassName, CssSelector, Id, LinkText, Name, PartialLinkText, Tagname,
        ///     XPath
        /// </summary>
        public static string DefaultSearchMethod
        {
            get => SearchParser.DefaultMethod;
            set => SearchParser.DefaultMethod = value;
        }

        private static bool IsClickable(IWebElement element) => element.Displayed && element.Enabled;

        /// <summary>
        ///     Delimiter for method and locator in element searches (default is ":")
        /// </summary>
        public static string SearchDelimiter
        {
            get => SearchParser.Delimiter;
            set => SearchParser.Delimiter = value;
        }

        private static bool SendKeysTo(IWebElement element, string keys)
        {
            var formattedKeys = new KeyConverter(keys).ToSeleniumFormat;
            element.SendKeys(formattedKeys);
            return true;
        }

        private IWebElement _query;

        /// <summary>
        ///     Return all the options of a listbox in a collection
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <param name="method">value or text, to return the values or the texts</param>
        /// <returns>The collection of options</returns>
        public ReadOnlyCollection<string> AllOptionsOfElementBy(string searchCriterion, string method) => 
            DoOperationOnElement(searchCriterion, element =>
            {
                return new ReadOnlyCollection<string>(
                    new SelectElement(element).Options.Select(option => option.GetValueBy(method)).ToList());
            });

        /// <summary>
        ///     Return an attribute  of an element
        /// </summary>
        /// <param name="attribute">Element attribute value to be returned</param>
        /// <param name="searchCriterion">criterion for selecting the element</param>
        /// <returns>the attribute value</returns>
        public string AttributeOfElement(string attribute, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => element.GetAttribute(attribute));

        /// <summary>
        ///     Clear an element
        /// </summary>
        /// <param name="searchCriterion"></param>
        /// <returns>true</returns>
        public bool ClearElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                element.Clear();
                return true;
            });

        /// <summary>
        ///     Click a specific element
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException" />
        /// <returns>true or an exception</returns>
        public bool ClickElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                MoveTo(element);
                if (!WaitUntilIsClickable(element)) return false;

                element.Click();
                return true;
            });

        /// <summary>
        ///     Click a specific element if it is visible. Useful for e.g. cookie confirmations
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <returns>true or an exception</returns>
        public bool? ClickElementIfVisible(string searchCriterion)
        {
            if (ElementExists(searchCriterion) && ElementIsVisible(searchCriterion))
            {
                return ClickElement(searchCriterion);
            }
            return null;
        }

        /// <summary>
        ///     Clicks a specific element with a modifier key (e.g. alt key)
        /// </summary>
        public bool ClickElementWithModifier(string searchCriterion, string modifier)
        {
            var keys = new KeyConverter(modifier).ToSeleniumFormat;
            return DoOperationOnElement(searchCriterion, element =>
            {
                new Actions(Driver).KeyDown(keys).Click(element).KeyUp(keys).Perform();
                return true;
            });
        }

        /// <summary>
        ///     Return the value of a CSS property of an element
        /// </summary>
        /// <param name="cssProperty">CSS property to inspect (e.g. color)</param>
        /// <param name="searchCriterion">Criterion to identify the element (method:locator)</param>
        /// <returns>the value of the CSS property</returns>
        public string CssPropertyOfElement(string cssProperty, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => element.GetCssValue(cssProperty));

        /// <summary>
        ///     Deselect an item in a list box or dropdown.
        /// </summary>
        /// <param name="item">option to select (method:value; method can be text, value, id. Default is text</param>
        /// <param name="searchCriterion">Criterion to identify the element (method:locator)</param>
        /// <returns>true</returns>
        public bool DeselectOptionInElement(string item, string searchCriterion) =>
            SelectOrDeselectOptionInElement(false, item, searchCriterion);

        private T DoOperationOnElement<T>(string searchCriterion, Func<IWebElement, T> operation)
        {
            if (Driver == null) return default(T);

            StaleElementReferenceException staleElementReferenceException = null;
            for (var attempts = 0; attempts < 2; attempts++)
            {
                try
                {
                    var element = Driver.FindElement(new SearchParser(searchCriterion).By);
                    Debug.Assert(element != null);
                    return operation(element);
                }
                catch (StaleElementReferenceException sere)
                {
                    staleElementReferenceException = sere;
                    Debug.Print("Stale element encountered - " + attempts);
                    Thread.Sleep(100);
                }
            }
            throw new StaleElementReferenceException("Still stale after retrying", staleElementReferenceException);
        }

        /// <summary>
        ///     Double click an element
        /// </summary>
        /// <param name="searchCriterion">The criterion for selecting the element<see cref="ClickElement" /></param>
        /// <returns>whether or not the double click succeeded</returns>
        public bool DoubleClickElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                MoveTo(element);
                // double click has issues for geckodriver.  
                // Firefox version 58 is supposed to fix this issue but did not (for me).
                // https://github.com/mozilla/geckodriver/issues/661
                if (Driver.IsFirefox())
                {
                    DoubleClickWithJavascript(element);
                }
                else
                {
                    new Actions(Driver).DoubleClick(element).Build().Perform();
                }

                return true;
            });

        private void DoubleClickWithJavascript(IWebElement element)
        {
            const string script = "var myEvent = new MouseEvent('dblclick', {bubbles: true, cancelable: true, view: window});"
                                  + "arguments[0].dispatchEvent(myEvent);";
            ((IJavaScriptExecutor)Driver).ExecuteScript(script, element);
        }

        /// <summary>
        ///     Drag an element DragElement onto another element DropElement
        ///     While Selenium doesn't do this right natively for HTML5, we inject JavaScript to make that work
        /// </summary>
        /// <param name="dragElementLocator">Locator for the DragElement</param>
        /// <param name="dropElementLocator">Locator for the DropElement</param>
        /// <returns>true or an exception</returns>
        public bool DragElementAndDropOnElement(string dragElementLocator, string dropElementLocator)
        {
            var dragElement = Driver.FindElement(new SearchParser(dragElementLocator).By);
            var dropElement = Driver.FindElement(new SearchParser(dropElementLocator).By);

            DragDrop.Html5DragAndDrop(Driver, dragElement, dropElement, DragDrop.Position.Center,
                DragDrop.Position.Center);
            return true;
        }

        /// <summary>
        ///     Drag an element from the current browser window onto an element in the window identified by the driver handle
        /// </summary>
        /// <param name="dragElementLocator">The locator for the source (drag) in the current browser window</param>
        /// <param name="dropElementLocator">The locator for the target (drop) in the target browser window</param>
        /// <param name="dropDriverHandle">The handle of the target browser window</param>
        /// <returns>true or an exception</returns>
        public bool DragElementAndDropOnElementInDriver(string dragElementLocator, string dropElementLocator, string dropDriverHandle)
        {
            var dragElement = Driver.FindElement(new SearchParser(dragElementLocator).By);
            var dragDriverHandle = DriverId;
            var sourceDriver = Driver;
            SetDriver(dropDriverHandle);
            var targetDriver = Driver;
            var dropElement = Driver.FindElement(new SearchParser(dropElementLocator).By);
            SetDriver(dragDriverHandle);
            DragDrop.DragToWindow(sourceDriver, dragElement, targetDriver, dropElement);
            return true;
        }

        /// <summary>
        ///     Returns whether a certain element exists on the page.
        /// </summary>
        /// <param name="searchCriterion">The criterion for selecting the element<see cref="ClickElement" /></param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException" />
        /// <returns>whether or not the element is present on the page</returns>
        public bool ElementExists(string searchCriterion) =>
            Driver != null && Driver.FindElements(new SearchParser(searchCriterion).By).Any();

        /// <summary>
        ///     Check if an element has a certain attribute
        /// </summary>
        /// <param name="searchCriterion">Criterion foe selecting the element</param>
        /// <param name="attribute">the attribute to check existence of</param>
        /// <returns>true if the attribute exists, false if not</returns>
        public bool ElementHasAttribute(string searchCriterion, string attribute) => AttributeOfElement(attribute, searchCriterion) != null;

        /// <summary>
        ///     Check if an element can be clicked
        /// </summary>
        /// <param name="searchCriterion">the search criterion to find the element with</param>
        /// <returns>whether or not the element can be clicked</returns>
        public bool ElementIsClickable(string searchCriterion) => DoOperationOnElement(searchCriterion, IsClickable);

        /// <summary>
        ///     Check if element is visible
        /// </summary>
        /// <param name="searchCriterion"></param>
        /// <returns></returns>
        public bool ElementIsVisible(string searchCriterion) => DoOperationOnElement(searchCriterion, element => element.Displayed);

        private void MoveTo(IWebElement element)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView({behavior: 'instant', block: 'nearest', inline: 'nearest'});", element);
            new Actions(Driver).MoveToElement(element).Build().Perform();
        }

        /// <summary>
        ///     Move to a certain element
        /// </summary>
        /// <param name="searchCriterion">The criterion for selecting the element<see cref="ClickElement" /></param>
        /// <returns>whether or not the move succeeded</returns>
        public bool MoveToElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                MoveTo(element);
                return true;
            });

        /// <summary>
        ///     Right-click an element (show the context menu)
        /// </summary>
        /// <param name="searchCriterion">The criterion for selecting the element<see cref="ClickElement" /></param>
        /// <returns>whether or not the context menu click succeeded</returns>
        public bool RightClickElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                // does not work right for Chrome. It opens the menu, but does not seem to be able to interact with it.
                // A construct with .ContextClick.SendKeys.Build.Perform doesn't work either.
                new Actions(Driver).MoveToElement(element).ContextClick(element).Build().Perform();
                return true;
            });

        /// <summary>
        ///     Return the fist selected item text of a listbox (single or multi-value)
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <returns>The first selected item text</returns>
        public string SelectedOptionInElement(string searchCriterion)
        {
            var list = SelectedOptionsInElement(searchCriterion);
            return list?.FirstOrDefault();
        }

        /// <summary>
        ///     Return the fist selected item text of a listbox (single or multi-value)
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <param name="method">value or text, to return the values or the texts</param>
        /// <returns>The first selected item text</returns>
        public string SelectedOptionInElementBy(string searchCriterion, string method)
        {
            var list = SelectedOptionsInElementBy(searchCriterion, method);
            return list?.FirstOrDefault();
        }

        /// <summary>
        ///     Return the selected item texts of a multi-select listbox
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <returns>The collection of selected options</returns>
        public ReadOnlyCollection<string> SelectedOptionsInElement(string searchCriterion) =>
            SelectedOptionsInElementBy(searchCriterion, "text");

        /// <summary>
        ///     Return the selected items of a multi-select listbox
        /// </summary>
        /// <param name="searchCriterion">The criterion to identify the element (method:locator)</param>
        /// <param name="method">value or text, to return the values or the texts</param>
        /// <returns>The collection of selected options</returns>
        public ReadOnlyCollection<string> SelectedOptionsInElementBy(string searchCriterion, string method) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                // we use the item parser to parse the selection option
                return new ReadOnlyCollection<string>(
                    new SelectElement(element).AllSelectedOptions.Select(option => option.GetValueBy(method)).ToList());
            });

        /// <summary>
        ///     Select an item in a list box or dropdown.
        /// </summary>
        /// <param name="item">option to select (method:value; method can be text, value, id. Default is text</param>
        /// <param name="searchCriterion">Criterion to identify the element (method:locator)</param>
        /// <returns>true</returns>
        public bool SelectOptionInElement(string item, string searchCriterion) =>
            SelectOrDeselectOptionInElement(true, item, searchCriterion);

        private bool SelectOrDeselectOptionInElement(bool select, string item, string searchCriterion)
        {
            var itemParser = new SearchParser(item);
            // we use the item parser to parse the selection option
            var method = itemParser.Method;
            var locator = itemParser.Locator;
            return DoOperationOnElement(searchCriterion, element =>
            {
                var selector = new SelectElement(element);
                switch (method.ToUpperInvariant())
                {
                    case "VALUE":
                        ChooseAction(select,
                            () => { selector.SelectByValue(locator); },
                            () => { selector.DeselectByValue(locator); });
                        break;
                    case "INDEX":
                        var index = int.Parse(locator, CultureInfo.InvariantCulture);
                        ChooseAction(select,
                            () => { selector.SelectByIndex(index); },
                            () => { selector.DeselectByIndex(index); });
                        break;
                    default: // includes 'text', and 'id'  in case SearchParser doesn't get a method
                        ChooseAction(select,
                            () => { selector.SelectByText(locator); },
                            () => { selector.DeselectByText(locator); });
                        break;
                }

                return true;
            });
        }

        /// <summary>
        ///     Send series of keyboard events to the active element
        /// </summary>
        /// <param name="keys">the keys typed. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s</param>
        /// <returns>true</returns>
        public bool SendKeys(string keys)
        {
            var element = Driver.SwitchTo().ActiveElement();
            return SendKeysTo(element, keys);
        }

        /// <summary>
        ///     Send a series of keyboard events to an element
        /// </summary>
        /// <param name="keys">the keys typed. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s</param>
        /// <param name="searchCriterion">the element to send them to</param>
        /// <returns>true</returns>
        public bool SendKeysToElement(string keys, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => SendKeysTo(element, keys));

        /// <summary>
        ///     Send a series of keyboard events to an element if the element has a certain type
        ///     This allows for handling potentially unsupported input elements as date, time for which the browser falls back to
        ///     text
        /// </summary>
        /// <param name="keys">the keys to send</param>
        /// <param name="searchCriterion">the element to send them to</param>
        /// <param name="expectedType">the expected type attribute</param>
        /// <returns>true if successful, null if type is not equal to the expected type, false otherwise</returns>
        public object SendKeysToElementIfTypeIs(string keys, string searchCriterion, string expectedType)
        {
            var realType = AttributeOfElement("type", searchCriterion);
            if (expectedType != realType) return null;

            Debug.WriteLine("Type is " + expectedType + ". Sending " + keys);
            return SendKeysToElement(keys, searchCriterion);
        }

        /// <summary>
        ///     Set attribute of an element (via JavaScript)
        /// </summary>
        /// <param name="attribute">Element attribute to set</param>
        /// <param name="searchCriterion">criterion for selecting the element</param>
        /// <param name="attributeValue">Value to be set</param>
        /// <returns>true if successful, false if not</returns>
        public bool SetAttributeOfElementTo(string attribute, string searchCriterion, string attributeValue) =>
            DoOperationOnElement(searchCriterion, element => element.SetAttribute(attribute, attributeValue));

        /// <summary>
        ///     Check an element (i.e. select it if it was not already selected)
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>whether successful or not</returns>
        public bool SetElementChecked(string searchCriterion) => SetElementChecked(searchCriterion, true);

        /// <summary>
        ///     Check or uncheck an element
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <param name="selected">select (true) or deselect (false)</param>
        /// <returns>whether successful or not</returns>
        public bool SetElementChecked(string searchCriterion, bool selected) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                if (element.Selected == selected) return true;

                element.Click();

                return element.Selected == selected;
            });

        /// <summary>
        ///     Sets the value of a certain element.
        ///     note: Also sets query attribute to the element found, so it can be used in submit (deprecated).
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <param name="value">The value it needs to be set to</param>
        /// <returns>whether or not it succeeded (true/false)</returns>
        public bool SetElementTo(string searchCriterion, string value)
        {
            _query = DoOperationOnElement(searchCriterion, element =>
            {
                // clear is needed as existing content needs to be removed.
                try
                {
                    element.Clear();
                }
                catch (InvalidElementStateException)
                {
                    //ignore 
                }
                catch (InvalidOperationException)
                {
                    //ignore
                }

                element.SendKeys(value);
                return element;
            });
            return _query != null;
        }

        /// <summary>
        ///     Uncheck an element (i.e. deselect it if it was not already selected)
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>whether successful or not</returns>
        public bool SetElementUnchecked(string searchCriterion) => SetElementChecked(searchCriterion, false);

        /// <summary>
        ///     Set the text (inner HTML) of an element. Overwrites content or elements that may be in there.
        /// </summary>
        /// <param name="searchCriterion">Criterion to identify the element (method:locator)</param>
        /// <param name="value">value to set</param>
        /// <returns>true if successful, false if not</returns>
        public bool SetTextInElementTo(string searchCriterion, string value) =>
            DoOperationOnElement(searchCriterion, element => element.SetInnerHtml(value));

        /// <summary>
        ///     Submit an element
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>whether or not the operation succeeded</returns>
        public bool SubmitElement(string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                element.Submit();
                return true;
            });

        /// <summary>
        ///     Return the text of an element
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>the text of the element</returns>
        public string TextInElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element => element.Text);

        /// <summary>
        ///     Check if a certain element matches with a regular expression
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <param name="regexPattern">The regex pattern to match the element value with</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException" />
        /// <returns>whether or not the text was a match</returns>
        public bool TextInElementMatches(string searchCriterion, string regexPattern)
        {
            var regex = new Regex(regexPattern);
            return DoOperationOnElement(searchCriterion, element => regex.IsMatch(element.Text));
        }

        /// <summary>
        ///     Upload a file to an element of input type file.
        /// </summary>
        /// <param name="fileName">the file to upload</param>
        /// <param name="searchCriterion">criterion (method:locator) to find the upload element with</param>
        /// <returns></returns>
        public bool UploadFileInElement(string fileName, string searchCriterion)
        {
            var absoluteFileName = Path.GetFullPath(fileName);
            return DoOperationOnElement(searchCriterion, element =>
            {
                element.SendKeys(absoluteFileName);
                return true;
            });
        }

        /// <summary>
        ///     Wait for an element via a certain criterion
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>whether or not the element was found before timeout</returns>
        public bool WaitForElement(string searchCriterion) =>
            WaitFor(drv =>
            {
                try
                {
                    return drv.FindElement(new SearchParser(searchCriterion).By) != null;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

        /// <summary>
        ///     Wait until there isn't an element with a certain criterion
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>true if the element is missing before the timeout, false if still there</returns>
        [Obsolete("Deprecated - use WaitUntilElementDoesNotExist instead")]
        public bool WaitForNoElement(string searchCriterion) =>
            WaitFor(drv =>
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

        /// <summary>
        ///     Wait for element to not exist
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        public bool WaitUntilElementDoesNotExist(string searchCriterion) => WaitFor(drv => !ElementExists(searchCriterion));

        /// <summary>
        ///     Wait for an an element to become clickable
        /// </summary>
        /// <param name="searchCriterion"></param>
        /// <returns>true if element is clickable before timeout, false if not</returns>
        public bool WaitUntilElementIsClickable(string searchCriterion) => WaitFor(drv => ElementIsClickable(searchCriterion)).ToBool();

        /// <summary>
        ///     Wait for element to become invisible
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>true if element is invisible before timeout, false if not</returns>
        public bool WaitUntilElementIsInvisible(string searchCriterion) => WaitFor(drv => !ElementIsVisible(searchCriterion));

        /// <summary>
        ///     Wait for an an element to become not clickable
        /// </summary>
        /// <param name="searchCriterion"></param>
        /// <returns></returns>
        public bool WaitUntilElementIsNotClickable(string searchCriterion) => WaitFor(drv => !ElementIsClickable(searchCriterion)).ToBool();

        /// <summary>
        ///     Wait for element to become visible
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <returns>true if element is visible before timeout, false if not</returns>
        public bool WaitUntilElementIsVisible(string searchCriterion) => WaitFor(drv => ElementIsVisible(searchCriterion));

        private bool WaitUntilIsClickable(IWebElement element) => WaitFor(drv => IsClickable(element)).ToBool();

        /// <summary>
        ///     Wait until the text in an element matches a regular expression
        /// </summary>
        /// <param name="searchCriterion">criterion (method:locator) to find the element with</param>
        /// <param name="regexPattern">regular expression to match element text with</param>
        /// <returns></returns>
        public bool WaitUntilTextInElementMatches(string searchCriterion, string regexPattern) =>
            WaitFor(drv => TextInElementMatches(searchCriterion, regexPattern));
    }
}
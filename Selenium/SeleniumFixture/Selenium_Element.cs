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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture
{
    /// <summary>
    ///     Element handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     Documentation("Selenium WebDriver fixture to support testing HTML interfaces via FitNesse/FitSharp")]
    public sealed partial class Selenium
    {
        private IWebElement _query;

        [Documentation("Default search method for finding elements: ClassName, CssSelector, Id, LinkText, Name, PartialLinkText, Tagname, XPath")]
        public static string DefaultSearchMethod
        {
            get => SearchParser.DefaultMethod;
            set => SearchParser.DefaultMethod = value;
        }

        [Documentation(" Delimiter for method and locator in element searches (default is ':')")]
        public static string SearchDelimiter
        {
            get => SearchParser.Delimiter;
            set => SearchParser.Delimiter = value;
        }

        [Documentation("Return a list of all options in a select element. Method can be value or text")]
        public ReadOnlyCollection<string> AllOptionsOfElementBy(string searchCriterion, string method) =>
            DoOperationOnElement(searchCriterion, element =>
                new ReadOnlyCollection<string>(new SelectElement(element).Options.Select(option => option.GetValueBy(method)).ToList()));

        [Documentation("Return an attribute value of a certain element")]
        public string AttributeOfElement(string attribute, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => element.GetAttribute(attribute));

        private static void ChooseAction(bool choice, Action trueAction, Action falseAction)
        {
            if (choice)
            {
                trueAction();
                return;
            }
            falseAction();
        }

        [Documentation("Empty the contents of an element")]
        public bool ClearElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            element.Clear();
            return true;
        });

        [Documentation("Click a specific element. Returns true or a NoSuchElementException")]
        public bool ClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            MoveTo(element);
            if (!WaitUntilIsClickable(element)) return false;
            element.Click();
            return true;
        });

        [Documentation("Click a specific element if it is visible. Useful for e.g.cookie confirmations")]
        public bool? ClickElementIfVisible(string searchCriterion)
        {
            if (ElementExists(searchCriterion) && ElementIsVisible(searchCriterion)) return ClickElement(searchCriterion);
            return null;
        }

        [Documentation("Click a specific element with a modifier key (Alt, Shift, Control - in Selenium SendKey format)")]
        public bool ClickElementWithModifier(string searchCriterion, string modifier)
        {
            var keys = new KeyConverter(modifier).ToSeleniumFormat;
            return DoOperationOnElement(searchCriterion, element =>
            {
                new Actions(Driver).KeyDown(keys).Click(element).KeyUp(keys).Perform();
                return true;
            });
        }

        [Documentation("Returns the number of elements matching the criteria")]
        public int CountOfElements(string searchCriterion) => ElementsMatching(searchCriterion)?.Count ?? 0;

        [Documentation("Return a CSS property of a certain element (e.g. color)")]
        public string CssPropertyOfElement(string cssProperty, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => element.GetCssValue(cssProperty));

        [Documentation("Deselect an option in a select element (list bos or dropdown). Option method can be text, value, id (default=text)")]
        public bool DeselectOptionInElement(string item, string searchCriterion) => SelectOrDeselectOptionInElement(false, item, searchCriterion);

        private T DoOperationOnElement<T>(string searchCriterion, Func<IWebElement, T> operation)
        {
            if (Driver == null) return default(T);
            // We might get a stale element if the DOM tree happens to change during the operation. In that case, try again.
            StaleElementReferenceException staleElementReferenceException = null;
            for (var attempts = 0; attempts < 2; attempts++)
            {
                try
                {
                    var element = FindElement(searchCriterion);
                    Debug.Assert(element != null);
                    return operation(element);
                }
                catch (StaleElementReferenceException sere)
                {
                    staleElementReferenceException = sere;
                    Thread.Sleep(100);
                }
            }
            throw new StaleElementReferenceException(ErrorMessages.StaleAfterRetry, staleElementReferenceException);
        }

        [Documentation("Double click an element")]
        public bool DoubleClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            MoveTo(element);
            // double click has issues for geckodriver. Firefox version 58 is supposed to fix this issue but did not (for me).
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

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "False positive")]
        [Documentation("Drag an element to an X,Y location. Only works for mobile platforms")]
        public bool DragElementAndDropAt(string dragElementLocator, Coordinate location)
        {
            var dragElement = FindElement(dragElementLocator);
            if (!Driver.IsAndroid() && !Driver.IsIos())
                throw new NotImplementedException(ErrorMessages.NoDragDropToCoordinates);
            if (!(Driver is IPerformsTouchActions driver)) return false;
            var touchAction = new TouchAction(driver);
            Debug.Assert(location != null, nameof(location) + " != null");
            touchAction.LongPress(dragElement).MoveTo(location.X, location.Y).Release().Perform();
            return true;
        }

        // todo: make a simpler version if target not invisible.
        [Documentation("Drag an element and drop it onto another element")]
        public bool DragElementAndDropOnElement(string dragElementLocator, string dropElementLocator)
        {
            var dragElement = FindElement(dragElementLocator);
            // We do the drop element resolution as late as possible since it may only appear during LongPress
            if (Driver.IsAndroid() || Driver.IsIos())
            {
                if (!(Driver is IPerformsTouchActions driver)) return false;
                // first we long press and find the element. It might only show up during longpress
                var checkAction = new TouchAction(driver);
                checkAction.LongPress(dragElement).Perform();
                if (!WaitForElement(dropElementLocator)) throw new NoSuchElementException(ErrorMessages.DropElementNotFound);
                var target = FindElement(dropElementLocator);
                // Now do the actual drag and drop. Not using MoveTo(Element) as element may still be invisible.
                var position = target.Location;
                var size = target.Size;
                var moveX = position.X + size.Width / 2;
                var moveY = position.Y + size.Height / 2;
                checkAction.Release().Perform();
                var action = new TouchAction(driver);
                dragElement = FindElement(dragElementLocator);
                action.LongPress(dragElement).MoveTo(moveX, moveY).Release().Perform();
                return true;
            }
            DragDrop.Html5DragAndDrop(Driver, dragElement, FindElement(dropElementLocator), DragDrop.Position.Center, DragDrop.Position.Center);
            return true;
        }

        [Documentation("Drag an element and drop it onto another element in another driver")]
        public bool DragElementAndDropOnElementInDriver(string dragElementLocator, string dropElementLocator, string dropDriverHandle)
        {
            var dragElement = FindElement(dragElementLocator);
            var dragDriverHandle = DriverId;
            var sourceDriver = Driver;
            SetDriver(dropDriverHandle);
            var targetDriver = Driver;
            var dropElement = FindElement(dropElementLocator);
            SetDriver(dragDriverHandle);
            DragDrop.DragToWindow(sourceDriver, dragElement, targetDriver, dropElement);
            return true;
        }

        [Documentation("Returns whether a certain element exists on the page")]
        public bool ElementExists(string searchCriterion) => Driver != null && ElementsMatching(searchCriterion).Any();

        [Documentation("Returns whether a certain element has the specified attribute")]
        public bool ElementHasAttribute(string searchCriterion, string attribute) => AttributeOfElement(attribute, searchCriterion) != null;

        [Documentation("Returns whether the element is selected/checked")]
        public bool ElementIsChecked(string searchCriterion) => DoOperationOnElement(searchCriterion, element => element.Selected);

        [Documentation("Returns whether a certain element can be clicked (i.e. is enabled and displayed)")]
        public bool ElementIsClickable(string searchCriterion) => DoOperationOnElement(searchCriterion, IsClickable);

        [Documentation("Returns whether a certain element is visible on the page")]
        public bool ElementIsVisible(string searchCriterion) => DoOperationOnElement(searchCriterion, element => element.Displayed);

        private IReadOnlyCollection<IWebElement> ElementsMatching(string searchCriterion) =>
            Driver?.FindElements(new SearchParser(searchCriterion).By);

        public IWebElement FindElement(string searchCriterion) => Driver.FindElement(new SearchParser(searchCriterion).By);

        private static bool IsClickable(IWebElement element) => element.Displayed && element.Enabled;

        private static int? KeyCode(string keyCodeIn)
        {
            // if this is an integer, use it. Otherwise, look if we can convert via an AndroidKeyCode field name
            if (int.TryParse(keyCodeIn, out var keyCode)) return keyCode;
            var myType = typeof(AndroidKeyCode);
            var myFieldInfo = myType.GetField(keyCodeIn, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (myFieldInfo == null) return null;
            var success = int.TryParse(myFieldInfo.GetValue(null).ToString(), out keyCode);
            if (!success) return null;
            return keyCode;
        }

        [Documentation("Long press an element (mobile only)")]
        public bool LongPressElementForSeconds(string searchCriterion, double seconds) => DoOperationOnElement(searchCriterion, element =>
        {
            if (!(Driver is IPerformsTouchActions driver)) return false;
            var action = new TouchAction(driver);
            action.LongPress(element).Wait(Convert.ToInt64(TimeSpan.FromSeconds(seconds).TotalMilliseconds)).Release().Perform();
            return true;
        });

        private void MoveTo(IWebElement element)
        {
            // Android doesn't support these.
            try
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript(
                    "arguments[0].scrollIntoView({behavior: 'instant', block: 'nearest', inline: 'nearest'});", element);
            }
            catch (WebDriverException e) when (e.Message == "Method is not implemented")
            {
                // ignore
            }
            try
            {
                new Actions(Driver).MoveToElement(element).Build().Perform();
            }
            catch (NotImplementedException)
            {
                // ignore
            }
        }

        [Documentation("Move the cursor to a certain element (e.g. for hovering)")]
        public bool MoveToElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            MoveTo(element);
            return true;
        });

        [Documentation("Right click an element (a.k.a. context click)")]
        public bool RightClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            // Does not work right for Chrome. It opens the menu, but does not seem to be able to interact with it.
            // A construct with .ContextClick.SendKeys.Build.Perform doesn't work either.
            new Actions(Driver).MoveToElement(element).ContextClick(element).Build().Perform();
            return true;
        });

        [Documentation("Scroll in a direction until an element is found. Return true if found, false if not." +
                       " Mobile only, uses MoveToElement with browsers (ignoring direction).")]
        public bool ScrollToElement(string direction, string searchCriterion)
        {
            if (!Driver.IsAndroid() && !Driver.IsIos()) return MoveToElement(searchCriterion);

            var contentHash = Driver.PageSource.GetHashCode();
            int oldHash;
            // We allow things like FromTop, from top, FROM top.
            // If that is used, we first scroll up to the top, and then start scrolling down.
            if (Regex.Replace(direction, @"\s+", string.Empty).Equals("fromtop", StringComparison.OrdinalIgnoreCase))
            {
                do
                {
                    oldHash = contentHash;
                    Scroll("up");
                    contentHash = Driver.PageSource.GetHashCode();
                } while (oldHash != contentHash);
                direction = "down";
            }

            do
            {
                oldHash = contentHash;
                if (ElementExists(searchCriterion) && ElementIsVisible(searchCriterion)) return true;
                Scroll(direction);
                contentHash = Driver.PageSource.GetHashCode();
            } while (oldHash != contentHash);
            return false;
        }

        [Documentation("Return the fist selected item text of a listbox(single or multi - value)")]
        public string SelectedOptionInElement(string searchCriterion)
        {
            var list = SelectedOptionsInElement(searchCriterion);
            return list?.FirstOrDefault();
        }

        [Documentation("Returns the selected option for single-select elements, or the first selected option for multi-select elements. " +
                       "method can be value or text")]
        public string SelectedOptionInElementBy(string searchCriterion, string method)
        {
            var list = SelectedOptionsInElementBy(searchCriterion, method);
            return list?.FirstOrDefault();
        }

        [Documentation("Return the selected item texts of a multi-select listbox")]
        public ReadOnlyCollection<string> SelectedOptionsInElement(string searchCriterion) => SelectedOptionsInElementBy(searchCriterion, "text");

        [Documentation("Returns the selected option for single-select elements, or the first selected option for multi-select elements. " +
                       "method can be value or text")]
        public ReadOnlyCollection<string> SelectedOptionsInElementBy(string searchCriterion, string method) =>
            DoOperationOnElement(searchCriterion, element =>
            {
                // we use the item parser to parse the selection option
                return new ReadOnlyCollection<string>(
                    new SelectElement(element).AllSelectedOptions.Select(option => option.GetValueBy(method)).ToList());
            });

        [Documentation("Select an option in a select element. item method can be text, value or id. Default is text")]
        public bool SelectOptionInElement(string item, string searchCriterion) => SelectOrDeselectOptionInElement(true, item, searchCriterion);

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
                        ChooseAction(select, () => { selector.SelectByValue(locator); }, () => { selector.DeselectByValue(locator); });
                        break;
                    case "INDEX":
                        var index = int.Parse(locator, CultureInfo.InvariantCulture);
                        ChooseAction(select, () => { selector.SelectByIndex(index); }, () => { selector.DeselectByIndex(index); });
                        break;
                    default: // includes 'text', and 'id'  in case SearchParser doesn't get a method
                        ChooseAction(select, () => { selector.SelectByText(locator); }, () => { selector.DeselectByText(locator); });
                        break;
                }

                return true;
            });
        }

        [Documentation("Send series of keyboard events to the active element. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s")]
        public bool SendKeys(string keys)
        {
            var element = Driver.SwitchTo().ActiveElement();
            return SendKeysTo(element, keys);
        }

        private static bool SendKeysTo(IWebElement element, string keys)
        {
            var formattedKeys = new KeyConverter(keys).ToSeleniumFormat;
            element.SendKeys(formattedKeys);
            return true;
        }

        [Documentation("Send a series of keyboard events to an element. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s")]
        public bool SendKeysToElement(string keys, string searchCriterion) =>
            DoOperationOnElement(searchCriterion, element => SendKeysTo(element, keys));

        [Documentation("Emulate typing keys into an element, to be used for input elements that are potentially unknown by browsers " +
                       "(e.g. date, time) and for which a fallback to basic text elements may be done. " +
                       "Return true if successful, null if type not recognized")]
        public object SendKeysToElementIfTypeIs(string keys, string searchCriterion, string expectedType)
        {
            var realType = AttributeOfElement("type", searchCriterion);
            if (expectedType != realType) return null;
            return SendKeysToElement(keys, searchCriterion);
        }

        [Documentation("Set an attribute value of a certain element. Uses JavaScript")]
        public bool SetAttributeOfElementTo(string attribute, string searchCriterion, string attributeValue) =>
            DoOperationOnElement(searchCriterion, element => element.SetAttribute(attribute, attributeValue));

        [Documentation("Check an element (i.e. select it if it was not already selected)")]
        public bool SetElementChecked(string searchCriterion) => SetElementChecked(searchCriterion, true);

        [Documentation("Check or uncheck an element (based on second parameter)")]
        public bool SetElementChecked(string searchCriterion, bool selected) => DoOperationOnElement(searchCriterion, element =>
        {
            if (element.Selected == selected) return true;
            element.Click();
            return element.Selected == selected;
        });

        [Documentation("Sets the value of a certain element (via SendKeys)")]
        public bool SetElementTo(string searchCriterion, string value)
        {
            _query = DoOperationOnElement(searchCriterion, element =>
            {
                try
                {
                    element.Clear(); // needed as existing content needs to be removed
                }
                catch (InvalidElementStateException)
                {
                    // ignore
                }
                catch (InvalidOperationException)
                {
                    // ignore
                }

                element.SendKeys(value);
                return element;
            });
            return _query != null;
        }

        [Documentation("Uncheck an element (i.e. deselect it if it was not already selected)")]
        public bool SetElementUnchecked(string searchCriterion) => SetElementChecked(searchCriterion, false);

        [Documentation("Set the innerHTML of a certain element (via JavaScript). Overwrites all content or other elements that may be in there.")]
        public bool SetTextInElementTo(string searchCriterion, string value) =>
            DoOperationOnElement(searchCriterion, element => element.SetInnerHtml(value));

        [Documentation("Submit a form via an element")]
        public bool SubmitElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            element.Submit();
            return true;
        });

        [Documentation("Single tap an element (mobile only)")]
        public bool TapElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            if (!(Driver is IPerformsTouchActions driver)) return false;
            var action = new TouchAction(driver);
            action.Tap(element).Perform();
            return true;
        });

        [Documentation("Return the text of a certain element")]
        public string TextInElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element => element.Text);

        [Documentation("Check if the text in a certain element matches with a regular expression")]
        public bool TextInElementMatches(string searchCriterion, string regexPattern)
        {
            var regex = new Regex(regexPattern);
            return DoOperationOnElement(searchCriterion, element => regex.IsMatch(element.Text));
        }

        [Documentation("Upload a file into an element suited for that")]
        public bool UploadFileInElement(string fileName, string searchCriterion)
        {
            var absoluteFileName = Path.GetFullPath(fileName);
            return DoOperationOnElement(searchCriterion, element =>
            {
                element.SendKeys(absoluteFileName);
                return true;
            });
        }

        private T WaitFor<T>(Func<IWebDriver, T> conditionToWaitFor)
        {
            // Do not mix implicit wait with explicit wait
            var implicitWait = Driver.GetImplicitWait();
            if (Math.Abs(implicitWait) > 0.001) Driver.SetImplicitWait(0);
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
                if (Math.Abs(implicitWait) > 0.001) Driver.SetImplicitWait(implicitWait);
            }
        }

        [Documentation("Waits for an element to be present on the page until timeout")]
        public bool WaitForElement(string searchCriterion) => WaitFor(drv =>
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

        [Documentation("Wait until an element does not exist on the page (e.g. got deleted)")]
        public bool WaitUntilElementDoesNotExist(string searchCriterion) => WaitFor(drv => !ElementExists(searchCriterion));

        [Documentation("Wait until an element is clickable")]
        public bool WaitUntilElementIsClickable(string searchCriterion) => WaitFor(drv => ElementIsClickable(searchCriterion)).ToBool();

        [Documentation("Wait until an element is invisible")]
        public bool WaitUntilElementIsInvisible(string searchCriterion) => WaitFor(drv => !ElementIsVisible(searchCriterion));

        [Documentation("Wait until an element is not clickable (e.g. made read-only)")]
        public bool WaitUntilElementIsNotClickable(string searchCriterion) => WaitFor(drv => !ElementIsClickable(searchCriterion)).ToBool();

        [Documentation("Wait until an element is visible")]
        public bool WaitUntilElementIsVisible(string searchCriterion) => WaitFor(drv => ElementIsVisible(searchCriterion));

        private bool WaitUntilIsClickable(IWebElement element) => WaitFor(drv => IsClickable(element)).ToBool();

        [Documentation("Wait until the text in an element matches the regular expression provided")]
        public bool WaitUntilTextInElementMatches(string searchCriterion, string regexPattern) =>
            WaitFor(drv => TextInElementMatches(searchCriterion, regexPattern));
    }
}
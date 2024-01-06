// Copyright 2015-2024 Rik Essenius
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
public sealed partial class Selenium
{
    private IWebElement _query;

    /// <summary>Default search method for finding elements: ClassName, CssSelector, Id, LinkText, Name, PartialLinkText, TagName, XPath</summary>
    public static string DefaultSearchMethod
    {
        get => SearchParser.DefaultMethod;
        set => SearchParser.DefaultMethod = value;
    }

    /// <summary> Delimiter for method and locator in element searches (default is ':')</summary>
    public static string SearchDelimiter
    {
        get => SearchParser.Delimiter;
        set => SearchParser.Delimiter = value;
    }

    /// <returns>a list of all options in a select element. Method can be value or text</returns>
    public ReadOnlyCollection<string> AllOptionsOfElementBy(string searchCriterion, string method) =>
        DoOperationOnElement(searchCriterion, element =>
            new ReadOnlyCollection<string>(new SelectElement(element).Options
                .Select(option => option.GetValueBy(method)).ToList()));

    /// <returns>an attribute value of a certain element</returns>
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

    /// <summary>Empty the contents of an element</summary>
    public bool ClearElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
    {
        element.Clear();
        return true;
    });

    /// <summary>Click a specific element. Moves to it first, and waits for it to get clickable.</summary>
    /// <returns>true if clicked, false if exists but could not be clicked, NoSuchElementException if it doesn't exist</returns>
    public bool ClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
    {
        MoveTo(element);
        if (!WaitUntilIsClickable(element)) return false;
        element.Click();
        return true;
    });

    /// <summary>Click a specific element without first moving to it or waiting for it to get clickable.</summary>
    /// <returns>true if clicked, NoSuchElementException if it doesn't exist</returns>
    public bool JustClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
    {
        element.Click();
        return true;
    });

    /// <summary>Click a specific element if it is visible. Useful for e.g.cookie confirmations</summary>
    public bool? ClickElementIfVisible(string searchCriterion)
    {
        if (ElementExists(searchCriterion) && ElementIsVisible(searchCriterion))
        {
            return ClickElement(searchCriterion);
        }
        return null;
    }

    /// <summary>Click a specific element with a modifier key (Alt, Shift, Control - in Selenium SendKey format)</summary>
    public bool ClickElementWithModifier(string searchCriterion, string modifier)
    {
        var keys = new KeyConverter(modifier).ToSeleniumFormat;
        return DoOperationOnElement(searchCriterion, element =>
        {
            new Actions(Driver).KeyDown(keys).Click(element).KeyUp(keys).Perform();
            return true;
        });
    }

    /// <returns>the number of elements matching the criteria</returns>
    public int CountOfElements(string searchCriterion) => ElementsMatching(searchCriterion)?.Count ?? 0;

    /// <returns>a CSS property of a certain element (e.g. color)</returns>
    public string CssPropertyOfElement(string cssProperty, string searchCriterion) =>
        DoOperationOnElement(searchCriterion, element => element.GetCssValue(cssProperty));

    /// <summary>Deselect an option in a select element (list bos or dropdown). Option method can be text, value, id (default=text)</summary>
    public bool DeselectOptionInElement(string item, string searchCriterion) =>
        SelectOrDeselectOptionInElement(false, item, searchCriterion);

    private T DoOperationOnElement<T>(string searchCriterion, Func<IWebElement, T> operation)
    {
        if (Driver == null) return default;
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

    /// <summary>Double click an element</summary>
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
        const string script =
            "var myEvent = new MouseEvent('dblclick', {bubbles: true, cancelable: true, view: window});"
            + "arguments[0].dispatchEvent(myEvent);";
        ((IJavaScriptExecutor)Driver).ExecuteScript(script, element);
    }

    /// <summary>Drag an element to an X,Y location. Only works for mobile platforms</summary>
    public bool DragElementAndDropAt(string dragElementLocator, Coordinate location)
    {
        var dragElement = FindElement(dragElementLocator);
        if (!Driver.IsAndroid() && !Driver.IsIos())
        {
            throw new NotImplementedException(ErrorMessages.NoDragDropToCoordinates);
        }

        Debug.Assert(location != null, nameof(location) + " != null");

        var action = new Actions(Driver);
        action.MoveToElement(dragElement).ClickAndHold().Pause(TimeSpan.FromSeconds(0.8)).MoveToLocation(location.X, location.Y).Release().Perform();
        return true;
    }

    private static Point CenterOfElement(IWebElement element)
    {
        var location = element.Location;
        var size = element.Size;
        return new Point(location.X + size.Width / 2, location.Y + size.Height / 2);
    }

    /// <summary>Drag an element and drop it onto another element</summary>
    public bool DragElementAndDropOnElement(string dragElementLocator, string dropElementLocator)
    {
        var dragElement = FindElement(dragElementLocator);
        // We do the drop element resolution as late as possible since it may only appear during LongPress
        // Also, the source may disappear when the drop is done, so we use its coordinates for robustness.
        if (Driver.IsAndroid() || Driver.IsIos())
        {

            // first we long press and find the element. It might only show up during long press
            // An example of that is the remove button in the Android notification bar

            var dragLocation = CenterOfElement(dragElement);
            var checkAction = new Actions(Driver);
            checkAction.MoveToElement(dragElement).ClickAndHold().Perform();
            if (!WaitForElement(dropElementLocator))
            {
                throw new NoSuchElementException(ErrorMessages.DropElementNotFound);
            }
            var dropElement = FindElement(dropElementLocator);
            checkAction
                .MoveToLocation(dragLocation.X, dragLocation.Y)
                .Pause(TimeSpan.FromSeconds(0.5))
                .MoveToElement(dropElement)
                ////.MoveToLocation(dropLocation.X, dropLocation.Y)
                .Release()
                .Perform();
            return true;
        }

        DragDrop.Html5DragAndDrop(
            Driver,
            dragElement,
            FindElement(dropElementLocator),
            DragDrop.Position.Center,
            DragDrop.Position.Center);
        return true;
    }

    /// <summary>Drag an element and drop it onto another element in another driver</summary>
    public bool DragElementAndDropOnElementInDriver(
        string dragElementLocator,
        string dropElementLocator,
        string dropDriverHandle)
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

    /// <returns>whether a certain element exists on the page</returns>
    public bool ElementExists(string searchCriterion) => Driver != null && ElementsMatching(searchCriterion).Any();

    /// <returns>whether a certain element has the specified attribute</returns>
    public bool ElementHasAttribute(string searchCriterion, string attribute) =>
        AttributeOfElement(attribute, searchCriterion) != null;

    /// <returns>whether the element is selected/checked</returns>
    public bool ElementIsChecked(string searchCriterion) =>
        DoOperationOnElement(searchCriterion, element => element.Selected);

    /// <returns>whether a certain element can be clicked (i.e. is enabled and displayed)</returns>
    public bool ElementIsClickable(string searchCriterion) => DoOperationOnElement(searchCriterion, IsClickable);

    /// <returns>whether a certain element is visible on the page</returns>
    public bool ElementIsVisible(string searchCriterion) =>
        DoOperationOnElement(searchCriterion, element => element.Displayed);

    private IReadOnlyCollection<IWebElement> ElementsMatching(string searchCriterion) =>
        Driver?.FindElements(new SearchParser(searchCriterion).By);

    /// <summary>Find a Web element</summary>
    public IWebElement FindElement(string searchCriterion) =>
        Driver.FindElement(new SearchParser(searchCriterion).By);

    private static bool IsClickable(IWebElement element) => element.Displayed && element.Enabled;

    private static int? KeyCode(string keyCodeIn)
    {
        // if this is an integer, use it. Otherwise, look if we can convert via an AndroidKeyCode field name
        if (int.TryParse(keyCodeIn, out var keyCode)) return keyCode;
        var myType = typeof(AndroidKeyCode);
        if (keyCodeIn == null) throw new ArgumentNullException(nameof(keyCodeIn));

        var myFieldInfo =
            myType.GetField(keyCodeIn, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase) ??
            myType.GetField("Keycode_" + keyCodeIn, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (myFieldInfo == null)
        {
            return null;
        }

        var success = int.TryParse(myFieldInfo.GetValue(null)?.ToString(), out keyCode);
        if (!success) return null;
        return keyCode;
    }

    /// <summary>Long press an element (mobile only)</summary>
    public bool LongPressElementForSeconds(string searchCriterion, double seconds) => 
        DoOperationOnElement(searchCriterion, element =>
        {
            var action = new Actions(Driver);
            action.MoveToElement(element).ClickAndHold().Pause(TimeSpan.FromSeconds(seconds)).Release().Perform();
            return true;
        });

    private void MoveTo(IWebElement element)
    {
        // Android and WinApp don't support these.
        try
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView({behavior: 'instant', block: 'nearest', inline: 'nearest'});",
                element);
        }
        catch (NotImplementedException)
        {
            // ignore
        }
        catch (WebDriverException e) when (e.Message.Contains("not implemented"))
        {
            // ignore
        }

        try
        {
            new Actions(Driver).MoveToElement(element).Perform();
        }
        catch (NotImplementedException)
        {
            // ignore
        }
        catch (WebDriverException e) when (e.Message.Contains(
                                               "Currently only pen and touch pointer input source types are supported"))
        {
            //ignore, WinApp peculiarity
        }
    }

    /// <summary>Move the cursor to a certain element (e.g. for hovering)</summary>
    public bool MoveToElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
    {
        MoveTo(element);
        return true;
    });

    /// <summary>Right-click an element (a.k.a. context click)</summary>
    public bool RightClickElement(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
    {
        // Does not work right for Chrome. It opens the menu, but does not seem to be able to interact with it.
        // A construct with .ContextClick.SendKeys.Build.Perform doesn't work either.
        new Actions(Driver).MoveToElement(element).ContextClick(element).Build().Perform();
        return true;
    });

    /// <summary>Scroll in a direction until an element is found. Return true if found, false if not.</summary>
    /// <remarks>Mobile only, uses MoveToElement with browsers (ignoring direction).</remarks>
    public bool ScrollToElement(string direction, string searchCriterion)
    {
        if (!Driver.IsAndroid() && !Driver.IsIos()) return MoveToElement(searchCriterion);

        var contentHash = Driver.PageSource.GetHashCode();
        int oldHash;
        // We allow things like FromTop, from top, FROM top.
        // If that is used, we first scroll up to the top, and then start scrolling down.
        if (Regex.Replace(direction, @"\s+", string.Empty).Equals(@"fromtop", StringComparison.OrdinalIgnoreCase))
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

    /// <returns>the fist selected item text of a listbox(single or multi - value)</returns>
    public string SelectedOptionInElement(string searchCriterion)
    {
        var list = SelectedOptionsInElement(searchCriterion);
        return list?.FirstOrDefault();
    }

    /// <returns>the selected option for single-select elements, or the first selected option for multi-select elements</returns>
    /// <param name="searchCriterion">the element to use</param>
    /// <param name="method">value or text</param>
    public string SelectedOptionInElementBy(string searchCriterion, string method)
    {
        var list = SelectedOptionsInElementBy(searchCriterion, method);
        return list?.FirstOrDefault();
    }

    /// <returns>the selected item texts of a multi-select listbox</returns>
    public ReadOnlyCollection<string> SelectedOptionsInElement(string searchCriterion) =>
        SelectedOptionsInElementBy(searchCriterion, "text");

    /// <returns>the selected option for single-select elements, or the first selected option for multi-select elements</returns>
    /// <param name="searchCriterion">the element to use</param>
    /// <param name="method">value or text</param>
    public ReadOnlyCollection<string> SelectedOptionsInElementBy(string searchCriterion, string method) =>
        DoOperationOnElement(searchCriterion, element =>
        {
            // we use the item parser to parse the selection option
            return new ReadOnlyCollection<string>(
                new SelectElement(element).AllSelectedOptions.Select(option => option.GetValueBy(method)).ToList());
        });

    /// <summary>Select an option in a select element. item method can be text, value or id. Default is text</summary>
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
                    ChooseAction(
                        select, 
                        () => { selector.SelectByValue(locator); },
                        () => { selector.DeselectByValue(locator); });
                    break;
                case "INDEX":
                    var index = int.Parse(locator, CultureInfo.InvariantCulture);
                    ChooseAction(
                        select, 
                        () => { selector.SelectByIndex(index); },
                        () => { selector.DeselectByIndex(index); });
                    break;
                default: // includes 'text', and 'id'  in case SearchParser doesn't get a method
                    ChooseAction(
                        select, 
                        () => { selector.SelectByText(locator); },
                        () => { selector.DeselectByText(locator); });
                    break;
            }

            return true;
        });
    }

    /// <summary>Send series of keyboard events to the active element. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s</summary>
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

    /// <summary>Send a series of keyboard events to an element. Can use + for Shift, ^ for Ctrl, % for alt, and some {SPECIAL}s</summary>
    public bool SendKeysToElement(string keys, string searchCriterion) =>
        DoOperationOnElement(searchCriterion, element => SendKeysTo(element, keys));

    /// <summary>
    ///     Emulate typing keys into an element, to be used for input elements that are potentially unknown by browsers
    ///     (e.g. date, time) and for which a fallback to basic text elements may be done
    /// </summary>
    /// <returns>true if successful, null if type not recognized</returns>
    public object SendKeysToElementIfTypeIs(string keys, string searchCriterion, string expectedType)
    {
        var realType = AttributeOfElement("type", searchCriterion);
        if (expectedType != realType) return null;
        return SendKeysToElement(keys, searchCriterion);
    }

    /// <summary>Set an attribute value of a certain element. Uses JavaScript</summary>
    public bool SetAttributeOfElementTo(string attribute, string searchCriterion, string attributeValue) =>
        DoOperationOnElement(searchCriterion, element => element.SetAttribute(attribute, attributeValue));

    /// <summary>Check an element (i.e. select it if it was not already selected)</summary>
    public bool SetElementChecked(string searchCriterion) => SetElementChecked(searchCriterion, true);

    /// <summary>Check or uncheck an element (based on second parameter)</summary>
    public bool SetElementChecked(string searchCriterion, bool selected) => 
        DoOperationOnElement(searchCriterion, element =>
        {
            if (element.Selected == selected) return true;
            element.Click();
            return element.Selected == selected;
        });

    /// <summary>Sets the value of a certain element (via SendKeys)</summary>
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

            // not using SendKeysTo here as we don't want +% etc. to get interpreted as special keys
            element.SendKeys(value);
            return element;
        });
        return _query != null;
    }

    /// <summary>Uncheck an element (i.e. deselect it if it was not already selected)</summary>
    public bool SetElementUnchecked(string searchCriterion) => SetElementChecked(searchCriterion, false);

    /// <summary>Set the innerHTML of a certain element (via JavaScript). Overwrites all content or other elements that may be in there.</summary>
    public bool SetTextInElementTo(string searchCriterion, string value) =>
        DoOperationOnElement(searchCriterion, element => element.SetInnerHtml(value));

    /// <summary>Submit a form via an element</summary>
    public bool SubmitElement(string searchCriterion) => 
        DoOperationOnElement(searchCriterion, element =>
        {
            element.Submit();
            return true;
        });

    /// <summary>Single tap an element (mobile only)</summary>
    public bool TapElement(string searchCriterion) => 
        DoOperationOnElement(searchCriterion, element =>
        {
            new Actions(Driver).MoveToElement(element).Click().Perform();
            return true;
        });

    /// <returns>the text of a certain element</returns>
    public string TextInElement(string searchCriterion) =>
        DoOperationOnElement(searchCriterion, element => element.Text);

    /// <summary>Check if the text in a certain element matches with a regular expression</summary>
    public bool TextInElementMatches(string searchCriterion, string regexPattern)
    {
        var regex = new Regex(regexPattern);
        return DoOperationOnElement(searchCriterion, element => regex.IsMatch(element.Text));
    }

    /// <summary>Upload a file into an element suited for that</summary>
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
            return default;
        }
        finally
        {
            if (Math.Abs(implicitWait) > 0.001) Driver.SetImplicitWait(implicitWait);
        }
    }

    /// <summary>Waits for an element to be present on the page until timeout</summary>
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

    /// <summary>Wait until an element does not exist on the page (e.g. got deleted)</summary>
    public bool WaitUntilElementDoesNotExist(string searchCriterion) =>
        WaitFor(_ => !ElementExists(searchCriterion));

    /// <summary>Wait until an element is clickable</summary>
    public bool WaitUntilElementIsClickable(string searchCriterion) =>
        WaitFor(_ => ElementIsClickable(searchCriterion)).ToBool();

    /// <summary>Wait until an element is invisible</summary>
    public bool WaitUntilElementIsInvisible(string searchCriterion) =>
        WaitFor(_ => !ElementIsVisible(searchCriterion));

    /// <summary>Wait until an element is not clickable (e.g. made read-only)</summary>
    public bool WaitUntilElementIsNotClickable(string searchCriterion) =>
        WaitFor(_ => !ElementIsClickable(searchCriterion)).ToBool();

    /// <summary>Wait until an element is visible</summary>
    public bool WaitUntilElementIsVisible(string searchCriterion) =>
        WaitFor(_ => ElementIsVisible(searchCriterion));

    private bool WaitUntilIsClickable(IWebElement element) => WaitFor(_ => IsClickable(element)).ToBool();

    /// <summary>Wait until the text in an element matches the regular expression provided</summary>
    public bool WaitUntilTextInElementMatches(string searchCriterion, string regexPattern) =>
        WaitFor(_ => TextInElementMatches(searchCriterion, regexPattern));
}
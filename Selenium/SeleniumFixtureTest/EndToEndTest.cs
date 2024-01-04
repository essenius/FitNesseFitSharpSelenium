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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

// ReSharper disable UnusedMember.Local -- we use reflection to call the test methods

namespace SeleniumFixtureTest
{
    /// <summary>
    ///     This class is the template for tests ran on different browsers, remote or local.
    ///     Since we can't make class initialization and class cleanup data driven, we make different test classes per
    ///     browser configuration, and we use one test method in those classes with a test data source attribute to
    ///     define which tests we are going to run. We use reflection because the test data needs to be static,
    ///     and because it allows simplification: just add a new test method here and it will be added in all browser tests.
    ///     Test methods are defined as private instance methods without parameters or return values, ending in "Test"
    /// </summary>
    /// <remarks>
    ///     It's a bit bigger than I would like, but splitting it in partial classes doesn't make it much better.
    /// </remarks>
    public class EndToEndTest
    {
        #region Support fields and properties

        private const string TestPage = "TestPage";

        private readonly Selenium _selenium = new();

        private string _browser;
        private bool _runningHeadless;
        private int _testsToDo;

        private static string BaseUrl { get; } = AppConfig.Get("TestSite");

        public static Uri CreateTestPageUri() => CreateUri(TestPage);

        public static Uri CreateUri(string page) => new(new Uri(BaseUrl), page);

        public static string RemoteSelenium { get; } = AppConfig.Get("RemoteSelenium");

        #endregion

        #region Test suite administration, called from the unit test classes

        public void TestCleanup()
        {
            // ClassCleanup is only executed after the whole test suite ends, so that would mean the fixture
            // would stay open until the end of the suite if we would put the Close in there.
            _testsToDo--;
            if (_testsToDo == 0)
            {
                _selenium.Close();
            }
        }

        // safety net

        public void ClassCleanup() => _selenium.Close();

        public void ClassInitialize(string browser, bool isRemote, bool useOptions = false)
        {
            _browser = browser;
            _runningHeadless = browser.EndsWith("Headless", StringComparison.OrdinalIgnoreCase);
            try
            {
                if (isRemote)
                {
                    if (useOptions)
                    {
                        var options = Selenium.NewOptionsFor(_browser);
                        Assert.IsTrue(
                            _selenium.SetRemoteBrowserAtAddressWithOptions(_browser, RemoteSelenium, options),
                            "Set Remote Browser with Options");
                    }
                    else
                    {
                        var capabilities = new Dictionary<string, string>
                        {
                            { "testCapability", "testValue" }
                        };
                        Assert.IsTrue(
                            _selenium.SetRemoteBrowserAtAddressWithCapabilities(_browser, RemoteSelenium, capabilities),
                            "Set Remote browser with Capabilities " + _browser);
                    }
                }
                else
                {
                    Assert.IsTrue(_selenium.SetBrowser(_browser), "Set local browser " + _browser);
                }
                Assert.IsTrue(_selenium.Open(CreateTestPageUri()), "Open page");
                Assert.IsTrue(_selenium.WaitUntilTitleMatches("Selenium Fixture Test Page"));
                var width = _selenium.WindowSize.X;
                if (width < 800) width = 800;
                _selenium.WindowSize = new Coordinate(width, _selenium.WindowSize.Y);

                // Some proxies intercept requests and e.g. attempt to sign on. So we need to wait until the real page shows up.
                _selenium.WaitUntilTitleMatches("SeleniumFixtureTestPage");
                _selenium.ResetTimeout();
                _testsToDo = TestMethods().Count();
            }
            catch (StopTestException se)
            {
                var message =
                    $"{_browser} not available for {(isRemote ? "isRemote" : "local")}. Inner exception: {se.InnerException?.Message}";
                MarkSkipped(nameof(ClassInitialize), message);
                Assert.Inconclusive(message);
            }
        }

        private static IEnumerable<MethodInfo> TestMethods() =>
            typeof(EndToEndTest).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name.EndsWith("Test") && x.ReturnType == typeof(void) && x.GetParameters().Length == 0);

        // Data source for the Test Data attribute.
        public static IEnumerable<object[]> TestCases =>
            TestMethods().Select(method => new object[] { method }).ToList();

        #endregion

        #region Support Methods

        private void AssertOptionValues(string locator, IReadOnlyCollection<string> expected, string message)
        {
            var allItems = _selenium.AllOptionsOfElementBy(locator, "text");
            var notExpected = allItems.Except(expected);
            var selected = _selenium.SelectedOptionsInElement(locator);
            foreach (var item in expected)
            {
                Assert.IsTrue(selected.Contains(item), message + ": '" + item + "' selected");
            }
            foreach (var item in notExpected)
            {
                Assert.IsFalse(selected.Contains(item), message + ": '" + item + "' deselected");
            }
        }

        private void CheckStorageFunctioning(string tag)
        {
            Assert.IsNotNull(_selenium.WebStorage, "WebStorage supported");
            Assert.IsTrue(_selenium.ClearWebStorage());
            Assert.AreEqual(0, _selenium.WebStorage.Count, $"{tag}: Item Count == 0 after opening page");
            _selenium.SetInWebStorageTo(@"testkey", @"testvalue");
            Assert.AreEqual(
                @"testvalue",
                _selenium.GetFromWebStorage("tes*ey"),
                $"{tag}: Can retrieve added test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after adding a value");
            _selenium.SetInWebStorageTo(@"testkey", @"testvalue2");
            Assert.AreEqual(
                @"testvalue2",
                _selenium.GetFromWebStorage(@"testkey"),
                $"{tag}: Can retrieve changed test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after changing a value");
            _selenium.SetInWebStorageTo("testkey1", @"testvalue1");
            Assert.AreEqual(
                @"testvalue1",
                _selenium.GetFromWebStorage("testkey1"),
                $"{tag}: Can retrieve 2nd added test value");
            Assert.AreEqual(2, _selenium.WebStorage.Count, $"{tag}: Item Count == 2 after adding 2nd value");
            Assert.AreEqual("testkey1", _selenium.GetKeyLikeFromWebStorage(@"testkey?"));
            _selenium.RemoveFromWebStorage(@"testkey");
            Assert.AreEqual(
                @"testvalue1",
                _selenium.GetFromWebStorage(@"testkey1"),
                $"{tag}: Can retrieve changed test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after removing an item");
        }

        private static bool ExpectNoSuchElementExceptionFor<T>(Func<T> functionToExecute)
        {
            try
            {
                functionToExecute();
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }


        private static bool IsRgb(string input, int r, int g, int b)
        {
            var rgba = $"rgba({r}, {g}, {b}, 1)";
            var rgb = $"rgb({r}, {g}, {b})";
            return input.Equals(rgba) || input.Equals(rgb);
        }

        private static void MarkSkipped(string key, string value) => Debug.WriteLine("Skipped {0}: {1}", key, value);

        public bool ProtectedModesAreEqual() => _selenium.ProtectedModesAre("EQUAL");

        private void VerifySendKeysToElementWithFallback(
            string keys, string element, string typeAttribute, string fallbackKeys)
        {
            _ = _selenium.SendKeysToElementIfTypeIs(keys, element, typeAttribute);
            _selenium.SendKeysToElementIfTypeIs("^a^{DEL}" + fallbackKeys, element, "text");
            Assert.AreEqual(fallbackKeys, _selenium.AttributeOfElement("value", element),
                "SendKeys value is correct for '{0}'", element);
        }

        private void VerifySetElementTo(string element, string value)
        {
            Assert.IsTrue(_selenium.SetElementTo(element, value), "Set '{0}' to '{1}'", element, value);
            Assert.AreEqual(
                value,
                _selenium.AttributeOfElement("value", element),
                "value of '{0}' is '{1}", element, value);
        }

        #endregion

        #region Test Cases

        private void AlertTest()
        {
            _selenium.SetTimeoutSeconds(3);
            var ok = _selenium.WaitForTextIgnoringCase("data load completed");
            Assert.IsTrue(ok, "Wait for data load completed");
            Assert.IsFalse(_selenium.AlertIsPresent());
            Assert.IsTrue(_selenium.ElementExists("trial:alertButton"));
            _selenium.ClickElement("content:Alert");
            Assert.IsTrue(_selenium.AlertIsPresent(), "Alert present");
            Assert.IsTrue(_selenium.DismissAlert(), "Dismissed alert");
            Assert.IsFalse(_selenium.AlertIsPresent(), "Alert not present");

            _selenium.ClickElement("id:alertButton");
            Assert.IsTrue(_selenium.AlertIsPresent());
            Assert.IsTrue(_selenium.AcceptAlert(), "Accept alert");
            Assert.IsFalse(_selenium.AlertIsPresent(), "Alert not present after accept");
            Assert.IsTrue(_selenium.ElementExists("id:alertButton"), "Alert button exists");
            _selenium.ClickElement("id:confirmButton");
            Assert.IsTrue(_selenium.AlertIsPresent(), "Confirm alert present");
            Assert.IsTrue(_selenium.AcceptAlert(), "Accept alert (2)");
            Assert.AreEqual("You pressed OK", _selenium.TextInElement("status"), "Pressed OK");

            _selenium.ClickElement(@"partialcontent:Confir");
            Assert.IsTrue(_selenium.DismissAlert(), "Dismiss alert");
            Assert.AreEqual("You pressed Cancel", _selenium.TextInElement("status"), "Dismiss succeeded");

            _selenium.ClickElement("Content:Prompt");
            Assert.IsTrue(_selenium.AlertIsPresent(), "Prompt alert present");
            _selenium.AcceptAlert();
            Assert.AreEqual("You returned: sure", _selenium.TextInElement("status"), "Prompt alert accepted");

            _selenium.ClickElement("trial:Prompt");
            _selenium.DismissAlert();
            Assert.AreEqual("You pressed Cancel", _selenium.TextInElement("status"), "Dismiss prompt alert succeeded");

            _selenium.ClickElement("id:promptButton");
            Selenium.WaitSeconds(0.5);
            Assert.IsTrue(_selenium.AlertIsPresent(), "Prompt alert is present (2)");
            _ = _selenium.RespondToAlert(@"naah");
            Assert.AreEqual(@"You returned: naah", _selenium.TextInElement("status"));
        }

        private void AsyncTest()
        {
            // source delays 1 second, so 1.1 should be enough
            _selenium.SetTimeoutSeconds(1.1);
            Assert.IsTrue(_selenium.ReloadPage(), "reload page");
            Assert.IsTrue(_selenium.WaitForPageToLoad(), "Waiting for page load");
            var source1 = _selenium.PageSource;

            Assert.IsTrue(source1.Contains("<div id=\"divAsyncLoad\">Loading...</div>"), "Loading...");
            Assert.IsTrue(_selenium.WaitForPageSourceToChange(), "wait for page source to change");
            var source2 = _selenium.PageSource;
            Assert.AreNotEqual(source1, source2, "Sources are not equal after wait for change");
            Assert.AreEqual(
                "0,1,1,2,3,5,8,13,21,34,55,89",
                _selenium.TextInElement("divAsyncLoad"),
                "output element has the expected values");
        }

        private void AttributeOfElementTest()
        {
            Assert.AreEqual(CreateUri("iframe1.html").ToString(), _selenium.AttributeOfElement("src", "id:iframe1"));
            var originalClass = _selenium.AttributeOfElement("class", "status");
            Assert.IsTrue(_selenium.SetAttributeOfElementTo("class", "status", "fail"), "Set class to fail");
            Assert.IsTrue(
                IsRgb(_selenium.CssPropertyOfElement("background-color", "status"), 204, 0, 0),
                "Color of fail class is red");
            Assert.IsTrue(_selenium.SetAttributeOfElementTo("class", "status", "success"), "Set class to success");
            Assert.IsTrue(
                IsRgb(_selenium.CssPropertyOfElement("background-color", "status"), 0, 204, 0),
                "Color of success class is green");
            Assert.IsTrue(_selenium.SetAttributeOfElementTo("class", "status", originalClass));
        }

        private void CheckBrokenImageTest()
        {
            Assert.IsFalse(
                _selenium.ExecuteScript(
                        "return brokenImage.naturalWidth!=\"undefined\" && brokenImage.naturalWidth>0;")
                    .ToBool(),
                "Test broken image");
            Assert.IsTrue(
                _selenium.ExecuteScript(
                        "return dragSource.naturalWidth!=\"undefined\" && dragSource.naturalWidth>0;")
                    .ToBool(),
                "Test OK image");
            Assert.IsTrue(_selenium.ElementHasAttribute("id:brokenImage", "Alt"), "Alt attribute exists");
        }

        private void ClearTest()
        {
            var originalValue = _selenium.AttributeOfElement("value", "Label:Text 1");
            Assert.IsTrue(_selenium.ClearElement("text1"));
            Assert.IsTrue(string.IsNullOrEmpty(_selenium.AttributeOfElement("value", "id:text1")));
            _selenium.SetElementTo("text1", originalValue);
        }

        private void ClickElementIfVisibleTest()
        {
            var clickIfVisible = _selenium.ClickElementIfVisible("am");
            Assert.IsTrue(clickIfVisible.HasValue && clickIfVisible.Value);
            Assert.IsNull(_selenium.ClickElementIfVisible(@"nonexisting"));
        }

        private void ClickElementTest()
        {
            Assert.IsTrue(_selenium.ClickElement("am"));
            Assert.IsTrue(_selenium.ElementIsChecked("am"), "am is checked");
            Assert.IsFalse(_selenium.ElementIsChecked("fm"), "fm is not checked");
            Assert.IsTrue(_selenium.ElementIsChecked("checkbox"), "checkbox is checked 1");
            Assert.IsTrue(_selenium.ClickElement("checkbox"), "clicked checkbox 1");
            Assert.IsFalse(_selenium.ElementIsChecked("checkbox"), "checkbox is not checked");
            Assert.IsTrue(_selenium.ClickElement("checkbox"), "clicked checkbox 2");
            Assert.IsTrue(_selenium.ElementIsChecked("checkbox"), "checkbox is checked 2");
        }

        private void ClickElementWithModifierTest()
        {
            var driver = _selenium.Driver;
            if (driver.IsIe())
            {
                MarkSkipped(nameof(ClickElementWithModifierTest),
                    "Bug in IE. See https://github.com/seleniumhq/selenium-google-code-issue-archive/issues/3425");
                return;
            }

            AssertOptionValues("id:multi-select", new Collection<string>(), "all deselected at start");
            Assert.IsTrue(_selenium.ClickElement("CssSelector:#multi-select option:nth-child(2)"), "click item 2");
            AssertOptionValues("id:multi-select", new Collection<string> { "item 2" }, "item 2 selected");
            Assert.IsTrue(
                _selenium.ClickElementWithModifier("CssSelector:#multi-select option:nth-child(3)", "+"),
                "Shift click item 3");
            AssertOptionValues(
                "id:multi-select",
                new Collection<string> { "item 2", "item 3" },
                "items 2 and 3 selected");
            _ = _selenium.DeselectOptionInElement("item 2", "id:multi-select");
            AssertOptionValues("id:multi-select", new Collection<string> { "item 3" }, "Only item 3 selected");
            _selenium.DeselectOptionInElement("item 3", "id:multi-select");
            AssertOptionValues("id:multi-select", new Collection<string>(), "all deselected at end");
        }

        private void DoubleClickElementTest()
        {
            // there are issues with the Marionette driver, so may fail for Firefox
            var content = _selenium.TextInElement("id:paragraph");
            Assert.IsTrue(_selenium.DoubleClickElement("id:paragraph"));
            Assert.AreNotEqual(content, _selenium.TextInElement("id:paragraph"));
        }

        private void DragDropAcrossWindowsTest()
        {
            _selenium.ReloadPage();
            Assert.IsTrue(_selenium.WaitForElement("dragSource"), "Wait for DragSource in current (target) browser");
            Assert.IsFalse(
                _selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"),
                "Source not dropped in target in driver 1");
            var targetHandle = _selenium.DriverId;
            var sourceHandle = _selenium.NewBrowser("chrome");
            try
            {
                _selenium.Open(CreateTestPageUri());
                Assert.IsTrue(_selenium.WaitForElement("dragSource"), "Wait for DragSource in source browser");
                Assert.IsFalse(
                    _selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"),
                    "Source not dropped in target in driver 2");
                _ = _selenium.DragElementAndDropOnElementInDriver("dragSource", "dropTarget", targetHandle);
                Assert.IsFalse(
                    _selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"),
                    "Source not dropped in target in driver 2");
                Assert.IsTrue(_selenium.SetDriver(targetHandle), "Switch back to original driver");
                Assert.IsTrue(
                    _selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"),
                    "Source was dropped in target in driver 1");
            }
            finally
            {
                _selenium.SetDriver(sourceHandle);
                Assert.IsTrue(_selenium.Close(), "Close source browser");
                _selenium.SetDriver(targetHandle);
            }
        }

        private void DragDropTest()
        {
            _selenium.ReloadPage();
            Assert.IsTrue(
                _selenium.WaitForElement("CssSelector: section > #dragSource"),
                "Wait for dragSource on section level 1");
            Assert.IsTrue(
                _selenium.DragElementAndDropOnElement("dragSource", "dropTarget"),
                "drag/drop dragSource to dropTarget");
            Assert.IsTrue(
                _selenium.ElementExists("CssSelector: div#dropTarget >  #dragSource"),
                "check if dragSource is now in dropTarget");
            Assert.IsFalse(
                _selenium.ElementExists("CssSelector: section > #dragSource"),
                "check if dragSource is no longer on section level");
            Assert.IsTrue(_selenium.ReloadPage(), "Reload page");
            Assert.IsTrue(
                _selenium.WaitForElement("CssSelector: section > #dragSource"),
                "Wait for dragSource on section level 2");
            try
            {
                _selenium.DragElementAndDropAt("dragSource", new Coordinate(150, 730));
                Assert.Fail("No exception thrown");
            }
            catch (NotImplementedException)
            {
                // pass
            }
        }

        private void ElementIsClickableTest()
        {
            Assert.IsTrue(_selenium.ElementIsClickable("button"), "Button is clickable");
            Assert.IsTrue(_selenium.ElementIsClickable("dragSource"), "Image is clickable");
            Assert.IsFalse(_selenium.ElementIsClickable("id:disabledButton"), "Disabled button is not clickable");

            Assert.IsTrue(_selenium.ClickElement("toggleDisabledButton"), "Toggle disable button 1");
            Assert.IsTrue(
                _selenium.WaitUntilElementIsClickable("id:disabledButton"),
                "Wait until disabled button is clickable");
            Assert.IsTrue(_selenium.ClickElement("toggleDisabledButton"), "Toggle disable button 2");
            Assert.IsTrue(
                _selenium.WaitUntilElementIsNotClickable("id:disabledButton"),
                "Wait until disabled button is not clickable");
        }

        private void ElementIsVisibleTest()
        {
            Assert.IsFalse(_selenium.ElementIsVisible("hiddenButton"));
            Assert.IsTrue(_selenium.ClickElement("toggleHideButton"), "Toggle hide button 1");
            Assert.IsTrue(_selenium.WaitUntilElementIsVisible("hiddenButton"), "Wait until hidden button is visible");
            Assert.IsTrue(_selenium.ClickElement("toggleHideButton"), "Toggle hide button 2");
            Assert.IsTrue(_selenium.WaitUntilElementIsInvisible("hiddenButton"), "Wait until hidden button is hidden");
        }

        private void ExecuteAsyncJavaScriptTest() =>
            Assert.IsTrue(
                (bool)_selenium.ExecuteAsyncScript("var callback = arguments[arguments.length - 1];callback(true);"));

        private void FrameTest()
        {
            Assert.IsTrue(_selenium.SwitchToFrame("iframe1"), "Switch to iframe1");
            Assert.IsTrue(_selenium.WaitForElement("iframe2"), "Wait for to iframe2");
            Assert.IsTrue(_selenium.SwitchToFrame("iframe2"), "Switch to iframe2");
            Assert.AreEqual("Test Link 1", _selenium.TextInElement("CssSelector: a.link"), "Check for Test Link 1");
            Assert.IsTrue(_selenium.SwitchToDefaultContext(), "Switch to default context");
            Assert.IsFalse(_selenium.ElementExists("CssSelector: a.link"), "check if link is gone");
            Assert.IsTrue(_selenium.TextExistsIgnoringCase("Nested Frames"), "Check if text 'Nested Frames' exists.");
        }

        private void JavaScriptTableTest()
        {
            Assert.IsTrue(_selenium.TextExists("Input should be 0 or positive"), "invalid input");
            Assert.IsTrue(_selenium.TextExists("Overflow"), "Overflow");
        }

        private void LengthOfPageSourceTest()
        {
            Assert.IsTrue(_selenium.ReloadPage(), "Reload page");
            Assert.IsTrue(_selenium.WaitForPageToLoad(), "Wait for page to load");

            // only needed for IE, but does not harm the others
            var initialLength = _selenium.LengthOfPageSource;
            Assert.IsTrue(_selenium.WaitUntilPageSourceIsLargerThan(initialLength), "Wait until HTML became larger");
            Assert.IsTrue(
                _selenium.TextInElementMatches("divAsyncLoad", "0,1,1,2"),
                "Check if output contains 0,1,1,2");
        }

        private void MoveToElementTest()
        {
            _selenium.SetTimeoutSeconds(2);
            _selenium.SetTextInElementTo("status", "Before hover");
            Assert.IsTrue(_selenium.MoveToElement(@"imageLightbulb"), "Move to image");
            Assert.IsTrue(_selenium.ElementIsVisible(@"imageLightbulb"), "Light bulb is visible");

            var status = _selenium.TextInElement("status");
            // usually status is 'Hovering..", but Chrome may be too fast
            var statusOk = status.Equals("Hovering over image") || status.Equals("OK");
            Assert.IsTrue(statusOk, "Status is 'Hovering' or 'OK'");
            Assert.IsTrue(
                _selenium.ScrollToElement("ignored", @"paragraphLightbulb"),
                "Scroll to paragraph (uses Move under the hood");
            Assert.IsTrue(_selenium.WaitUntilTextInElementMatches("status", ""), "Wait until text in status is empty");
        }

        private void NonExistingElementsTest()
        {
            Assert.IsTrue(
                ExpectNoSuchElementExceptionFor(() => _selenium.ClickElement("NonExistingElement")),
                "ClickElement");
            Assert.IsTrue(
                ExpectNoSuchElementExceptionFor(() => _selenium.MoveToElement("NonExistingElement")),
                "MoveToElement");
            Assert.IsTrue(
                ExpectNoSuchElementExceptionFor(() => _selenium.SubmitElement("NonExistingElement")),
                "SubmitElement");
            Assert.IsTrue(
                ExpectNoSuchElementExceptionFor(() => _selenium.TextInElement("NonExistingElement")),
                "TextInElement");
            Assert.IsTrue(
                ExpectNoSuchElementExceptionFor(() => _selenium.UploadFileInElement("zip", "nonExistingElement")),
                "UploadFileInElement");
        }

        private void NonExistingWindowTest()
        {
            try
            {
                _ = _selenium.SelectWindow("NonExistingWindow");
                Assert.Fail("NoSuchWindowException not thrown");
            }
            catch (NoSuchWindowException)
            {
                // pass
            }
            _selenium.SelectWindow(string.Empty);
        }

        private void ScreenshotTest()
        {
            // select a non-text element so we don't get a blinking cursor. 
            _selenium.ClickElement("id:fm");
            var shot1 = Selenium.Screenshot();
            var shotObject = Selenium.ScreenshotObject();
            Assert.IsTrue(
                Regex.IsMatch(shot1,
                    "<img alt=\\\"Screenshot\\\" src=\\\"data:image\\/png;base64,(\\S+)\\s\\/>"));
            Assert.IsTrue(shot1.Length > 100);
            Assert.AreEqual(shot1, shotObject.Rendering);
            Assert.AreEqual("Screenshot", shotObject.ToString(), "Alt and ToString() OK");
        }

        private void ScriptWithNullParametersTest()
        {
            Assert.IsTrue(
                _selenium.ExecuteScriptWithParameters("return true;", null).ToBool(),
                "Execute script with null param");

            var elementType =
                _selenium.ExecuteScript(
                    "var i=document.createElement('input');i.setAttribute('type','month');return i.type;");
            Assert.IsTrue(elementType.Equals("text") || elementType.Equals("month"));
        }

        private void ScriptWithParametersTest()
        {
            var originalDivText = _selenium.TextInElement("id:div");
            var originalParagraphText = _selenium.TextInElement("id:paragraph");
            var parameters = new Collection<string>
            {
                "id:div",
                "New div text",
                @"xpath://*[contains(@id, 'paragr')]",
                "Replaced paragraph text"
            };
            Assert.IsTrue(
                _selenium.ExecuteScriptWithParameters(
                        "arguments[0].innerHTML = arguments[1];arguments[2].innerHTML = arguments[3];return true;",
                        parameters)
                    .ToBool(), "Execute script");
            Assert.AreEqual("New div text", _selenium.TextInElement("id:div"), "Replace div text");
            Assert.AreEqual(
                "Replaced paragraph text", _selenium.TextInElement("id:paragraph"),
                "Replace paragraph text");
            Assert.IsTrue(_selenium.SetTextInElementTo("id:div", originalDivText), "Set original div text back");
            Assert.IsTrue(
                _selenium.SetTextInElementTo("id:paragraph", originalParagraphText),
                "Set original paragraph text back");
        }

        private void ScriptWithPlainParametersTest()
        {
            var driver = _selenium.Driver;
            var parameters = new Collection<object> { driver.FindElement(By.Id("dragSource")) };
            Assert.IsTrue(
                _selenium.ExecuteScriptWithPlainParameters(
                        "return arguments[0].naturalWidth!=\"undefined\" && arguments[0].naturalWidth>0;", parameters)
                    .ToBool(),
                "image is not broken");
        }

        private void SelectDropDownElementTest()
        {
            // taking default methods for options, which should use text (via id)
            _ = _selenium.SelectOptionInElement("value:item1", "id:dropdown");
            Assert.AreEqual("item1", _selenium.AttributeOfElement("value", "dropdown"));

            AssertOptionValues("dropdown", new Collection<string> { "item 1" }, "dropdown test 1");
            _selenium.SelectOptionInElement("index:2", "dropdown");
            AssertOptionValues("dropdown", new Collection<string> { "item 3" }, "dropdown test 2");
            Assert.AreEqual("item3", _selenium.AttributeOfElement("value", "dropdown"));
            _selenium.SelectOptionInElement("text:item", "dropdown");
            Assert.AreEqual("item0", _selenium.AttributeOfElement("value", "dropdown"));
            AssertOptionValues("dropdown", new Collection<string> { "item" }, "dropdown test 3");

            var allSingleValueListboxOptions = _selenium.AllOptionsOfElementBy("id:dropdown", "value");
            var expected = new Collection<string> { "item1", "item2", "item3", "item4", "item5", "item0" };
            Assert.IsTrue(
                allSingleValueListboxOptions.All(s => expected.Contains(s)),
                "expected options contains all actual options - value");
            Assert.IsTrue(
                expected.All(s => allSingleValueListboxOptions.Contains(s)),
                "actual options contains all expected options - value");
        }

        private void SelectByLabelTest()
        {
            Assert.AreEqual(
                "7",
                _selenium.AttributeOfElement("value", "label:Meter:"),
                "Select by label before element, specifying colon");
            Assert.AreEqual(
                "32",
                _selenium.AttributeOfElement("value", "label:Progress:"),
                "Select by parent label ignoring colon");
            Assert.IsTrue(
                _selenium.TextInElement("label:Text Area").StartsWith("Sample text area"),
                "Select by label after element, without specifying colon");
        }

        private void SelectByPartialContentTest()
        {
            Assert.IsTrue(
                _selenium.TextInElement("PartialContent:allows for checking").StartsWith("Sample text area"),
                "Select text area by partial content");
            Assert.AreEqual(
                "7",
                _selenium.AttributeOfElement("value", @"partialcontent:7 of"),
                "Select by partial content");
        }

        private void SelectMultiElementTest()
        {
            // taking default methods for options, which should use text (via id)
            _selenium.SelectOptionInElement("item 1", "label:Multi Select");
            _selenium.SelectOptionInElement("item 3", "id:multi-select");
            _selenium.SelectOptionInElement("item 5", "id:multi-select");

            AssertOptionValues(
                "id:multi-select",
                new Collection<string> { "item 1", "item 3", "item 5" },
                "first test");
            _selenium.SelectOptionInElement("item", "id:multi-select");
            AssertOptionValues(
                "id:multi-select",
                new Collection<string> { "item 1", "item 3", "item 5", "item" },
                "second test");
            _selenium.DeselectOptionInElement("item 1", "id:multi-select");
            AssertOptionValues("id:multi-select", new Collection<string> { "item 3", "item 5", "item" }, "third test");
            _selenium.DeselectOptionInElement("item 1", "id:multi-select");
            AssertOptionValues("id:multi-select", new Collection<string> { "item 3", "item 5", "item" }, "fourth test");
            _selenium.SelectOptionInElement("item", "id:multi-select");
            AssertOptionValues("id:multi-select", new Collection<string> { "item 3", "item 5", "item" }, "fifth test");
        }

        private void SelectSingleElementTest()
        {
            _selenium.SelectOptionInElement("text:item", "id:single-select");
            Assert.AreEqual("item", _selenium.AttributeOfElement("value", "id:single-select"));
            _selenium.SelectOptionInElement("text:item 1", "id:single-select");
            Assert.AreEqual("item 1", _selenium.AttributeOfElement("value", "id:single-select"));
            AssertOptionValues("id:single-select", new Collection<string> { "item 1" }, "single select test");
            Assert.AreEqual("item 1", _selenium.SelectedOptionInElement("id:single-select"), "SelectedOptionInElement");
            Assert.AreEqual(
                "item 1",
                _selenium.SelectedOptionInElementBy("id:single-select", "text"),
                "SelectedOptionInElementBy");

            var allSingleValueListboxOptions = _selenium.AllOptionsOfElementBy("id:single-select", "text");
            var expected = new Collection<string> { "item 1", "item 2", "item 3", "item 4", "item 5", "item" };
            Assert.IsTrue(
                allSingleValueListboxOptions.All(s => expected.Contains(s)),
                "expected options contains all actual options - text");
            Assert.IsTrue(
                expected.All(s => allSingleValueListboxOptions.Contains(s)),
                "actual options contains all expected options - text");
        }

        private void SendKeysToElementTest()
        {
            // Chrome Headless does not seem to like to copy (^c fails) -- seems fixed now.
            var originalValue1 = _selenium.AttributeOfElement("value", "label:Text 1");
            var originalValue2 = _selenium.AttributeOfElement("value", "text2");
            Assert.IsTrue(_selenium.SendKeysToElement("^ac^{DEL}New Value", "text1"));
            Assert.IsTrue(_selenium.SendKeysToElement("^a^{DEL}", "text2"));
            Assert.IsTrue(_selenium.SendKeysToElement(originalValue1, "text2"));
            Assert.AreEqual("New Value", _selenium.AttributeOfElement("value", "text1"));
            Assert.AreEqual(originalValue1, _selenium.AttributeOfElement("value", "text2"));
            Assert.IsTrue(_selenium.SendKeysToElement("^zz^", "text1"), "Send undo to text1");
            Assert.IsTrue(_selenium.SendKeysToElement("^zz^", "text2"), "Send unto to text2");
            Assert.AreEqual(originalValue1, _selenium.AttributeOfElement("value", "text1"), "Undo for text1 succeeded");
            Assert.AreEqual(originalValue2, _selenium.AttributeOfElement("value", "text2"), "Undo for text2 succeeded");
            Assert.IsTrue(_selenium.SendKeysToElement("^a^{DEL}+abc+def+ghi", "text1"), "Send Keys with shift toggles");
            Assert.AreEqual(@"ABCdefGHI", _selenium.AttributeOfElement("value", "text1"), "Shift toggling worked");
            Assert.IsTrue(_selenium.SendKeysToElement("^ac", "text1"), "Send Select All-Copy All to text1");
            Assert.IsTrue(_selenium.MoveToElement("text2"), "Move to text2");
            _selenium.ExecuteScriptWithParameters(
                "document.getElementById(arguments[0]).focus(); ",
                new Collection<string> { "text2" });
            Assert.IsTrue(_selenium.SendKeys("^av"), "Send Select All-Paste to text2");
            Assert.AreEqual(@"ABCdefGHI", _selenium.AttributeOfElement("value", "text1"), "text 1 didn't change");
            Assert.AreEqual(
                @"ABCdefGHI", _selenium.AttributeOfElement("value", "text2"),
                "text 2 now contains the value of text 1");

            // Initially the test had "-+" instead of "+_" to test whether shift"-" is seen as "_" but that fails in Firefox. 
            // This looks like a bug in the Firefox driver.
            Assert.IsTrue(
                _selenium.SendKeysToElement("{END}+{LEFT}{LEFT}{LEFT}+_-^v", "text1"),
                "Type _- over the last 3 characters in text 1 and paste");
            Assert.AreEqual(
                @"ABCdef_-ABCdefGHI", _selenium.AttributeOfElement("value", "text1"),
                "check final value of test1");
        }

        private void SetElementCheckedTest()
        {
            Assert.IsTrue(_selenium.SetElementChecked("am"), "check am 1");
            Assert.IsTrue(_selenium.ElementHasAttribute("am", "checked"));
            Assert.IsTrue(_selenium.ElementIsChecked("am"), "am is checked");
            Assert.IsFalse(_selenium.ElementIsChecked("fm"), "fm is not checked");
            Assert.IsTrue(_selenium.SetElementChecked("am", true), "check am 2");
            Assert.IsTrue(_selenium.ElementIsChecked("am"), "am is checked");
            Assert.IsTrue(_selenium.ElementIsChecked("checkbox"), "checkbox is checked");
            Assert.IsTrue(_selenium.SetElementChecked("checkbox"), "check checkbox 1");
            Assert.IsTrue(_selenium.ElementIsChecked("checkbox"), "checkbox is checked");
            Assert.IsTrue(_selenium.SetElementUnchecked("checkbox"), "uncheck checkbox 2");
            Assert.IsFalse(_selenium.ElementIsChecked("checkbox"), "checkbox is unchecked");
            Assert.IsTrue(_selenium.SetElementChecked("checkbox"), "check checkbox 3");
            Assert.IsTrue(_selenium.ElementIsChecked("checkbox"), "checkbox is checked");
        }

        private void SetInputTest()
        {
            VerifySetElementTo("email", "rik@home.net");
            VerifySetElementTo("url", "http://fitnesse.org");
            VerifySetElementTo("tel", "+31234567890");
            VerifySetElementTo("height", "1.58");
            VerifySetElementTo("fibonacci", "13");
            VerifySetElementTo("search", "_selenium");
        }

        private void SetNewInputTypesTest()
        {
            // This test is dependent on the date settings of the machine it runs on
            VerifySendKeysToElementWithFallback("{RIGHT 100}{TAB}", "skill", "range", "100");
            VerifySendKeysToElementWithFallback("{LEFT 25}{TAB}", "skill", "range", "75");
            VerifySendKeysToElementWithFallback("3{RIGHT}2004{TAB}", "month", "month", "2004-03");
            VerifySendKeysToElementWithFallback("472014", "week", "week", "2014-W47");
            if (_selenium.Driver.IsFirefox())
            {
                VerifySendKeysToElementWithFallback("2014-07-02", "date", "date", "2014-07-02");
                VerifySendKeysToElementWithFallback("01:23", "time", "time", "01:23");
                VerifySendKeysToElementWithFallback(
                    "09092014{RIGHT}0123A", 
                    "datetime-local", 
                    "datetime-local",
                    "2014-09-09T01:23");
            }
            else
            {
                VerifySendKeysToElementWithFallback("0207{RIGHT}2014", "date", "date", "2014-07-02");
                VerifySendKeysToElementWithFallback("{RIGHT 4}{LEFT}0123", "time", "time", "01:23");
                VerifySendKeysToElementWithFallback(
                    "2409{RIGHT}2014{RIGHT}0123",
                    "datetime-local",
                    "datetime-local",
                    "2014-09-24T01:23");
            }

            // The color picker is a pain since it opens a system dialog which Selenium can't get to
            // So we 'cheat' here and directly set the value. We don't have to test the color picker.

            _selenium.SetAttributeOfElementTo("value", "color", "#ff7f00");
            Assert.AreEqual("#ff7f00", _selenium.AttributeOfElement("value", "color"));
        }

        private void StorageTest()
        {
            // setting storage type to Local by default
            CheckStorageFunctioning("Local");
            _selenium.SetInWebStorageTo("testkey2", @"testvalue3");
            var dict = new Dictionary<string, string>();
            Assert.AreEqual(2, _selenium.WebStorage.Count, "Item Count == 2 after adding SetInWebStorageTo");
            dict.Add("testkey4", @"testvalue5");
            _selenium.AddToWebStorage(dict);
            Assert.AreEqual(
                3,
                _selenium.WebStorage.Count,
                "Item Count == 3 after AddToWebStorage, before switching to Session");
            _selenium.UseWebStorage(StorageType.Session);
            CheckStorageFunctioning("Session");
            Assert.AreEqual(1, _selenium.WebStorage.Count, "Item Count after adding one item to session storage");
            _selenium.UseWebStorage(StorageType.Local);
            Assert.AreEqual(3, _selenium.WebStorage.Count, "Item Count after switching back to local storage");
            Assert.IsTrue(_selenium.ClearWebStorage(), "Can clear local storage");
            Assert.AreEqual(0, _selenium.WebStorage.Count, "Item Count after clearing local storage");
            _selenium.UseWebStorage(StorageType.Session);
            Assert.AreEqual(1, _selenium.WebStorage.Count, "Item Count after switching back to session storage");
            var backupStorage = _selenium.WebStorage;
            Assert.IsTrue(_selenium.ClearWebStorage(), "Can clear session storage");
            Assert.AreEqual(0, _selenium.WebStorage.Count, "Item Count after clearing session storage");
            dict.Add("testkey6", @"testvalue7");
            _selenium.AddToWebStorage(dict);
            Assert.AreEqual(
                2,
                _selenium.WebStorage.Count,
                "Item Count after adding 2 items to cleared session storage");
            _selenium.WebStorage = backupStorage;
            Assert.AreEqual(1, _selenium.WebStorage.Count, "Item Count after restoring session storage");
        }

        private void TextInElementMatchesTest()
        {
            Assert.IsTrue(
                _selenium.TextInElementMatches("paragraph", "piece of text"),
                "Search for 'piece of text' anywhere in element");
            Assert.IsFalse(
                _selenium.TextInElementMatches("paragraph", "^piece of text$"),
                "Search for exact 'piece of text'");
            Assert.IsTrue(
                _selenium.TextInElementMatches("paragraph", "piece .* elements"),
                "Search for 'piece' and later 'elements'");
        }

        private void UploadTest()
        {
            var driver = _selenium.Driver;
            if (driver.IsIe() && _selenium.AreAllProtectedModes(true))
            {
                MarkSkipped(nameof(UploadTest), "Upload not supported for IE with Protected Mode on");
                return;
            }
            var testFile = Path.GetFullPath("uploadTestFile.txt");
            Assert.IsTrue(File.Exists(testFile), "Test file exists");
            Assert.IsTrue(_selenium.UploadFileInElement(testFile, "name:fileToUpload"));
            Assert.IsTrue(
                _selenium.TextInElement("fileContent").Contains("Small text file used for upload tests."),
                "content contains expected value");
        }

        private void WaitForTextTest()
        {
            Assert.IsTrue(_selenium.ReloadPage());
            Assert.IsFalse(
                _selenium.TextExistsIgnoringCase("data load completed"),
                "data load completed does not exist yet");
            Assert.IsTrue(_selenium.WaitForTextIgnoringCase("data load completed"), "Wait for data load completed");
            Assert.IsTrue(_selenium.WaitForText("0,1,1,2"), "Wait for 0,1,1,2");
        }

        private void WaitForTitleTest()
        {
            _selenium.Open(new Uri("http://www.google.com"));
            Assert.IsTrue(_selenium.WaitUntilTitleMatches("Google"));
            _selenium.Open(CreateTestPageUri());
            Assert.IsTrue(_selenium.WaitUntilTitleMatches("Test Page"));
        }

        private void WaitUntilScriptReturnsTrueTest() =>
            Assert.IsTrue(_selenium.WaitUntilScriptReturnsTrue("return true;").ToBool());

        private void WaitWithTimeoutTest()
        {
            _selenium.SetTimeoutSeconds(0.5);
            Assert.IsFalse(_selenium.WaitForElement("NonExistingElement"));
            Assert.IsTrue(_selenium.WaitUntilElementDoesNotExist("NonExistingElement"));
            Assert.IsFalse(_selenium.WaitUntilElementIsClickable("NonExistingElement"));
            Assert.IsTrue(_selenium.WaitUntilElementIsClickable("button"));
        }

        private void WindowDimensionsTest()
        {
            // define sizes that are larger than minimal, but not likely to be maximized sizes
            var testSize = new Coordinate(652, 224);

            var originalSize = _selenium.WindowSize;
            _selenium.WindowSize = testSize;
            Assert.IsTrue(_selenium.WindowSizeIsCloseTo(testSize));
            if (!_runningHeadless)
            {
                _selenium.MaximizeWindow();
                Assert.IsFalse(_selenium.WindowSizeIsCloseTo(testSize), "Window size changed after maximize");
            }
            // test of deprecated function
#pragma warning disable 618
            Selenium.ExceptionOnDeprecatedFunctions = false;
            _ = _selenium.SetWindowSizeX(1, 1);
            var width = _selenium.WindowWidth;
            var height = _selenium.WindowHeight;
            Selenium.ExceptionOnDeprecatedFunctions = true;
#pragma warning restore 618
            Assert.AreEqual(width, _selenium.WindowSize.X, "deprecated width matches Size.X");
            Assert.AreEqual(height, _selenium.WindowSize.Y, "deprecated height matches Size.Y");
            _selenium.WindowSize = originalSize;
            Assert.IsTrue(_selenium.WindowSizeIsCloseTo(originalSize), "Reset window size to original values");

            // only slightly related, but might as well test that here too.
            Assert.IsTrue(_selenium.WindowHandles.Count > 0);
        }

        private void WindowSwitchingTest()
        {
            if (_selenium.Driver.IsIe())
            {
                MarkSkipped(nameof(WindowSwitchingTest),
                    "Bug in Edge in IE mode: see https://github.com/SeleniumHQ/selenium/issues/8868");
                return;
            }
            var pageCount = _selenium.PageCount;
            var currentPage = _selenium.CurrentWindowName;
            _selenium.ClickElement("linkTab");
            var handle = _selenium.WaitForNewWindowName();
            Assert.IsTrue(_selenium.PageCount > pageCount, "Page was added");
            Assert.IsNotNull(handle);
            _selenium.SelectWindow(handle);
            Assert.AreEqual(handle, _selenium.CurrentWindowName, "Switched to new window");
            Assert.IsTrue(_selenium.WaitForElement(@"tagname:h1"));
            Assert.AreEqual("Test site for Selenium Fixture", _selenium.TextInElement(@"tagname:h1"), "on new page");
            _selenium.SelectWindow(string.Empty);
            Assert.AreEqual(
                "Selenium Fixture Test Page", _selenium.TextInElement(@"tagname:h1"),
                "switched back successfully");
            _selenium.SelectWindow(handle);
            _selenium.ClosePage();
            Assert.IsTrue(_selenium.PageCount == pageCount, "Page count reduced");
            _selenium.SelectWindow(string.Empty);
            Assert.AreEqual(currentPage, _selenium.CurrentWindowName, "Switched back after closing page");
        }
    }
}

#endregion

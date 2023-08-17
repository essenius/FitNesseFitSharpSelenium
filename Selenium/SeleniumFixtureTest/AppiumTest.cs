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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    // For this test class to work, make sure the Appium 2 server has been started and the Android emulator is active
    // The specs for the device these tests were designed for are: 
    // Name: 4.7 WXGA API 22, OS: Lollipop 5.1.1, API: 22, Processor: x86, RAM: 512MB, Resolution: 720 x 1280 WXGA.
    // It assumes that the tests are run on Windows 10 1809 or newer, as that can use Hyper-V.
    // This is about the lowest version of Android that can run on Appium 2.


    [TestClass]
    public sealed class AppiumTest
    {
        private const string Apps = "AccessibilityId:Apps";
        private const string Browser = "XPath://android.widget.TextView[@text = 'Browser']";
        private const string CalculatorIcon = "xpath://*[@text='Calculator']";

        private static int _testsToDo;
        private static readonly Selenium Fixture = new();

        [TestMethod]
        [TestCategory("Native")]
        public void AppiumBasicOperationsTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to Apps page");
            Assert.IsTrue(Fixture.WaitForElement(CalculatorIcon));
            Assert.IsTrue(Fixture.Scroll("right"), "Scroll to the right");
            Assert.IsTrue(Fixture.WaitForTextIgnoringCase("Widget Preview"), "Wait for text 'Widget Preview'");
            Assert.IsTrue(Fixture.Scroll("left"), "Scroll to the left");
            Assert.IsTrue(Fixture.WaitForText("Music"), "Wait for text 'Music'");
            Assert.IsTrue(Fixture.PressKeyCode("Back"), "Press the Back button");
            Assert.IsTrue(Fixture.WaitForElement(Browser), "Check if back at the home page");
        }

        [TestMethod]
        [TestCategory("Native")]
        public void AppiumCalculatorTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Tap Apps element");
            Assert.IsTrue(Fixture.Scroll("left"), "Make sure we're on the first page");

            Assert.IsTrue(Fixture.WaitForElement(CalculatorIcon), "Wait for Calculator icon");
            Assert.IsTrue(Fixture.TapElement(CalculatorIcon), "Open the calculator");
            Assert.IsTrue(Fixture.WaitForElement("id:com.android.calculator2:id/formula"), "Wait for the calculator to open");
            Assert.IsTrue(Fixture.LongPressElementForSeconds("xpath://android.widget.Button[@content-desc=\"delete\"] | //android.widget.Button[@content-desc=\"clear\"]", 0.5));
            Assert.IsTrue(Fixture.TapElement("id:com.android.calculator2:id/digit_7"), "Press 7");
            Assert.IsTrue(Fixture.TapElement("AccessibilityId:times"), "Press *");
            Assert.IsTrue(Fixture.TapElement("id:com.android.calculator2:id/digit_8"), "Press 8");
            Assert.AreEqual("7×8", Fixture.TextInElement("id:com.android.calculator2:id/formula"));
            Assert.IsTrue(Fixture.TapElement("AccessibilityId:equals"), "Press =");
            Assert.AreEqual("56", Fixture.TextInElement("id:com.android.calculator2:id/formula"), "Check the calculation result");
            var screenshot = Selenium.Screenshot();
            Debug.Print(Selenium.Screenshot());
            Assert.IsTrue(screenshot.StartsWith(@"<img alt=""Screenshot"" src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAt"), "Screenshot starts OK");

            //// Now we test the LongPressKeyCode with Home. Something strange happening here. It does something else from Appium than on the device emulator itself:
            //// it goes to the recent apps. But it is consistent with long pressing the circle button on the emulator menu (which should be Home).

            Assert.IsTrue(Fixture.PressKeyCode("Home"), "Go Home");
            Assert.IsTrue(Fixture.WaitForElement(Browser), "Wait for the Browser icon");
            Assert.IsTrue(Fixture.PressKeyCode("Keycode_APP_SWITCH"), "Move to Recent Apps page");
            Assert.IsTrue(Fixture.WaitForTextIgnoringCase("Calculator"), "Calculator exists on the recent apps page");
        }

        [TestMethod]
        [TestCategory("Native")]
        public void AppiumDragDropTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to the Apps page");
            // don't use text search as that disappears when doing drag/drop
            const string galleryIcon = "AccessibilityId:Gallery";
            Assert.IsTrue(Fixture.ScrollToElement("left", galleryIcon), "Scroll left to the page with Gallery on it");
            Assert.IsTrue(Fixture.ElementExists(galleryIcon), "Check the Gallery icon is there");
            Assert.IsTrue(Fixture.DragElementAndDropAt(galleryIcon, new Coordinate(400, 400)), "Drag and drop the gallery icon on the home page");
            // the drag/drop makes the element temporarily disappear, so wait for it to be back
            Assert.IsTrue(Fixture.WaitForElement(galleryIcon), "Wait for the Gallery icon");

            const string deleteArea = @"id:com.google.android.googlequicksearchbox:id/delete_target_text";
            Assert.IsTrue(Fixture.DragElementAndDropOnElement(galleryIcon, deleteArea), "Delete the icon by dragging it to the Delete element");
            Assert.IsFalse(Fixture.ElementExists(galleryIcon));
        }

        [TestMethod]
        [TestCategory("Native")]
        [ExpectedExceptionWithMessage(typeof(ArgumentException), "Direction 'bogus' should be Up, Down, Left or Right")]
        public void AppiumScrollWrongTest() => Fixture.Scroll("bogus");

        [TestMethod]
        [TestCategory("Native")]
        public void AppiumSettingsAppTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps));
            const string settingsIcon = "XPath://android.widget.TextView[@text = 'Settings']";
            Assert.IsTrue(Fixture.WaitForElement(settingsIcon));
            Assert.IsTrue(Fixture.TapElement(settingsIcon));
            const string textMore = "XPath://android.widget.TextView[@text = 'More']";
            Assert.IsTrue(Fixture.WaitForElement(textMore));
            Fixture.TapElement(textMore);

            const string switchAirplaneMode = "id:android:id/switchWidget";
            Assert.IsTrue(Fixture.WaitForElement(switchAirplaneMode));
            Assert.AreEqual("OFF", Fixture.TextInElement(switchAirplaneMode));
            Assert.IsTrue(Fixture.PressKeyCode("Back"));
            Assert.IsTrue(Fixture.WaitForElement(settingsIcon));

            const string textAccounts = "XPath://android.widget.TextView[@text = 'Accounts']";
            Assert.IsFalse(Fixture.ElementExists(textAccounts));

            Fixture.Scroll("Down");
            Assert.IsTrue(Fixture.WaitForElement(textAccounts));
            Assert.IsFalse(Fixture.ElementExists(textMore));
            Fixture.ScrollToElement("Up", textMore);

            var elementCount = Fixture.CountOfElements("ClassName:android.widget.TextView");
            Assert.IsTrue(elementCount > 0);
            const string textAbout = "XPath://android.widget.TextView[@text = 'About phone']";
            Assert.IsTrue(Fixture.ScrollToElement("FromTop", textAbout));
            Assert.IsTrue(Fixture.TapElement(textAbout));
            Fixture.WaitForElement("XPath://android.widget.TextView[@text = 'System Update']");
            Assert.IsTrue(Fixture.PressKeyCode("Back"));

            Assert.IsTrue(Fixture.ScrollToElement("Up", textMore));
        }

        [ClassCleanup]
        public static void ClassCleanup() => Fixture.Close();
        // safety net in case tests were ran individually

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            _testsToDo = typeof(AppiumTest)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .Count(m => m.GetCustomAttribute(typeof(TestMethodAttribute)) != null);
            var options = Selenium.NewOptionsFor("Android") as AppiumOptions;
            Assert.IsNotNull(options, "options != null");
            options.PlatformVersion = "5";
            options.AutomationName = "UiAutomator2";
            options.DeviceName = @"4.7 WXGA API 22";

            options.AddAdditionalAppiumOption("clearSystemFiles", "true");
            options.AddAdditionalAppiumOption("adbExecTimeout", "60000");
            options.AddAdditionalAppiumOption("enforceXPath1", true);

            Fixture.SetTimeoutSeconds(10);
            try
            {
                Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithOptions("Android", "http://127.0.0.1:4723", options));
            }
            catch (StopTestException se)
            {
                Assert.Inconclusive($"Could not start Appium test: {se.InnerException?.Message}");
                return;
            }

            Assert.IsTrue(Fixture.PressKeyCode("Home"));
            Assert.IsTrue(Fixture.WaitForElement(Apps), "Wait for the Apps button");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // ClassCleanup is only executed after the whole test suite ends, so that would mean the fixture
            // would stay open until the end of the suite if we would put the Close in there.
            _testsToDo--;
            if (_testsToDo == 0)
            {
                Fixture.Close();
            }
            else
            {
                Assert.IsTrue(Fixture.PressKeyCode("Home"));
                Assert.IsTrue(Fixture.WaitForElement(Apps));
            }
        }
    }
}

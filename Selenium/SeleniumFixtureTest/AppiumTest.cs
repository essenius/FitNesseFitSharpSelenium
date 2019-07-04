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
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Enums;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    // For this test class to work, make sure the Appium server has been started and an Android 4.4 emulator is active
    [TestClass]
    public sealed class AppiumTest
    {
        private const string Apps = "AccessibilityId:Apps";
        private const string Browser = "XPath://android.widget.TextView[@text = 'Browser']";

        private static TestContext _testContext;
        private static int _testsToDo;
        private static readonly Selenium Fixture = new Selenium();

        [TestMethod, TestCategory("Appium")]
        public void AppiumBasicOperationsTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to Apps page");
            Assert.IsTrue(Fixture.WaitForElement(Apps));
            Assert.IsTrue(Fixture.Scroll("right"), "Scroll to the right");
            Assert.IsTrue(Fixture.TextExistsIgnoringCase("Analog Clock"), "Check if the text 'Analog Clock' is there");
            Assert.IsTrue(Fixture.Scroll("left"), "Scroll to the left");
            Assert.IsTrue(Fixture.TextExists("Music"), "Check if the text 'Music't is there");
            Assert.IsTrue(Fixture.PressKeyCode("Back"), "Press the Back button");
            Assert.IsTrue(Fixture.WaitForElement(Browser), "Check if back at the home page");
        }

        [TestMethod, TestCategory("Appium")]
        public void AppiumCalculatorTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Tap Apps element");
            Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:Apps"), "Wait for Apps element (another one)");
            Assert.IsTrue(Fixture.TapElement(Apps), "Tap the second apps element (just in case it's on the other tab)");
            const string calculatorIcon = "xpath://*[@text='Calculator']";
            Assert.IsTrue(Fixture.WaitForElement(calculatorIcon), "Wait for Calculator icon");
            Assert.IsTrue(Fixture.TapElement(calculatorIcon), "Open the calculator");
            Assert.IsTrue(Fixture.TapElement("id:com.android.calculator2:id/digit7"), "Press 7");
            Assert.IsTrue(Fixture.TapElement("accessibilityId:multiply"), "Press *");
            Assert.IsTrue(Fixture.TapElement("id:com.android.calculator2:id/digit8"), "Press 8");
            const string resultBox = "xpath://android.widget.ViewSwitcher/android.widget.EditText";
            Assert.AreEqual("7multiplied by8", Fixture.TextInElement(resultBox), "Check value of result box");
            Assert.IsTrue(Fixture.TapElement("accessibilityId:equals"), "Press =");
            Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:56"), "Wait for answer (also checks value)");
            Assert.AreEqual("56", Fixture.TextInElement(resultBox), "Check the calculation result");
            Debug.Print(Selenium.Screenshot());

            // Now we test the LongPressKeyCode with Home. Something strange happening here. It does something else from Appium than on the device emulator itself:
            // it goes to the recent apps. But it is consistent with long pressing the circle button on the emulator menu (which should be Home).

            Assert.IsTrue(Fixture.PressKeyCode("Home"), "Go Home");
            Assert.IsTrue(Fixture.WaitForElement(Browser), "Wait for the Browser icon");
            Assert.IsTrue(Fixture.LongPressKeyCode("Home"), "Move to Recent Apps page");
            Assert.IsTrue(Fixture.WaitForTextIgnoringCase("Calculator"), "Calculator exists on the recent apps page");
        }

        [TestMethod, TestCategory("Appium")]
        public void AppiumDragDropTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to the Apps page");
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to the Apps page");
            const string galleryIcon = "XPath://android.widget.TextView[@text = 'Gallery']";
            Assert.IsTrue(Fixture.ScrollToElement("left", galleryIcon), "Scroll left to the page with Gallery on it");
            Assert.IsTrue(Fixture.ElementExists(galleryIcon), "Check the Gallery icon is there");
            Assert.IsTrue(Fixture.DragElementAndDropAt(galleryIcon, 400, 800), "Drag and drop the gallery icon on the home page");
            const string deleteArea = "id:com.android.launcher:id/delete_target_text";
            Assert.IsTrue(Fixture.DragElementAndDropOnElement(galleryIcon, deleteArea), "Delete the icon by dragging it to the Delete element");
            Assert.IsFalse(Fixture.ElementExists(galleryIcon));
        }

        [TestMethod, TestCategory("Appium")]
        public void AppiumLongPressElementForSecondsTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to the Apps page");
            Assert.IsTrue(Fixture.TapElement(Apps), "Go to the Apps page");
            const string calculatorIcon = "XPath://android.widget.TextView[@text = 'Calculator']";
            Assert.IsTrue(Fixture.LongPressElementForSeconds(calculatorIcon, 1), "Long Press Calculator Icon one second to copy it to the home page");
            const string deleteArea = "id:com.android.launcher:id/delete_target_text";
            Assert.IsTrue(Fixture.DragElementAndDropOnElement(calculatorIcon, deleteArea), "Delete the icon by dragging it to the Delete element");
            Assert.IsFalse(Fixture.ElementExists(calculatorIcon));
            // negative cases
            Assert.IsFalse(Fixture.PressKeyCode(string.Empty));
            Assert.IsFalse(Fixture.LongPressKeyCode(string.Empty));
        }

        [TestMethod, TestCategory("Appium"),
         ExpectedExceptionWithMessage(typeof(ArgumentException), "Direction 'bogus' should be Up, Down, Left or Right")]
        public void AppiumScrollWrongTest() => Fixture.Scroll("bogus");

        [TestMethod, TestCategory("Appium")]
        public void AppiumSettingsAppTest()
        {
            Assert.IsTrue(Fixture.TapElement(Apps));
            const string settingsIcon = "XPath://android.widget.TextView[@text = 'Settings']";
            Assert.IsTrue(Fixture.TapElement(settingsIcon));
            const string switchBluetooth = "id:com.android.settings:id/switchWidget";
            Assert.IsTrue(Fixture.WaitForElement(switchBluetooth));
            Assert.AreEqual("OFF", Fixture.TextInElement(switchBluetooth));

            const string addAccountLink = "AndroidUiAutomator:new UiSelector().textContains(\"Add account\")";
            Assert.IsFalse(Fixture.ElementExists(addAccountLink));
            Fixture.Scroll("Down");
            Assert.IsTrue(Fixture.WaitForElement(addAccountLink));
            Assert.IsFalse(Fixture.ElementExists(switchBluetooth));
            Fixture.ScrollToElement("Up", switchBluetooth);

            var elementCount = Fixture.CountOfElements("ClassName:android.widget.TextView");
            Assert.IsTrue(elementCount > 0);
            Assert.IsTrue(Fixture.ScrollToElement("FromTop", "XPath://android.widget.TextView[@text = 'About phone']"));
            Assert.IsTrue(Fixture.ScrollToElement("Up", switchBluetooth));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // safety net in case tests were ran individually
            Fixture.Close();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _testContext = testContext; // not used at this point
            _testsToDo = typeof(AppiumTest)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .Count(m => m.GetCustomAttribute(typeof(TestMethodAttribute)) != null);
            Debug.Print($"Running {_testsToDo} tests for Appium");
            var caps = new Dictionary<string, object>
            {
                {MobileCapabilityType.DeviceName, "My Device 1"},
                {"automationName", "UiAutomator1"},
                //{ "appPackage", "com.android.settings"},
                //{ "appActivity", ".Settings"},
                {AndroidMobileCapabilityType.AppPackage, "com.android.launcher"},
                {"appActivity", "com.android.launcher2.Launcher"},
                {"newCommandTimeout", 300},
                {"clearSystemFiles", true},
                {"adbExecTimeout", 30000}
            };

            Fixture.SetTimeoutSeconds(60);
            Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithCapabilities("Android", "http://localhost:4723", caps));
            Assert.IsTrue(Fixture.TapElement(Apps));
            // expect the android guidance to kick in, and click it away
            const string okButton = "ClassName:android.widget.Button";
            Assert.IsTrue(Fixture.PressKeyCode("Home"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Not doing ClassCleanup because that is only executed after the whole test suite ends.
            _testsToDo--;
            if (_testsToDo == 0)
            {
                Debug.Print("Closing session");
                Fixture.Close();
            }
            else
            {
                Assert.IsTrue(Fixture.PressKeyCode("Home"));
            }
        }
    }
}
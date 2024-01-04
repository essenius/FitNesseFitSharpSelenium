// Copyright 2021-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using SeleniumFixture;

namespace SeleniumFixtureTest;
// For this test class to work, make sure the Appium 2 server has been started and the Android emulator is active
// The specs for the device these tests were designed for: Pixel 2 Pie 9.0 - API 28 x86, 1GB, 1080x1920, 420 dpi

[TestClass]
public sealed class Appium2Test
{
    private static int _testsToDo;
    private static readonly Selenium Fixture = new();

    [TestMethod]
    [TestCategory("Native")]
    public void Appium2CalculatorTest()
    {
        const string calculatorIcon = "XPath://*[@content-desc = 'Calculator']";
        const string digit7 = "id:com.android.calculator2:id/digit_7";
        const string digit8 = "id:com.android.calculator2:id/digit_8";
        const string multiply = "AccessibilityId:multiply";
        const string formula = "id:com.android.calculator2:id/formula";
        const string equals = "AccessibilityId:equals";
        const string result = "id:com.android.calculator2:id/result";

        Assert.IsTrue(Fixture.Scroll("Down"), "Scroll down");
        Assert.IsTrue(Fixture.WaitForElement(calculatorIcon));
        Assert.IsTrue(Fixture.ClickElement(calculatorIcon));
        Assert.IsTrue(Fixture.WaitForElement(digit7));
        Assert.IsTrue(Fixture.ClickElement(digit7));
        Assert.IsTrue(Fixture.ClickElement(multiply));
        Assert.IsTrue(Fixture.ClickElement(digit8));
        Assert.AreEqual("7×8", Fixture.TextInElement(formula));
        Assert.IsTrue(Fixture.ClickElement(equals));
        Assert.AreEqual("56", Fixture.TextInElement(result));
    }

    [ClassCleanup]
    public static void ClassCleanup() => Fixture.Close();
    // safety net in case tests were ran individually

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _testsToDo = typeof(Appium2Test)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
            .Count(m => m.GetCustomAttribute(typeof(TestMethodAttribute)) != null);
        var options = Selenium.NewOptionsFor("Android") as AppiumOptions;
        Assert.IsNotNull(options, "Options != null");
        options.DeviceName = "Pixel 2 API 28";
        options.AutomationName = "UiAutomator2";
        options.PlatformVersion = "9";
        Fixture.SetTimeoutSeconds(60);
        try
        {
            Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithOptions("Android", "http://127.0.0.1:4723", options));
        }
        catch (StopTestException)
        {
            Assert.Inconclusive("Could not start Appium test");
            return;
        }
        Assert.IsTrue(Fixture.PressKeyCode("Home"), "Press Home");
        Selenium.WaitSeconds(0.5);
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
            Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:Play Store"));
        }
    }
}
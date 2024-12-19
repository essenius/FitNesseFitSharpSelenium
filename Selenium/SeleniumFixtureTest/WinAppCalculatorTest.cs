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

using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using SeleniumFixture;

namespace SeleniumFixtureTest;

/// <remarks>
///     Uses WinAppDriver, see https://github.com/microsoft/WinAppDriver/releases.
///     Start WinAppDriver with parameter 4727 as Appium uses the default port 4723
/// </remarks>
[TestClass]
public class WinAppCalculatorTest
{
    private static readonly Selenium Fixture = new();

    private static void AssertResult(string expectedResult) =>
        Assert.AreEqual($"Display is {expectedResult}", Fixture.TextInElement("AccessibilityId:CalculatorResults"));

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Fixture.Close();
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        var options = Selenium.NewOptionsFor("WinApp") as AppiumOptions;
        Assert.IsNotNull(options, "options != null");
        options.App = @"Microsoft.WindowsCalculator_8wekyb3d8bbwe!App";
        options.DeviceName = "WindowsPC";
        options.AutomationName = "Windows";

        Selenium.DefaultSearchMethod = "name";
        Fixture.RemoteBrowserBasePath = "";
        try
        {
            Assert.IsTrue(
                Fixture.SetRemoteBrowserAtAddressWithOptions("WinApp", "http://127.0.0.1:4723", options));
        }
        catch (StopTestException)
        {
            Assert.Inconclusive("Can't start WinApp");
        }
    }

    private static bool ResultOk(string expectedResult, string rawResult)
    {
        var result = new Regex(@".*\s([-+]?[0-9]*[\.|,]?[0-9]+)\s.*").Matches(rawResult);
        return result[0].Groups.Count > 1 && result[0].Groups[1].Value.Equals(expectedResult);
    }

    [TestMethod]
    [TestCategory("Native")]
    public void WinAppCalcAddTest()
    {
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:TogglePaneButton"), "Open menu");
        Assert.IsTrue(Fixture.WaitForElement("Standard Calculator"), "Wait for standard");
        Assert.IsTrue(Fixture.ClickElement("Standard Calculator"), "Click standard");
        Fixture.ClickElement("Clear");
        AssertResult("0");
        Assert.IsTrue(Fixture.ClickElement("One"), "Click 1");
        Assert.IsTrue(Fixture.ClickElement("Plus"), "Click +");
        Assert.IsTrue(Fixture.ClickElement("Seven"), "Click 7");
        Assert.IsTrue(Fixture.ClickElement("Equals"), "Click =");
        AssertResult("8");
    }

    [TestMethod]
    [TestCategory("Native")]
    public void WinAppCalcScientificTest()
    {
        // Alt-2 is scientific mode
        Assert.IsTrue(Fixture.SendKeysToElement("%2%", "Calculator"));
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:piButton"), "Wait for Pi");
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:degButton"), "Click Deg");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:radButton"), "Wait for Rad");
        Fixture.ClickElement("Clear");
        AssertResult("0");
        Assert.IsTrue(Fixture.ClickElement("One"), "Click 1");
        Assert.IsTrue(Fixture.ClickElement("Trigonometry"), "Click Trigonometry");
        Assert.IsTrue(Fixture.WaitForElement("Sine"), "Wait for Sin");

        Assert.IsTrue(Fixture.ClickElement("Sine"), "Click Sin");
        AssertResult("0.8414709848078965066525023216303");
        Assert.AreEqual(
            "Expression is sine radians (1)",
            Fixture.TextInElement("AccessibilityId:CalculatorExpression"), "Expression OK");

        // put back to default (so the test can be run again)
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:radButton"), "Click Rad");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:gradButton"), "Wait for Grad");
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:gradButton"), "Click Grad");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:degButton"), "Wait for Deg");
    }

    [TestMethod]
    [TestCategory("Native")]
    public void WinAppCalcVolumeTest()
    {
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:TogglePaneButton"), "Open menu");
        Assert.IsTrue(Fixture.WaitForElement("Volume Converter"), "Wait for volume converter");
        Assert.IsTrue(Fixture.ClickElement("Volume Converter"), "click volume converter");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:Units1"), "Wait for unit 1");
        Fixture.ClickElement("Clear entry");
        Assert.IsTrue(ResultOk("0", Fixture.TextInElement("AccessibilityId:Value1")), "Value1==0");
        Assert.IsTrue(ResultOk("0", Fixture.TextInElement("AccessibilityId:Value2")), "Value2==0");

        Assert.IsTrue(Fixture.SetElementTo("AccessibilityId:Units1", "Liters"), "unit1=liters");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:Units2"), "Wait for unit2");
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:Units2"), "Click unit2");
        Selenium.WaitSeconds(0.5);
        Fixture.SendKeysToElement("Gallons (US)", "AccessibilityId:Units2");
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:Units2"), "Click Unit 2 again");
        Assert.IsTrue(Fixture.WaitForElement("AccessibilityId:Value2"), "Wait for value2");
        Assert.IsTrue(Fixture.ClickElement("AccessibilityId:Value2"), "Click value2");
        Assert.IsTrue(Fixture.SetElementTo("AccessibilityId:Value2", "10"), "Set value2 to 10");
        var actualValue = Fixture.TextInElement("AccessibilityId:Value1");
        Assert.IsTrue(ResultOk("37.85412", actualValue), $"Value1 ({actualValue}) OK");
    }
}
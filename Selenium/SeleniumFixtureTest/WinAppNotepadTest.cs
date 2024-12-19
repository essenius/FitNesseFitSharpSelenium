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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using SeleniumFixture;
using System;

namespace SeleniumFixtureTest;

/// <remarks>
///     Uses WinAppDriver, see https://github.com/microsoft/WinAppDriver/releases.
///     Doing that through Appium which partly caters for the non-compliance of WinAppDriver.
/// </remarks>
[TestClass]
public class WinAppNotepadTest
{
    private static readonly Selenium Fixture = new();
    private static string _closeKeys = "^a^{Del}^w^";
    private static string _editorClass = "RichEditD2DPT";
    private static string _newLine = "\r";
    private static bool _isAtLeastWindows10 = true;


    [ClassCleanup]
    public static void ClassCleanup()
    {
        if (!_isAtLeastWindows10) return;

        // Just SendKeys doesn't work, as WinAppDriver can't handle ActiveElement
        Fixture.SendKeysToElement(_closeKeys, _editorClass);
        Fixture.Close();
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _isAtLeastWindows10 = Environment.OSVersion.Version.Build >= 10240;
            
        if (!_isAtLeastWindows10) return;

        var options = Selenium.NewOptionsFor("WinApp") as AppiumOptions;
        Assert.IsNotNull(options, "options != null");
        options.App = "notepad.exe";
        options.AutomationName = "Windows";
        Fixture.RemoteBrowserBasePath = "";

        Selenium.DefaultSearchMethod = "ClassName";
        Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithOptions("WinApp", "http://127.0.0.1:4723", options));
        var isWindows10 = Environment.OSVersion.Version.Build < 22000;
        if (isWindows10)
        {
            _editorClass = "Edit";
            _newLine = "\r\n";
            _closeKeys = "^a^{Del}%{F4}%";
        }
        else
        {
            // Open a new tab (earlier versions don't have that)
            Assert.IsTrue(Fixture.SendKeysToElement("^n^", "Notepad"));
        }
    }

    [TestMethod]
    [TestCategory("Native")]
    public void NotePadTest()
    {
        if (!_isAtLeastWindows10) return;

        Fixture.SetTimeoutSeconds(2);
        const string testMessage = "The quick brown fox jumps over the lazy dog.";

        Assert.IsTrue(Fixture.SetElementTo(_editorClass, testMessage), "Set element value OK");
        Assert.IsTrue(Fixture.SendKeysToElement("^{END}^{ENTER}Hello{ENTER}there", _editorClass), "SendKeys OK");
        var result = Fixture.TextInElement(_editorClass);
        Assert.AreEqual($"{testMessage}{_newLine}Hello{_newLine}there", result, "Content OK");

        /* Sizing doesn't work well with WinAppDriver, so disabling until that's corrected
        var desiredSize = new Coordinate(400, 140);
        Fixture.WindowPosition = new Coordinate(10, 10);
        Assert.AreEqual(10, Fixture.WindowPosition.X, "X position OK");
        Assert.AreEqual(10, Fixture.WindowPosition.Y, "Y position OK");
        Fixture.WindowSize = desiredSize;
        Assert.AreEqual(400, Fixture.WindowSize.X, "Width OK");
        Assert.AreEqual(140, Fixture.WindowSize.Y, "Height OK");
        Fixture.MaximizeWindow();
        Assert.AreNotEqual(desiredSize, Fixture.WindowSize, "Size differs");
        Fixture.WindowSize = desiredSize;
        Assert.AreEqual(desiredSize, Fixture.WindowSize, "Size OK");
        var desiredLocation = new Coordinate(200, 250);
        Fixture.WindowPosition = desiredLocation;
        Assert.AreEqual(desiredLocation, Fixture.WindowPosition, "Position OK");
        */

        var snapshot = Selenium.Screenshot();
        const string expectedPart =
            @"<img alt=""Screenshot"" src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUg";
        Assert.IsTrue(snapshot.StartsWith(expectedPart), "Snapshot starts OK");
    }
}
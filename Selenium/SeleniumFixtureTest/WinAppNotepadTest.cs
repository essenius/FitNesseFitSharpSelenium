// Copyright 2015-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    /// <remarks>
    ///     Uses WinAppDriver, see https://github.com/microsoft/WinAppDriver/releases .
    ///     Start WinAppDriver with parameter 4727 as Appium uses the default port 4723
    /// </remarks>
    [TestClass]
    public class WinAppNotepadTest
    {
        private static readonly Selenium Fixture = new();

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Fixture.SendKeys("%{F4}%");
            Selenium.WaitSeconds(1);
            Fixture.ClickElementIfVisible("name:Don't Save");
            Fixture.Close();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            var caps = new Dictionary<string, string>
            {
                { "app", "notepad.exe" }
            };
            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithCapabilities("WinApp", "http://127.0.0.1:4727", caps));
        }

        [TestMethod]
        [TestCategory("Native")]
        public void NotePadTest()
        {
            Fixture.SetTimeoutSeconds(2);
            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetElementTo("ClassName:Edit", "The quick brown fox jumps over the lazy dog."), "Set element value OK");
            Assert.IsTrue(Fixture.SendKeys("^{END}^{ENTER}Hello{ENTER}there"), "SendKeys OK");
            Assert.AreEqual("The quick brown fox jumps over the lazy dog.\r\nHello\r\nthere", Fixture.TextInElement(@"ClassName:Edit"), "Content OK");
            var desiredSize = new Coordinate(400, 140);
            Fixture.WindowPosition = new Coordinate(10, 10);
            Assert.AreEqual(10, Fixture.WindowPosition.X, "X position OK");
            Assert.AreEqual(10, Fixture.WindowPosition.Y, "Y position OK");
            Fixture.WindowSize = desiredSize;
            Assert.AreEqual(400, Fixture.WindowSize.X, "Widh OK");
            Assert.AreEqual(140, Fixture.WindowSize.Y, "Height OK");
            Fixture.MaximizeWindow();
            Assert.AreNotEqual(desiredSize, Fixture.WindowSize, "Size differs");
            Fixture.WindowSize = desiredSize;
            Assert.AreEqual(desiredSize, Fixture.WindowSize, "Size OK");
            var desiredLocation = new Coordinate(200, 250);
            Fixture.WindowPosition = desiredLocation;
            Assert.AreEqual(desiredLocation, Fixture.WindowPosition, "Position OK");
            var snapshot = Selenium.Screenshot();
            const string expectedPart =
                "<img alt=\"Screenshot\" src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAZAAAACMCAIAAADdvmjPAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAB";
            Assert.IsTrue(snapshot.StartsWith(expectedPart), "Snapshot starts OK");
        }
    }
}

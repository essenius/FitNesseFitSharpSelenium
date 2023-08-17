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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
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
            // Just SendKeys doesn't work, as WinAppDriver can't handle ActiveElement
            Fixture.SendKeysToElement("^a{Del}%{F4}%", "ClassName:Edit");
            Fixture.Close();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            var options = Selenium.NewOptionsFor("WinApp") as AppiumOptions;
            Assert.IsNotNull(options, "options != null");
            options.App = "notepad.exe";
            options.AutomationName = "Windows";
            //options.AddAdditionalAppiumOption("ms:experimental-webdriver", true);

            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithOptions("WinApp", "http://127.0.0.1:4723", options));
        }

        [TestMethod]
        [TestCategory("Native")]
        public void NotePadTest()
        {
            Fixture.SetTimeoutSeconds(2);
            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetElementTo("ClassName:Edit", "The quick brown fox jumps over the lazy dog."), "Set element value OK");
            Assert.IsTrue(Fixture.SendKeysToElement("^{END}^{ENTER}Hello{ENTER}there", "ClassName:Edit"), "SendKeys OK");
            Assert.AreEqual("The quick brown fox jumps over the lazy dog.\r\nHello\r\nthere", Fixture.TextInElement(@"ClassName:Edit"), "Content OK");
            
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
}

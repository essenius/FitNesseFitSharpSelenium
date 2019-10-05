using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class WinAppNotepadTest
    {
        private static TestContext _testContext;
        private static readonly Selenium Fixture = new Selenium();

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Fixture.SendKeys("%{F4}%");
            Selenium.WaitSeconds(1);
            Fixture.ClickElementIfVisible("name:Don't Save");
            Fixture.Close();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _testContext = testContext; // not used at this point
            var caps = new Dictionary<string, object>
            {
                {"app", "notepad.exe"}
            };
            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetRemoteBrowserAtAddressWithCapabilities("WinApp", "http://127.0.0.1:4723", caps));
        }

        [TestMethod]
        public void NotePadTest()
        {
            Fixture.SetTimeoutSeconds(2);
            Selenium.DefaultSearchMethod = "name";
            Assert.IsTrue(Fixture.SetElementTo("ClassName:Edit", "The quick brown fox jumps over the lazy dog."), "Set element value OK");
            Assert.IsTrue(Fixture.SendKeys("^{END}^{ENTER}Hello{ENTER}there"), "SendKeys OK");
            Assert.AreEqual("The quick brown fox jumps over the lazy dog.\r\nHello\r\nthere", Fixture.TextInElement(@"ClassName:Edit"), "Content OK");
            var desiredSize = new Coordinate(400, 140);
            Fixture.WindowPosition = new Coordinate(10,10);
            Assert.AreEqual(10, Fixture.WindowPosition.X, "X position OK");
            Assert.AreEqual(10, Fixture.WindowPosition.Y, "Y position OK");
            Fixture.WindowSize = desiredSize;
            Assert.AreEqual(400, Fixture.WindowSize.X, "Widh OK");
            Assert.AreEqual(140, Fixture.WindowSize.Y, "Height OK");
            Fixture.MaximizeWindow();
            Assert.AreNotEqual(desiredSize, Fixture.WindowSize, "Size differs");
            Debug.Print(Fixture.WindowSize.ToString());
            Fixture.WindowSize = desiredSize;
            Assert.AreEqual(desiredSize, Fixture.WindowSize, "Size OK");
            var desiredLocation = new Coordinate(200, 250);
            Fixture.WindowPosition = desiredLocation;
            Assert.AreEqual(desiredLocation, Fixture.WindowPosition, "Position OK");
            var snapshot = Selenium.Screenshot();
            Debug.Print(snapshot);
            const string expectedPart = "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAZAAAACMCAIAAADdvmjPAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAB";
            Assert.IsTrue(snapshot.StartsWith(expectedPart), "Snapshot starts OK"); 
        }
    }
}

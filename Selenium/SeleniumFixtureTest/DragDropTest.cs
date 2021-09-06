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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class DragDropTest
    {
        private Selenium _selenium;

        [TestMethod]
        [TestCategory("Integration")]
        public void DragDropAcrossWindowsTest()
        {
            var driverHandle1 = _selenium.NewBrowser("chrome");
            var url = EndToEndTest.CreateTestPageUri();
            _selenium.Open(url);
            Assert.IsTrue(_selenium.WaitForElement("dragSource"), "Wait for DragSource in browser 1");
            Assert.IsFalse(_selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"), "Source not dropped in target in driver 1");
            var driver1 = _selenium.Driver;
            var target = driver1.FindElement(By.Id("dropTarget"));

            var driverHandle2 = _selenium.NewBrowser("chrome");
            _selenium.Open(url);
            Assert.IsTrue(_selenium.WaitForElement("dragSource"), "Wait for DragSource in browser 2");
            var driver2 = _selenium.Driver;
            var source = driver2.FindElement(By.Id("dragSource"));
            Assert.AreNotEqual(driver1, driver2, "We have two different drivers");

            DragDrop.DragToWindow(driver2, source, driver1, target);
            Assert.IsFalse(_selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"), "Source not dropped in target in driver 2");

            _selenium.SetDriver(driverHandle1);
            Assert.IsTrue(_selenium.ElementExists("CssSelector: div#dropTarget > #dragSource"), "Source was dropped in target in driver 1");
            _selenium.Close();

            _selenium.SetDriver(driverHandle2);
            _selenium.Close();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DragDropPositionGetXTest()
        {
            Assert.AreEqual(1, DragDrop.Position.BottomLeft.GetX(100));
            Assert.AreEqual(50, DragDrop.Position.Bottom.GetX(100));
            Assert.AreEqual(99, DragDrop.Position.BottomRight.GetX(100));
            Assert.AreEqual(1, DragDrop.Position.Left.GetX(100));
            Assert.AreEqual(50, DragDrop.Position.Center.GetX(100));
            Assert.AreEqual(99, DragDrop.Position.Right.GetX(100));
            Assert.AreEqual(1, DragDrop.Position.TopLeft.GetX(100));
            Assert.AreEqual(50, DragDrop.Position.Top.GetX(100));
            Assert.AreEqual(99, DragDrop.Position.TopRight.GetX(100));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DragDropPositionGetYTest()
        {
            Assert.AreEqual(99, DragDrop.Position.BottomLeft.GetY(100));
            Assert.AreEqual(99, DragDrop.Position.Bottom.GetY(100));
            Assert.AreEqual(99, DragDrop.Position.BottomRight.GetY(100));
            Assert.AreEqual(50, DragDrop.Position.Left.GetY(100));
            Assert.AreEqual(50, DragDrop.Position.Center.GetY(100));
            Assert.AreEqual(50, DragDrop.Position.Right.GetY(100));
            Assert.AreEqual(1, DragDrop.Position.TopLeft.GetY(100));
            Assert.AreEqual(1, DragDrop.Position.Top.GetY(100));
            Assert.AreEqual(1, DragDrop.Position.TopRight.GetY(100));
        }

        [TestInitialize]
        public void SeleniumDragDropTestInitialize() => _selenium = new Selenium();
    }
}

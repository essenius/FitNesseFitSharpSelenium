﻿// Copyright 2015-2021 Rik Essenius
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
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class Experiments
    {
        [TestMethod]
        [TestCategory("Experiments")]
        public void SeleniumShowChromeRightClickError()
        {
            var selenium = new Selenium();
            // Chrome cannot interact with context menus. Workaround is using native sendkeys, but we don't want to go there.
            // This test tries to select all in the context meny and then press delete. It just presses delete instead, so the
            // field doesn't get empty. If this test would fail, right click would work
            selenium.SetBrowser("chrome");
            selenium.SetTimeoutSeconds(20);
            const string textboxLocator = "id:text1";
            Assert.IsTrue(selenium.Open(EndToEndTest.CreateTestPageUri()), "Open page");
            Assert.IsTrue(selenium.WaitUntilTitleMatches("Selenium Fixture Test Page"));
            Assert.IsTrue(selenium.RightClickElement(textboxLocator), "Show context menu");
            Selenium.WaitSeconds(0.2); // allow dropdown to expand
            const string selectAllInContextMenuSequence = "{DOWN}a";
            selenium.SendKeysToElement(new KeyConverter(selectAllInContextMenuSequence).ToSeleniumFormat, textboxLocator);
            selenium.SendKeysToElement("{DELETE}", textboxLocator);
            Assert.IsFalse(string.IsNullOrEmpty(selenium.AttributeOfElement("value", textboxLocator)), "text 1 is empty");
            selenium.Close();
        }
    }
}
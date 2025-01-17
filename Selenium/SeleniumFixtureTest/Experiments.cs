﻿// Copyright 2015-2024 Rik Essenius
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest;

[TestClass]
public class Experiments
{
    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumShowChromeRightClickError()
    {
        var selenium = new Selenium();
        // Chrome cannot interact with context menus. Workaround is using native sendkeys, but we don't want to go there.
        // This test tries to select all in the context menu and then press delete. It just presses delete instead, so the
        // field doesn't get empty. If this test would fail, right click would work
        selenium.SetBrowser("chrome");
        selenium.SetTimeoutSeconds(20);
        const string textboxLocator = "id:text1";
        Assert.IsTrue(selenium.Open(EndToEndTest.CreateTestPageUri()), "Open page");
        Assert.IsTrue(selenium.WaitUntilTitleMatches("Selenium Fixture Test Page"));
        Assert.IsTrue(selenium.RightClickElement(textboxLocator), "Show context menu");
        Selenium.WaitSeconds(0.2); // allow dropdown to expand
        const string selectAllInContextMenuSequence = "{DOWN}a";
        selenium.SendKeysToElement(new KeyConverter(selectAllInContextMenuSequence).ToSeleniumFormat,
            textboxLocator);
        selenium.SendKeysToElement("{DELETE}", textboxLocator);
        Assert.IsFalse(string.IsNullOrEmpty(selenium.DomPropertyOfElement("value", textboxLocator)),
            "text 1 is empty");
        selenium.Close();
    }

    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumIeTest()
    {
        var selenium = new Selenium();
        var options = Selenium.NewOptionsFor("ie") as InternetExplorerOptions;
        Assert.IsNotNull(options, "Options is not null");
        selenium.SetBrowserWithOptions("ie", options);
        selenium.Open(new Uri("https://www.google.com?hl=en"));

        selenium.Close();
    }

    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumTypeTest()
    {
        var selenium = new Selenium();
        Selenium.SetProxyType("system");
        selenium.SetBrowser("firefox");
        selenium.Open(EndToEndTest.CreateTestPageUri());
        selenium.SendKeysToElementIfTypeIs("472024", "week", "week");
        Assert.IsTrue(selenium.ElementHasDomAttribute("id:brokenImage", "alt"), "Alt attribute exists");

        selenium.Close();
    }

    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumEdgeTest()
    {
        var selenium = new Selenium();
        Selenium.SetProxyType("system");
        var options = Selenium.NewOptionsFor("edge") as EdgeOptions;
        Assert.IsNotNull(options, "Options is not null");
        selenium.SetBrowserWithOptions("edge", options);
        selenium.Open(new Uri("http://www.google.com?hl=en"));
        selenium.Close();
    }

    [TestMethod]
    [TestCategory("Experiments")]
    public void SeleniumRemoteTest()
    {
        var selenium = new Selenium();
        var options = Selenium.NewOptionsFor("chrome") as ChromeOptions;
        Assert.IsNotNull(options, "Options is not null");
        selenium.SetRemoteBrowserAtAddressWithOptions("chrome", "http://localhost:6667", options);
        selenium.Open(new Uri("http://www.google.com?hl=en"));
        selenium.Close();
    }
}

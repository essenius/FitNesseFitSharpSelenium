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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest;

[TestClass]
public class BrowserStorageTest
{
    // todo: fix issue of ChromeDriver not closing down

    private IWebDriver _driver;
    private string _driverHandle;

    private void BrowserStorageJavaScriptFindFirstOnLocalTest()
    {
        var bs = new JavaScriptBrowserStorage(_driver, StorageType.Local);
        bs.Clear();
        bs.SetItem("key1", "value1");
        Assert.AreEqual("value1", bs["key1"], @"JSFFL GetValue via this[]");
        Assert.AreEqual("key1", bs.FindFirstKeyLike("key*"), @"JSFFL FindFirst before clear");
        bs.RemoveItem("key1");
        Assert.IsNull(bs.FindFirstKeyLike("key*"), @"JSFFL FindFirst after clear");
    }

    private void BrowserStorageJavaScriptFindFirstOnSessionTest()
    {
        var bs = new JavaScriptBrowserStorage(_driver, StorageType.Session);
        bs.Clear();
        bs.SetItem("key1", "value1");
        Assert.AreEqual("value1", bs.GetItem("key1"), @"JSFFS GetValue");
        Assert.AreEqual("key1", bs.FindFirstKeyLike("key*"), @"JSFFS FindFirst before clear");
        bs.RemoveItem("key1");
        Assert.IsNull(bs.FindFirstKeyLike("key*"), @"JSFFS FindFirst after clear");
    }

    private static void BrowserStorageNoTest()
    {
        var bs = new NoBrowserStorage();
        Assert.IsNull(bs.KeySet);
        Assert.IsNull(bs.GetItem("N key"));
        Assert.IsFalse(bs.Clear());
        Assert.IsFalse(bs.RemoveItem("N key"));
        try
        {
            bs.SetItem("key", "value");
            Assert.Fail("No exception thrown");
        }
        catch (NotImplementedException)
        {
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void BrowserStorageTests()
    {
        BrowserStorageJavaScriptFindFirstOnSessionTest();
        BrowserStorageNoTest();
        BrowserStorageJavaScriptFindFirstOnLocalTest();
    }

    [TestCleanup]
    public void TestCleanup() => BrowserDriverContainer.RemoveDriver(_driverHandle);

    [TestInitialize]
    public void TestInitialize()
    {
        _driverHandle = BrowserDriverContainer.NewDriver("edge headless", null);
        _driver = BrowserDriverContainer.Current;
        _driver.Navigate().GoToUrl(EndToEndTest.CreateTestPageUri());
    }
}
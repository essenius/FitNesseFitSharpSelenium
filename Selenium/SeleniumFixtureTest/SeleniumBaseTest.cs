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
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class SeleniumBaseTest
    {
        private readonly Collection<KeyValuePair<string, string>> _skippedTests = new();
        private ProtectedModeEnum _activeProtectedMode = ProtectedModeEnum.Unknown;

        private Selenium _selenium;
        public static string RemoteSelenium { get; } = AppConfig.Get("RemoteSelenium");

        private void CheckStorageFunctioning(string tag)
        {
            Assert.IsNotNull(_selenium.WebStorage, "WebStorage supported");
            Assert.IsTrue(_selenium.ClearWebStorage());
            Assert.AreEqual(0, _selenium.WebStorage.Count, $"{tag}: Item Count == 0 after opening page");
            _selenium.SetInWebStorageTo(@"testkey", @"testvalue");
            Assert.AreEqual(@"testvalue", _selenium.GetFromWebStorage("tes*ey"), $"{tag}: Can retrieve added test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after adding a value");
            _selenium.SetInWebStorageTo(@"testkey", @"testvalue2");
            Assert.AreEqual(@"testvalue2", _selenium.GetFromWebStorage(@"testkey"), $"{tag}: Can retrieve changed test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after changing a value");
            _selenium.SetInWebStorageTo("testkey1", @"testvalue1");
            Assert.AreEqual(@"testvalue1", _selenium.GetFromWebStorage("testkey1"), $"{tag}: Can retrieve 2nd added test value");
            Assert.AreEqual(2, _selenium.WebStorage.Count, $"{tag}: Item Count == 2 after adding 2nd value");
            Assert.AreEqual("testkey1", _selenium.GetKeyLikeFromWebStorage(@"testkey?"));
            _selenium.RemoveFromWebStorage(@"testkey");
            Assert.AreEqual(@"testvalue1", _selenium.GetFromWebStorage(@"testkey1"), $"{tag}: Can retrieve changed test value");
            Assert.AreEqual(1, _selenium.WebStorage.Count, $"{tag}: Item Count == 1 after removing an item");
        }

        private void GetProtectedMode()
        {
            if (_activeProtectedMode != ProtectedModeEnum.Unknown) return;
            if (_selenium.AreAllProtectedModes(true))
            {
                _activeProtectedMode = ProtectedModeEnum.On;
                return;
            }
            if (_selenium.AreAllProtectedModes(false))
            {
                _activeProtectedMode = ProtectedModeEnum.Off;
                return;
            }
            _activeProtectedMode = ProtectedModeEnum.Mixed;
        }

        private void MarkSkipped(string key, string value) => _skippedTests.Add(new KeyValuePair<string, string>(key, value));


        [TestMethod]
        [TestCategory("LocalBrowser")]
        [DeploymentItem(@"test\SeleniumFixtureTest\uploadTestFile.txt")]
        public void SeleniumLocalInternetExplorerTests()
        {
            GetProtectedMode();
            Assert.AreNotEqual(ProtectedModeEnum.Unknown, _activeProtectedMode);
            if (_activeProtectedMode != ProtectedModeEnum.Mixed) return;
            MarkSkipped(nameof(SeleniumLocalInternetExplorerTests), "Protected Modes are not all equal");
            /*SeleniumLocalTest("ie"); */
        }

        [TestMethod]
        [TestCategory("Experiments")]
        public void SeleniumShowChromeRightClickError()
        {
            // Chrome cannot interact with context menus. Workaround is using native sendkeys, but we don't want to go there.
            // This test tries to select all in the context meny and then press delete. It just presses delete instead, so the
            // field doesn't get empty. If this test would fail, right click would work
            SetBrowser(false, "chrome");
            _selenium.SetTimeoutSeconds(20);
            const string textboxLocator = "id:text1";
            Assert.IsTrue(_selenium.Open(EndToEndTest.CreateTestPageUri()), "Open page");
            Assert.IsTrue(_selenium.WaitUntilTitleMatches("Selenium Fixture Test Page"));
            Assert.IsTrue(_selenium.RightClickElement(textboxLocator), "Show context menu");
            Selenium.WaitSeconds(0.2); // allow dropdown to expand
            const string selectAllInContextMenuSequence = "{DOWN}a";
            _selenium.SendKeysToElement(new KeyConverter(selectAllInContextMenuSequence).ToSeleniumFormat, textboxLocator);
            _selenium.SendKeysToElement("{DELETE}", textboxLocator);
            Assert.IsFalse(string.IsNullOrEmpty(_selenium.AttributeOfElement("value", textboxLocator)), "text 1 is empty");
        }

        private void SeleniumStorageTestFor(string browser)
        {
            SetBrowser(false, browser);

            try
            {
                Assert.IsTrue(_selenium.Open(EndToEndTest.CreateTestPageUri()), $"{browser}: Open page");
                // setting storage type to Local by default
                CheckStorageFunctioning($"{browser}/Local");
                _selenium.SetInWebStorageTo("testkey2", @"testvalue3");
                var dict = new Dictionary<string, string>();
                Assert.AreEqual(2, _selenium.WebStorage.Count, $"{browser}: Item Count == 2 after adding SetInWebStorageTo");
                dict.Add("testkey4", @"testvalue5");
                _selenium.AddToWebStorage(dict);
                Assert.AreEqual(3, _selenium.WebStorage.Count, $"{browser}: Item Count == 3 after AddToWebStorage, before switching to Session");
                _selenium.UseWebStorage(StorageType.Session);
                CheckStorageFunctioning($"{browser}/Session");
                Assert.AreEqual(1, _selenium.WebStorage.Count, $"{browser}: Item Count after adding one item to session storage");
                _selenium.UseWebStorage(StorageType.Local);
                Assert.AreEqual(3, _selenium.WebStorage.Count, $"{browser}: Item Count after switching back to local storage");
                Assert.IsTrue(_selenium.ClearWebStorage(), $"{browser}: Can clear local storage");
                Assert.AreEqual(0, _selenium.WebStorage.Count, $"{browser}: Item Count after clearing local storage");
                _selenium.UseWebStorage(StorageType.Session);
                Assert.AreEqual(1, _selenium.WebStorage.Count, $"{browser}: Item Count after switching back to session storage");
                var backupStorage = _selenium.WebStorage;
                Assert.IsTrue(_selenium.ClearWebStorage(), $"{browser}: Can clear session storage");
                Assert.AreEqual(0, _selenium.WebStorage.Count, $"{browser}: Item Count after clearing session storage");
                dict.Add("testkey6", @"testvalue7");
                _selenium.AddToWebStorage(dict);
                Assert.AreEqual(2, _selenium.WebStorage.Count, $"{browser}: Item Count after adding 2 items to cleared session storage");
                _selenium.WebStorage = backupStorage;
                Assert.AreEqual(1, _selenium.WebStorage.Count, $"{browser}: Item Count after restoring session storage");
            }
            finally
            {
                _selenium.Close();
            }
        }


        [TestMethod]
        [TestCategory("Integration")]
        public void SeleniumWebStorageTest()
        {
            SeleniumStorageTestFor("chrome");
            SeleniumStorageTestFor("firefox");
        }


        private void SetBrowser(bool remote, string browser)
        {
            if (remote)
            {
                var capabilities = new Dictionary<string, object>
                {
                    { "testCapability", "testValue" }
                };
                Assert.IsTrue(_selenium.SetRemoteBrowserAtAddressWithCapabilities(browser, RemoteSelenium, capabilities),
                    "Set remote browser " + browser);
            }
            else
            {
                Assert.IsTrue(_selenium.SetBrowser(browser), "Set local browser " + browser);
            }
        }

        private enum ProtectedModeEnum
        {
            Unknown,
            Off,
            On,
            Mixed
        }

        #region Additional test attributes

        [TestInitialize]
        public void SeleniumBaseTestInit()
        {
            _selenium = new Selenium();
        }

        [TestCleanup]
        public void SeleniumBaseTestCleanup()
        {
            _selenium.Close();
        }

        #endregion
    }
}

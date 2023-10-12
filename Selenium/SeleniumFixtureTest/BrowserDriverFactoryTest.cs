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

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class BrowserDriverFactoryTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(@"chrome", @"CHROME")]
        [DataRow(@"Chrome", @"CHROME")]
        [DataRow(@"chROMe", @"CHROME")]
        [DataRow(@"Chrome   Headless", @"CHROMEHEADLESS")]
        [DataRow(@"Google Chrome Headless", @"CHROMEHEADLESS")]
        [DataRow(@"Google Chrome", @"CHROME")]
        [DataRow(@"GoogleChrome", @"CHROME")]
        [DataRow(@"internet explorer", @"IE")]
        [DataRow(@"Internet Explorer", @"IE")]
        [DataRow(@"firefox", @"FIREFOX")]
        [DataRow(@"ff", @"FIREFOX")]
        [DataRow(@"ffheadless", @"FIREFOXHEADLESS")]
        [DataRow(@"unknown", @"UNKNOWN")]
        public void BrowserDriverFactoryStandardizeBrowserNameTest(string input, string expected)
        {
            var method = typeof(BrowserDriverFactory).GetMethod("StandardizeBrowserName", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            Assert.AreEqual(expected, method.Invoke(null, new object[] { input })?.ToString());
        }
    }
}

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
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest;

[TestClass]
public class AppConfigTest
{
    [TestMethod]
    [TestCategory("Unit")]
    public void AppConfigGetTest()
    {
        Assert.IsTrue(AppConfig.Get(@"HOMEDRIVE").Matches("[A-Za-z]:"));
        Assert.IsTrue(AppConfig.Get(@"InternetExplorer.IgnoreProtectedModeSettings").Matches("true|false"));
        Assert.IsNull(AppConfig.Get(@"nonexisting_q231"));
        var testSite = AppConfig.Get(@"TestSite");
        Assert.IsNotNull(testSite, @"TestSite exists");
        Assert.IsTrue(testSite.Contains(@"azurewebsites"), "overruled in environment");
        Assert.IsTrue(AppConfig.Get(@"InternetExplorer.IgnoreProtectedModeSettings").Matches("true|false"));
        Assert.AreEqual("C:\\test", AppConfig.Get("DriverFolder"));
    }
}
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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest;

[TestClass]
public class BrowserDriverCreatorTest
{
    [TestMethod]
    [TestCategory("Unit")]
    public void BrowserDriverCreatorGetDefaultServiceFirefoxTest()
    {
        using var service1 = BrowserDriverCreator.GetDefaultService<FirefoxDriverService>();
        Assert.IsInstanceOfType(service1, typeof(FirefoxDriverService));
        Assert.IsFalse(service1.IsRunning);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void BrowserDriverCreatorGetDefaultServiceTest()
    {
        using (var service1 = BrowserDriverCreator.GetDefaultService<InternetExplorerDriverService>())
        {
            Assert.IsInstanceOfType(service1, typeof(InternetExplorerDriverService));
            Assert.IsFalse(service1.IsRunning);
        }
        try
        {
            var x = BrowserDriverCreator.GetDefaultService<ChromeDriverService>(@"c:\");
            x.Start();
            Assert.Fail("Expected exception didn't happen");
        }
        catch (Win32Exception ex)
        {
            Assert.IsTrue(ex.Message.StartsWith(@"An error occurred trying to start process 'c:\chromedriver.exe'"));
        }
    }
}

// Copyright 2024 Rik Essenius
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
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace SeleniumFixtureTest;

[TestClass]
public class PlainSeleniumTest

{
    [TestMethod ,TestCategory("Experiments")]
    public void TestChromeStartup()

    {
        var options = new ChromeOptions();
        //options.AddArgument("--no-sandbox");
        //options.AddArgument("--remote-debugging-port=9222");
        //options.AddArgument("--no-first-run");
        //options.AddArgument("--remote-debugging-pipe");
        //options.AddArgument("--disable-dev-shm-usage");
        //options.AddArgument("--headless");
        //options.AddExcludedArgument("enable-automation");
        var driverService = ChromeDriverService.CreateDefaultService();
        var driver = new ChromeDriver(driverService, options, TimeSpan.FromSeconds(15));
        driver.Navigate().GoToUrl(EndToEndTest.CreateTestPageUri());
        Assert.IsTrue(driver.FindElement(By.Id("sectionElements")).Displayed);
        driver.Quit();
    }

    [TestMethod, TestCategory("Experiments")]
    public void TestFirefoxStartup()

    {
        var driver = new FirefoxDriver();
        driver.Navigate().GoToUrl(EndToEndTest.CreateTestPageUri());
        Assert.IsTrue(driver.FindElement(By.Id("sectionElements")).Displayed);
        driver.Quit();
    }

    [TestMethod, TestCategory("Experiments")]
    public void TestEdgeStartup()

    {
        var driver = new EdgeDriver();
        driver.Navigate().GoToUrl(EndToEndTest.CreateTestPageUri());
        Assert.IsTrue(driver.FindElement(By.Id("sectionElements")).Displayed);
        driver.Quit();
    }

    [TestMethod, TestCategory("Experiments")]
    public void TestEdgeStartupWithGoogle()

    {
        var driver = new EdgeDriver();
        driver.Navigate().GoToUrl("https://google.com");
        Assert.IsTrue(driver.FindElement(By.Name("q")).Displayed);
        driver.Quit();
    }

    [TestMethod, TestCategory("Experiments")]
    public void TestChromeStartupWithGoogle()

    {
        var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://google.com");
        Assert.IsTrue(driver.FindElement(By.Name("q")).Displayed);
        driver.Quit();
    }


    [TestMethod, TestCategory("Experiments")]
    public void TestInternetExplorerStartup()

    {

        var ieOptions = new InternetExplorerOptions
        {
            AttachToEdgeChrome = true,
            IgnoreZoomLevel = true,
            IntroduceInstabilityByIgnoringProtectedModeSettings = false
        };

        var driver = new InternetExplorerDriver(ieOptions);
       driver.Navigate().GoToUrl("https://bing.com");
        driver.FindElement(By.Id("sb_form_q")).SendKeys("WebDriver");
        driver.FindElement(By.Id("sb_form")).Submit();

        driver.Quit();
    }
}

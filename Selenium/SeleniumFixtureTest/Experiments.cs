using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.IE;

namespace SeleniumFixtureTest
{
    public class Experiments
    {
        [TestMethod]
        public void SeleniumInternetExplorerTimeoutTest()
        {
            var options = new InternetExplorerOptions();
            options.AddAdditionalCapability("ie.edgechromium", true);
            options.AddAdditionalCapability("ie.edgepath",
                "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe");
            var driverService = InternetExplorerDriverService.CreateDefaultService();
            var driver = new InternetExplorerDriver(driverService, options, TimeSpan.FromSeconds(15));
            driver.Navigate().GoToUrl("https://www.google.com");
            driver.Quit();
        }
    }
}

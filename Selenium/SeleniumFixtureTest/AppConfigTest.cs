using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class AppConfigTest
    {
        [TestMethod, TestCategory("Unit")]
        public void AppConfigGetTest()
        {
            Assert.IsTrue(AppConfig.Get("HOMEDRIVE").Matches("[A-Za-z]:"));
            Assert.IsTrue(AppConfig.Get("InternetExplorer.IgnoreProtectedModeSettings").Matches("true|false"));
            Assert.IsNull(AppConfig.Get("nonexisting_q231"));
        }
    }
}
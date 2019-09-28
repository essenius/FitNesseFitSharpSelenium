// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class BrowserDriverFactoryTest
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive"),
         SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             @"standardizebrowsername", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml"), TestMethod,
         TestCategory("Unit")]
        public void BrowserDriverFactoryStandardizeBrowserNameTest()
        {
            var privateTarget = new PrivateType(typeof(BrowserDriverFactory));
            var input = TestContext.DataRow["input"].ToString();
            var expected = TestContext.DataRow["expected"].ToString();
            Assert.AreEqual(expected, privateTarget.InvokeStatic(@"StandardizeBrowserName", input));
        }
    }
}

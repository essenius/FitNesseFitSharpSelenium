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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ProtectedModeTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"), DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "protectedmode", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml")]
        public void ProtectedModeAllAreTest()
        {
            var testId = TestContext.DataRow["testId"].ToString();
            bool[] zones =
            {
                TestContext.DataRow["zone1"].ToBool(),
                TestContext.DataRow["zone2"].ToBool(),
                TestContext.DataRow["zone3"].ToBool(),
                TestContext.DataRow["zone4"].ToBool()
            };
            var protectedMode = new ProtectedMode(new ZoneListFactoryMock(zones));
            var allOn = TestContext.DataRow["expectedAllOn"].ToBool();
            var allSame = TestContext.DataRow["expectedAllSame"].ToBool();

            Assert.AreEqual(allOn, protectedMode.AllAre(true), "AllOn for [" + testId + "]");
            Assert.AreEqual(allSame, protectedMode.AllAreSame(), "AllSame for [" + testId + "]");
            Assert.AreEqual(allSame && !allOn, protectedMode.AllAre(false), "AllOff for [" + testId + "]");

            var state = protectedMode.State;
            Assert.AreEqual(4, state.Count);
            var index = 1;
            foreach (var entry in state)
            {
                Assert.AreEqual(3, entry.Count);
                Assert.AreEqual(index, entry[0]);
                Assert.AreEqual(zones[index - 1], entry[1]);
                Assert.AreEqual("User", entry[2]);
                index++;
            }
        }
    }
}
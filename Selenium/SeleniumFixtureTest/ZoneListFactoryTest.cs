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

using System.Linq;
using System.Runtime.Versioning;
using DotNetWindowsRegistry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    [SupportedOSPlatform("windows")]
    public class ZoneListFactoryTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ZoneListFactoryCreateTest()
        {
            var registry = new InMemoryRegistry();
            var zoneListFactory = new ZoneListFactory(registry);
            var zoneList = zoneListFactory.CreateZoneList();
            Assert.AreEqual(4, zoneList.Count, "count is 4");
            var index = 1;
            foreach (var zone in zoneList)
            {
                Assert.AreEqual(index++, zone.Id);
                var expectedItemsList = new[] { string.Empty, "Machine Policies", "User Policies", "User", "Machine" };
                Assert.IsTrue(expectedItemsList.Contains(zone.FoundIn), "[" + zone.FoundIn + " not found");
            }
        }
    }
}

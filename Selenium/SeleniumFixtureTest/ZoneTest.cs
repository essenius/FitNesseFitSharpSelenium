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

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Microsoft.Win32.Fakes;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ZoneTest
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "False positive"),
         SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"), DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             @"zonetest", DataAccessMethod.Sequential), DeploymentItem("test\\SeleniumFixtureTest\\TestData.xml")]
        public void ZoneInitialProtectedModeTest()
        {
            using (ShimsContext.Create())
            {
                var hklmPoliciesValue = TestContext.DataRow["hklmPolicies"].ToString();
                var hkcuPoliciesValue = TestContext.DataRow["hkcuPolicies"].ToString();
                var hklmValue = TestContext.DataRow["hklm"].ToString();
                var hkcuValue = TestContext.DataRow["hkcu"].ToString();

                var hklmShim = ShimFactory.CreateRegistryKey(hklmPoliciesValue, hklmValue);
                var hkcuShim = ShimFactory.CreateRegistryKey(hkcuPoliciesValue, hkcuValue);

                var zone = new Zone(1, hklmShim, hkcuShim);

                var expected = TestContext.DataRow["expectedProtected"].ToBool();
                var testId = TestContext.DataRow["testId"].ToString();
                Assert.AreEqual(expected, zone.IsProtected, testId);
            }
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException)),
         SuppressMessage("ReSharper", "UnusedVariable", Justification = "Forcing exception")]
        public void ZoneIsProtectedInInvalidKeyTest()
        {
            var zone = new Zone(1, Registry.LocalMachine, Registry.CurrentUser);
            var result = zone.IsProtectedIn("wrong key");
        }

        [TestMethod, TestCategory("Unit")]
        public void ZoneNoZoneInformationReturnsProtected()
        {
            using (ShimsContext.Create())
            {
                var hkcuShim = new ShimRegistryKey
                {
                    OpenSubKeyStringBoolean = (key, isWritable) => null
                };
                var hklmShim = new ShimRegistryKey
                {
                    OpenSubKeyStringBoolean = (key, isWritable) => null
                };
                Assert.IsTrue(new Zone(2, hklmShim, hkcuShim).IsProtected);
            }
        }
    }
}
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

using System;
using System.Runtime.Versioning;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Microsoft.Win32.Fakes;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ZoneTest
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("both policies", "0", "3", "", "", true)]
        [DataRow("both hkcu", "", "0", "3", "", true)]
        [DataRow("hkcuPolicies/hklm", "", "3", "", "0", false)]
        [DataRow("hklmPolicies/hkcu", "3", "", "0", "", false)]
        [DataRow("hkcu/hklm", "", "", "0", "3", true)]
        [DataRow("hklm", "", "", "", "0", true)]
        [DataRow("both hklm", "0", "", "", "3", true)]
        public void ZoneInitialProtectedModeTest(string testId, string hklmPoliciesValue, string hkcuPoliciesValue, string hkcuValue,
            string hklmValue, bool expectedProtected)
        {
            if (!OperatingSystem.IsWindows()) return;
            using (ShimsContext.Create())
            {
                var hklmShim = ShimFactory.CreateRegistryKey(hklmPoliciesValue, hklmValue);
                var hkcuShim = ShimFactory.CreateRegistryKey(hkcuPoliciesValue, hkcuValue);
                var zone = new Zone(1, hklmShim, hkcuShim);
                Assert.AreEqual(expectedProtected, zone.IsProtected, testId);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        [SupportedOSPlatform("windows")]
        public void ZoneIsProtectedInInvalidKeyTest()
        {
            var zone = new Zone(1, Registry.LocalMachine, Registry.CurrentUser);
            _ = zone.IsProtectedIn("wrong key");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ZoneNoZoneInformationReturnsProtected()
        {
            if (!OperatingSystem.IsWindows()) return;
            using (ShimsContext.Create())
            {
                var hkcuShim = new ShimRegistryKey
                {
                    OpenSubKeyStringBoolean = (_, _) => null
                };
                var hklmShim = new ShimRegistryKey
                {
                    OpenSubKeyStringBoolean = (_, _) => null
                };
                Assert.IsTrue(new Zone(2, hklmShim, hkcuShim).IsProtected);
            }
        }
    }
}

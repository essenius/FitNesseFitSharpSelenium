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
using DotNetWindowsRegistry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ZoneTest
    {
        [SupportedOSPlatform("windows")]
        private static void AddValue(IRegistryKey baseKey, bool policy, object value)
        {
            if (string.IsNullOrEmpty(value.ToString())) return;
            const string zoneKeyTemplate = @"SOFTWARE\{0}Microsoft\Windows\CurrentVersion\Internet Settings\Zones\1";
            var zoneKey = string.Format(zoneKeyTemplate, policy ? @"Policies\": "");
            var key = baseKey.CreateSubKey(zoneKey);
            key.SetValue("2500", value, RegistryValueKind.DWord);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("both policies", 0, 3, "", "", true)]
        [DataRow("both hkcu", "", 0, 3, "", true)]
        [DataRow("hkcuPolicies/hklm", "", 3, "", 0, false)]
        [DataRow("hklmPolicies/hkcu", 3, "", 0, "", false)]
        [DataRow("hkcu/hklm", "", "", 0, 3, true)]
        [DataRow("hklm", "", "", "", 0, true)]
        [DataRow("both hklm", 0, "", "", 3, true)]
        public void ZoneInitialProtectedModeTest(string testId, object hklmPoliciesValue, object hkcuPoliciesValue, object hkcuValue,
            object hklmValue, bool expectedProtected)
        {
            if (!OperatingSystem.IsWindows()) return;
            var registry = new InMemoryRegistry();
            var hkcu = registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            var hklm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            AddValue(hklm, policy: true, hklmPoliciesValue);
            AddValue(hklm, policy: false, hklmValue);
            AddValue(hkcu, policy: true, hkcuPoliciesValue);
            AddValue(hkcu, policy: false, hkcuValue);

            var zone = new Zone(1, registry);
            Assert.AreEqual(expectedProtected, zone.IsProtected, testId);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        [SupportedOSPlatform("windows")]
        public void ZoneIsProtectedInInvalidKeyTest()
        {
            var zone = new Zone(1, new InMemoryRegistry());
            _ = zone.IsProtectedIn("wrong key");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ZoneNoZoneInformationReturnsProtected()
        {
            if (!OperatingSystem.IsWindows()) return;
            var registry = new InMemoryRegistry();
            Assert.IsTrue(new Zone(2, registry).IsProtected);
        }
    }
}

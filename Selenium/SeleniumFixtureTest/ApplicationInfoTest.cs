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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ApplicationInfoTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ApplicationInfoTestVersionComparison()
        {
            Assert.IsTrue(ApplicationInfo.VersionIsAtLeast("1.9"), "Version is at least 1.1");
            Assert.IsTrue(ApplicationInfo.VersionIsAtLeast("1.9.8"), "Version is at least 1.9.8");
            Assert.IsTrue(ApplicationInfo.VersionIsAtLeast(ApplicationInfo.Version), "Version is at least itself");
            var version = new Version(ApplicationInfo.Version);
            var newVersion = new Version(version.Major, version.Minor, version.Build + 1);
            Assert.IsFalse(ApplicationInfo.VersionIsAtLeast(newVersion.ToString(3)), "Version is not at least 1 build up");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void ApplicationInfoTestVersionWrongParam() => ApplicationInfo.VersionIsAtLeast("1");
    }
}

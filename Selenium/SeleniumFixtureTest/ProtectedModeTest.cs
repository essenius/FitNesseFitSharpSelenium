// Copyright 2015-2024 Rik Essenius
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
using SeleniumFixture.Model;

namespace SeleniumFixtureTest;

[TestClass]
public class ProtectedModeTest
{
    [DataTestMethod]
    [TestCategory("Unit")]
    [DataRow("all off", new[] { false, false, false, false }, false, true)]
    [DataRow("all on", new[] { true, true, true, true }, true, true)]
    [DataRow("two on", new[] { false, true, false, true }, false, false)]
    public void ProtectedModeAllAreTest(string testId, bool[] zones, bool expectedAllOn, bool expectedAllSame)
    {
        if (!OperatingSystem.IsWindows()) return;
        var protectedMode = new ProtectedMode(new ZoneListFactoryMock(zones));

        Assert.AreEqual(expectedAllOn, protectedMode.AllAre(true), "AllOn for [" + testId + "]");
        Assert.AreEqual(expectedAllSame, protectedMode.AllAreSame(), "AllSame for [" + testId + "]");
        Assert.AreEqual(expectedAllSame && !expectedAllOn, protectedMode.AllAre(false), "AllOff for [" + testId + "]");

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
﻿// Copyright 2021-2024 Rik Essenius
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

namespace SeleniumFixtureTest;

// The Internet Explorer driver currently has an issue with timing out on page load.
// see https://gitmemory.com/issue/SeleniumHQ/selenium/7833/567543368 
[TestClass]
public class SeleniumLocalInternetExplorerTest : SeleniumTestBase
{
    [ClassCleanup]
    public static void ClassCleanup() => Test.ClassCleanup();

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        Test.ClassInitialize("ie", false);
        if (!Test.ProtectedModesAreEqual())
        {
            Assert.Inconclusive("Protected Modes are not all equal");
        }
    }
}
// Copyright 2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SeleniumFixtureTest
{
    /// <summary>
    ///     Base class for end to end test classes on different browser configurations, containing the constant parts.
    ///     The child classes will be test classes with a ClassInitialize that differs per configuration
    ///     We can't inherit static classes, so we need to put ClassCleanup in the child as well or it won't get run.
    /// </summary>
    public class SeleniumTestBase
    {
        protected static readonly EndToEndTest Test = new();

        [TestMethod]
        [TestCategory("Browser")]
        [EndToEndTestCases]
        public void RunTest(MethodInfo testCase)
        {
            testCase.Invoke(Test, BindingFlags.DoNotWrapExceptions, null, null, null);
        }

    }
}

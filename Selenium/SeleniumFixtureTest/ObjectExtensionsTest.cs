﻿// Copyright 2015-2019 Rik Essenius
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
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ObjectExtensionsTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void ObjectExtensionsIsGlobTest()
        {
            Assert.IsTrue("ab*f".IsGlob());
            Assert.IsFalse("abc".IsGlob());
        }

        [TestMethod, TestCategory("Unit")]
        public void ObjectExtensionsIsLikeTest()
        {
            Assert.IsTrue(@"abc".IsLike("/a?c/".RegexPattern()));
            Assert.IsFalse(@"abcd".IsLike("/a?d/".RegexPattern()));
            Assert.IsTrue(@"abc".IsLike("/abc/".RegexPattern()));
        }

        [TestMethod, TestCategory("Unit")]
        public void ObjectExtensionsIsRegexTest()
        {
            Assert.IsTrue("//".IsRegex());
            Assert.IsFalse("/a".IsRegex());
            Assert.IsFalse("a/".IsRegex());
            Assert.IsFalse("~=3".IsRegex());
        }

        [TestMethod, TestCategory("Unit")]
        public void ObjectExtensionsMatchesTest()
        {
            Assert.IsTrue(@"abc".Matches("a.c"));
            Assert.IsTrue(@"abcd".Matches("a.*d"));
            Assert.IsFalse(@"abcd".Matches("a.d"));
            Assert.IsTrue(@"abc".Matches("abc"));
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException))]
        public void ObjectExtensionsToBoolExceptionTest() => Assert.IsTrue(ObjectExtensions.ToBool(null));

        [TestMethod, TestCategory("Unit")]
        public void ObjectExtensionsToBoolTest()
        {
            Assert.IsTrue("true".ToBool());
            Assert.IsFalse("false".ToBool());
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("WebDriverBackedSeleniumTestTest\\TestData.xml"),
         DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML",
             "|DataDirectory|\\TestData.xml", "GracefulNames", DataAccessMethod.Sequential)
        ]
        public void ObjectExtensionsToMethodNameTest()
        {
            var input = TestContext.DataRow["input"].ToString();
            var expected = TestContext.DataRow["expected"].ToString();
            var actual = input.ToMethodName();
            Assert.AreEqual(expected, actual, "input: " + input);
        }
    }
}
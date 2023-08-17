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
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class KeyConverterTest
    {
        private Selenium _selenium;

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(ArgumentException), "Could not find end delimiter '}'")]
        public void KeyConverterExceptionTest()
        {
            var _ = new KeyConverter("{wrong").ToSeleniumFormat;
        }

        [TestMethod]
        public void KeyConverterSpecialKeysTest()
        {
            var k = new KeyConverter("^ac{Del}New Text~").ToSeleniumFormat;
            int[] expected = { 57353, 97, 99, 57367, 78, 101, 119, 32, 84, 101, 120, 116, 57351 };
            Assert.AreEqual(expected.Length, k.Length, "expected size {0} but got {1}", expected.Length, k.Length);
            for (var i = 0; i < k.Length; i++)
            {
                Assert.AreEqual(expected[i], k[i], "#{0}: expected {1} but got {2}", i, expected[i], (int)k[i]);
            }
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("all lower case characters", @"abcdefghijklmnopqrstuvwxyz", @"abcdefghijklmnopqrstuvwxyz")]
        [DataRow("all upper case characters", @"ABCDEFGHIJKLMNOPQRSTUVWXYZ", @"ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [DataRow("all digits", @"1234567890", @"1234567890")]
        [DataRow("keyboard symbols other than letters and digits", "`!@#$&*()-_=[];:'\",<.>/?, ", "`!@#$&*()-_=[];:'\",<.>/?, ")]
        [DataRow("special characters {%^~}", "{{}{%}{^}{~}{}}{[abc]}", "{%^~}[abc]")]
        [DataRow("{}", "{}", "")]
        [DataRow(@"{abc}def}", @"{abc}def}", @"abc}def")]
        [DataRow(@"{a 5}", @"{a 5}", @"aaaaa")]
        [DataRow(@"{abc 2}", @"{abc 2}", @"abcabc")]
        public void KeyConverterTest1(string testId, string input, string expected) =>
            Assert.AreEqual(expected, new KeyConverter(input).ToSeleniumFormat, testId);

        [TestInitialize]
        public void KeyConverterTestInitialize() => _selenium = new Selenium();

        [TestMethod]
        [TestCategory("Integration")]
        public void KeyConverterTestInSelenium()
        {
            try
            {
                _selenium.SetTimeoutSeconds(20);
                Assert.IsTrue(_selenium.SetBrowser("chrome"));
                Assert.IsTrue(_selenium.Open(EndToEndTest.CreateTestPageUri()));
                Assert.IsTrue(_selenium.WaitForElement("text1"));
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("^a").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("^c").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("{DEL}").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("My New Text").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text2", new KeyConverter("^a").ToSeleniumFormat), "Text 2");
                Assert.IsTrue(_selenium.SetElementTo("text2", new KeyConverter("{dEl}^v").ToSeleniumFormat), "Text 2");
            }
            finally
            {
                _selenium.Close();
            }
        }
    }
}

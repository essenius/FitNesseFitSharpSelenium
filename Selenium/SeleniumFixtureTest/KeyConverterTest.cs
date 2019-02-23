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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class KeyConverterTest
    {
        private Selenium _selenium;

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException), "Could not find end delimiter '}'")]
        [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Forcing exception")]
        public void KeyConverterExceptionTest()
        {
            var result = new KeyConverter("{wrong").ToSeleniumFormat;
        }

        [TestMethod, TestCategory("Unit")]
        public void KeyConverterTest1()
        {
            Assert.AreEqual(@"abcdefghijklmnopqrstuvwxyz",
                new KeyConverter(@"abcdefghijklmnopqrstuvwxyz").ToSeleniumFormat, "all lower case characters");
            Assert.AreEqual(@"ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                new KeyConverter(@"ABCDEFGHIJKLMNOPQRSTUVWXYZ").ToSeleniumFormat, "all upper case characters");
            Assert.AreEqual("1234567890", new KeyConverter("1234567890").ToSeleniumFormat, "all digits");
            Assert.AreEqual("`!@#$&*()-_=[];:'\",<.>/?,",
                new KeyConverter("`!@#$&*()-_=[];:'\",<.>/?,").ToSeleniumFormat,
                "keyboard symbols other than letters and digits");
            Assert.AreEqual("{%^~}[abc]", new KeyConverter("{{}{%}{^}{~}{}}{[abc]}").ToSeleniumFormat,
                "special characters {%^~}");
            Assert.AreEqual("", new KeyConverter("{}").ToSeleniumFormat, "{}");
            Assert.AreEqual(@"abc}def", new KeyConverter(@"{abc}def}").ToSeleniumFormat, @"{abc}def}");
            Assert.AreEqual(@"aaaaa", new KeyConverter(@"{a 5}").ToSeleniumFormat, @"{a 5}");
            Assert.AreEqual(@"abcabc", new KeyConverter(@"{abc 2}").ToSeleniumFormat, @"{abc 2}");

            var k = new KeyConverter("^ac{DEL}New Text~").ToSeleniumFormat;
            int[] expected = {57353, 97, 99, 57367, 78, 101, 119, 32, 84, 101, 120, 116, 57351};
            Assert.AreEqual(expected.Length, k.Length, "expected size {0} but got {1}", expected.Length, k.Length);
            for (var i = 0; i < k.Length; i++)
            {
                Assert.AreEqual(expected[i], k[i], "#{0}: expected {1} but got {2}", i, expected[i], (int)k[i]);
            }
        }

        [TestInitialize]
        public void KeyConverterTestInitialize() => _selenium = new Selenium();

        [TestMethod, TestCategory("Integration")]
        public void KeyConverterTestInSelenium()
        {
            try
            {
                Assert.IsTrue(_selenium.SetBrowser("chrome"));
                Assert.IsTrue(_selenium.Open(SeleniumBaseTest.CreateTestPageUri()));
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("^a").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("^c").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("{DEL}").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text1", new KeyConverter("My New Text").ToSeleniumFormat), "Text 1");
                Assert.IsTrue(_selenium.SetElementTo("text2", new KeyConverter("^a").ToSeleniumFormat), "Text 2");
                Assert.IsTrue(_selenium.SetElementTo("text2", new KeyConverter("{DEL}^v").ToSeleniumFormat), "Text 2");
            }
            finally
            {
                _selenium.Close();
            }
        }
    }
}
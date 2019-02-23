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
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class JavaScriptFunctionTest
    {
        [TestMethod, TestCategory("Integration")]
        public void JavaScriptFunctionTestWithFibonacci()
        {
            // protected mode must be off for this to work in IE
            var selenium = new Selenium();
            Assert.IsTrue(selenium.SetBrowser("firefox"));
            Assert.IsTrue(selenium.Open(SeleniumBaseTest.CreateTestPageUri()));
            var javaScriptFunction = new JavaScriptFunction();
            javaScriptFunction.Reset();
            javaScriptFunction.Set("value", 10);
            Assert.AreEqual(Convert.ToInt64(55), javaScriptFunction.Get("Fibonacci"));
            javaScriptFunction.Reset();
            javaScriptFunction.Set("value", "aq");
            Assert.AreEqual("Input should be numerical", javaScriptFunction.Get("Fibonacci"));
            Assert.IsTrue(selenium.Close());
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(NoNullAllowedException))]
        public void JavaScriptFunctionTestWithoutBrowser()
        {
            BrowserDriver.Current = null;
            var javaScriptFunction = new JavaScriptFunction();
            javaScriptFunction.Reset();
        }
    }
}
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
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumFixture;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class InternetExplorerDriverCreatorTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(StopTestException),
            "Internet Explorer requires a screen scaling of 100%. Set via Control Panel/Display Settings")]
        public void InternetExplorerDriverCreatorWrongScreenSizeIdentifiedTest()
        {
            var ieDriverCreator = new InternetExplorerDriverCreator(new Proxy { Kind = ProxyKind.System }, TimeSpan.FromSeconds(60));
            var field = ieDriverCreator.GetType().GetField("_nativeMethods", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(ieDriverCreator, new NativeMethodsMock());
            ieDriverCreator.LocalDriver();
        }
    }
}

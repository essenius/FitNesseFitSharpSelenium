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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void SimpleMapTest()
        {
            var test = new Map();
            test.Key("key");
            test.Value("value");
            test.Execute();
            var result = test.Content();
            Assert.IsTrue(result.ContainsKey("key"), "key exists");
            Assert.AreEqual("value", result["key"], "value is correct");
            var test2 = new Map(result);
            Assert.AreEqual(1, test2.Content().Count, "Parameter processed correctly");
        }
    }
}

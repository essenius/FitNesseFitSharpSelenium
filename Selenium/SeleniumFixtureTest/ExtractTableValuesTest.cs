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
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ExtractTableValuesTest
    {
        private Selenium _selenium;

        [TestCleanup]
        public void ExtractTableValuesCleanup() => _selenium.Close();

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedExceptionWithMessage(typeof(NoNullAllowedException), "Browser Driver was not initialized")]
        public void ExtractTableValuesNoDriverTest()
        {
            var extract = new ExtractTableValues(string.Empty);
            extract.Query();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ExtractTableValuesSimpleTest()
        {
            _selenium.SetBrowser("chrome");
            _selenium.Open(EndToEndTest.CreateTestPageUri());
            // Tricky stuff happening here - attempts to logon behind the scenes
            _selenium.WaitUntilTitleMatches("SeleniumFixtureTestPage");
            TestTable("XPath://table[@id='normalTable']", 0, new[]
            {
                new[] { new[] { "header1", "value 1" }, new[] { "header2", "10" } },
                new[] { new[] { "header1", "value 2" }, new[] { "header2", "20" } },
                new[] { new[] { "header1", "value 3" }, new[] { "header2", "30" } }
            });

            TestTable("CssSelector: table#tableWithoutHeaders", 0, new[]
            {
                new[] { new[] { "Column 1", "value 4" }, new[] { "Column 2", "40" } },
                new[] { new[] { "Column 1", "value 5" }, new[] { "Column 2", "50" } }
            });

            TestTable("id:tableWithEmptyHeaders", 0, new[]
            {
                new[] { new[] { "Column 1", "value 6" }, new[] { "Column 2", "60" } },
                new[] { new[] { "Column 1", "value 7" }, new[] { "Column 2", "70" } }
            });
        }

        [TestInitialize]
        public void ExtractTableValuesTestInitialize() => _selenium = new Selenium();

        [TestMethod]
        [TestCategory("Integration")]
        public void KendoTableTest()
        {
            Selenium.SetProxyType("system");
            // we take an old page from WayBack Machine which we know is not changing 
            // this can be slow to load, so we increase the timeout
            Selenium.CommandTimeoutSeconds = 120;
            Assert.IsTrue(_selenium.SetBrowser("firefox"));
            Assert.IsTrue(_selenium.Open(new Uri("https://web.archive.org/web/20150713051625/http://demos.telerik.com/kendo-ui/grid/index")));

            Assert.IsTrue(_selenium.WaitUntilElementIsVisible("XPath://table[@role=\"grid\"]"));
            // tricky: the header and data rows are in different tables with the same role. The fixture can now handle that

            TestTable("XPath://table[@role='grid']", 2, new[]
            {
                new[]
                {
                    new[] { "Contact Name", @"Maria Anders" }, new[] { "Contact Title", "Sales Representative" },
                    new[] { "Company Name", @"Alfreds Futterkiste" }, new[] { "Country", "Germany" }
                },
                new[]
                {
                    new[] { "Contact Name", "Ana Trujillo" }, new[] { "Contact Title", "Owner" },
                    new[] { "Company Name", @"Ana Trujillo Emparedados y helados" }, new[] { "Country", "Mexico" }
                }
            });

            _selenium.ClickElement("LinkText:Contact Name");
            TestTable("XPath://table[@role='grid']", 2, new[]
            {
                new[]
                {
                    new[] { "Contact Name", @"Alejandra Camino" }, new[] { "Contact Title", "Accounting Manager" },
                    new[] { "Company Name", @"Romero y tomillo" }, new[] { "Country", "Spain" }
                },
                new[]
                {
                    new[] { "Contact Name", @"Alexander Feuer" }, new[] { "Contact Title", "Marketing Assistant" },
                    new[] { "Company Name", @"Morgenstern Gesundkost" }, new[] { "Country", "Germany" }
                }
            });
        }

        private static void TestTable(string xPathQuery, int max, string[][][] expectedValues)
        {
            var etv = new ExtractTableValues(xPathQuery, max);
            var table = etv.Query();
            Assert.IsNotNull(table);
            Assert.AreEqual(expectedValues.GetLength(0), etv.RowCount, $"RowCount {xPathQuery}");
            Assert.AreEqual(expectedValues.GetLength(0), table.Count, $"query Row Count {xPathQuery}");
            for (var row = 0; row < table.Count; row++)
            {
                var rowCollection = table[row] as Collection<object>;
                Assert.IsNotNull(rowCollection);
                Assert.AreEqual(expectedValues[0].GetLength(0), etv.ColumnCount, $"etv Column Count {xPathQuery}");
                Assert.AreEqual(expectedValues[0].GetLength(0), rowCollection.Count, $"query Column Count {xPathQuery}");
                for (var column = 0; column < rowCollection.Count; column++)
                {
                    var columnCollection = rowCollection[column] as Collection<object>;
                    Assert.IsNotNull(columnCollection);
                    Assert.AreEqual(2, expectedValues[0][0].GetLength(0), "Cell Count");
                    Assert.AreEqual(expectedValues[row][column][0], columnCollection[0], $"{xPathQuery}({row},{column},{0})");
                    Assert.AreEqual(expectedValues[row][column][1], columnCollection[1], $"{xPathQuery}({row},{column},{1})");
                }
            }
        }
    }
}

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
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class ExtractTableValuesTest
    {
        private static void TestTable(string xPathQuery, int max, string[,,] expectedValues)
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
                Assert.AreEqual(expectedValues.GetLength(1), etv.ColumnCount, $"etv Column Count {xPathQuery}");
                Assert.AreEqual(expectedValues.GetLength(1), rowCollection.Count, $"query Column Count {xPathQuery}");
                for (var column = 0; column < rowCollection.Count; column++)
                {
                    var columnCollection = rowCollection[column] as Collection<object>;
                    Assert.IsNotNull(columnCollection);
                    Assert.AreEqual(2, expectedValues.GetLength(2), "Cell Count");
                    Assert.AreEqual(expectedValues[row, column, 0], columnCollection[0], $"{xPathQuery}({row},{column},{0})");
                    Assert.AreEqual(expectedValues[row, column, 1], columnCollection[1], $"{xPathQuery}({row},{column},{1})");
                }
            }
        }

        private Selenium _selenium;

        [TestCleanup]
        public void ExtractTableValuesCleanup() => _selenium.Close();

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(NoNullAllowedException), "Browser Driver was not initialized")]
        public void ExtractTableValuesNoDriverTest()
        {
            var extract = new ExtractTableValues(string.Empty);
            extract.Query();
        }

        [TestMethod, TestCategory("Integration")]
        public void ExtractTableValuesSimpleTest()
        {
            _selenium.SetBrowser("chrome");
            _selenium.Open(SeleniumBaseTest.CreateTestPageUri());
            TestTable("XPath://table[@id='normalTable']", 0, new[,,]
            {
                {{"header1", "value 1"}, {"header2", "10"}},
                {{"header1", "value 2"}, {"header2", "20"}},
                {{"header1", "value 3"}, {"header2", "30"}}
            });

            TestTable("CssSelector: table#tableWithoutHeaders", 0, new[,,]
            {
                {{"Column 1", "value 4"}, {"Column 2", "40"}},
                {{"Column 1", "value 5"}, {"Column 2", "50"}}
            });

            TestTable("id:tableWithEmptyHeaders", 0, new[,,]
            {
                {{"Column 1", "value 6"}, {"Column 2", "60"}},
                {{"Column 1", "value 7"}, {"Column 2", "70"}}
            });
        }

        [TestInitialize]
        public void ExtractTableValuesTestInitialize() => _selenium = new Selenium();

        [TestMethod, TestCategory("Integration")]
        public void KendoTableTest()
        {
            _selenium.SetBrowser("chrome");
            _selenium.Open(new Uri("http://demos.telerik.com/kendo-ui/grid/index"));

            // tricky: the header and data rows are in different tables with the same role. The fixture can now handle that

            TestTable("XPath://table[@role='grid']", 2, new[,,]
            {
                {
                    {"Contact Name", @"Maria Anders"}, {"Contact Title", "Sales Representative"},
                    {"Company Name", @"Alfreds Futterkiste"}, {"Country", "Germany"}
                },
                {
                    {"Contact Name", "Ana Trujillo"}, {"Contact Title", "Owner"},
                    {"Company Name", @"Ana Trujillo Emparedados y helados"}, {"Country", "Mexico"}
                }
            });

            _selenium.ClickElement("LinkText:Contact Name");
            TestTable("XPath://table[@role='grid']", 2, new[,,]
            {
                {
                    {"Contact Name", @"Alejandra Camino"}, {"Contact Title", "Accounting Manager"},
                    {"Company Name", @"Romero y tomillo"}, {"Country", "Spain"}
                },
                {
                    {"Contact Name", @"Alexander Feuer"}, {"Contact Title", "Marketing Assistant"},
                    {"Company Name", @"Morgenstern Gesundkost"}, {"Country", "Germany"}
                }
            });
        }
    }
}
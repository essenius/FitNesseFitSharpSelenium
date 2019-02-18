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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;

namespace SeleniumFixture
{
    /// <summary>
    ///     Fixture to extract table values from an HTML table
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public class ExtractTableValues
    {
        private static string DefaultHeader(int columnNumber) => string.Format(CultureInfo.CurrentCulture, "Column {0}", columnNumber + 1);

        /// <summary>
        ///     Store function documentation in the assembly Used by FixtureExplorer
        /// </summary>
        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Extract table values from an HTML table"},
            {"`2", "Extract table values from an HTML table, limit number of results"},
            {
                "`3",
                "Extract table values from an HTML table; import the headers and data for the table from different parent tables"
            },
            {
                "`4",
                "Extract table values from an HTML table; import the headers and data for the table from different parent tables and limit # of results"
            },
            {
                "`5",
                "Extract table values from an HTML table; import the headers and data for the table from different parent tables and limit # of results"
            },
            {nameof(ColumnCount), "Number of columns in the table"},
            {nameof(Query), "Query table interface returning the content of the table"},
            {nameof(RowCount), "Number of data rows in the table (excluding header)"}
        };

        private readonly string _headerLocation;
        private readonly int _maxResults;
        private readonly string _relativeCellLocationInRow;
        private readonly string _relativeHeaderLocationInTable;
        private readonly string _relativeRowLocationInTable;
        private readonly string _rowLocation;
        private readonly string _tableLocation;
        private int _columnCount;

        private Collection<object> _result;
        private int _rowCount;

        private bool _wasCalculated;

        /// <summary>
        ///     Import the table search criterion to identify the table we are interested in (XPath format).
        /// </summary>
        /// <param name="tableLocation">The XPath search criterion to identify the table.</param>
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "FitSharp can't handle optional params")]
        public ExtractTableValues(string tableLocation) : this(tableLocation, 0)
        {
        }

        /// <summary>
        ///     Import the table search criteria to identify the table we are interested in.
        /// </summary>
        /// <param name="tableLocation">The search criterion to identify the table.</param>
        /// <param name="maxResults">Maximum number of data rows to return</param>
        public ExtractTableValues(string tableLocation, int maxResults) :
            this(tableLocation, "XPath:thead/tr/th", "XPath:tbody/tr", "XPath:td", maxResults)
        {
        }

        /// <summary>
        ///     Import the headers and data for the table from different parent tables
        /// </summary>
        /// <param name="headerLocation">Absolute locator for the header cells</param>
        /// <param name="rowLocation">Absolute locator for the data rows</param>
        /// <param name="maxResults">Maximum number of data rows to return</param>
        public ExtractTableValues(string headerLocation, string rowLocation, int maxResults) :
            this(headerLocation, rowLocation, "XPath:td", maxResults)
        {
        }

        /// <summary>
        ///     Import the headers and data for the table from different parent tables 1
        /// </summary>
        /// <param name="headerLocation">Absolute locator for the header cells (collection)</param>
        /// <param name="rowLocation">Absolute locator for the data rows (collection)</param>
        /// <param name="relativeCellLocationInRow">Relative locator (from rows) for the data cells</param>
        /// <param name="maxResults">Maximum number of data rows to return</param>
        public ExtractTableValues(string headerLocation, string rowLocation, string relativeCellLocationInRow, int maxResults)
        {
            _headerLocation = headerLocation;
            _rowLocation = rowLocation;
            _relativeCellLocationInRow = relativeCellLocationInRow;
            _maxResults = maxResults;
        }

        /// <summary>
        ///     Import the headers and data for the table from different parent tables
        /// </summary>
        /// <param name="tableLocation">The locator for hte table element</param>
        /// <param name="relativeHeaderLocationInTable">Relative locator (to the table) for the header cells</param>
        /// <param name="relativeRowLocationInTable">Relative locator (to the table) for the data rows</param>
        /// <param name="relativeCellLocationInRow">Relative locator (from rows) for the data cells</param>
        /// <param name="maxResults">Maximum number of data rows to return</param>
        public ExtractTableValues(string tableLocation, string relativeHeaderLocationInTable,
            string relativeRowLocationInTable, string relativeCellLocationInRow, int maxResults)
        {
            _tableLocation = tableLocation;
            _relativeHeaderLocationInTable = relativeHeaderLocationInTable;
            _relativeRowLocationInTable = relativeRowLocationInTable;
            _relativeCellLocationInRow = relativeCellLocationInRow;
            _maxResults = maxResults;
        }

        /// <summary>
        ///     Number of columns in the extracted table
        /// </summary>
        public int ColumnCount
        {
            get
            {
                DoExtraction();
                return _columnCount;
            }
        }

        /// <summary>
        ///     Number of rows in the extracted table
        /// </summary>
        public int RowCount
        {
            get
            {
                DoExtraction();
                return _rowCount;
            }
        }

        private void DoExtraction()
        {
            if (_wasCalculated) return;
            if (BrowserDriver.Current == null) throw new NoNullAllowedException("Browser Driver was not initialized");
            var headerCollection = new Collection<string>();
            ReadOnlyCollection<IWebElement> headerElements;
            IEnumerable<IWebElement> rowElements;
            if (string.IsNullOrEmpty(_tableLocation))
            {
                headerElements = BrowserDriver.Current.FindElements(new SearchParser(_headerLocation).By);
                rowElements = BrowserDriver.Current.FindElements(new SearchParser(_rowLocation).By);
            }
            else
            {
                var tableElements = BrowserDriver.Current.FindElements(new SearchParser(_tableLocation).By);
                headerElements = tableElements.FindElements(new SearchParser(_relativeHeaderLocationInTable).By);
                rowElements = tableElements.FindElements(new SearchParser(_relativeRowLocationInTable).By);
            }

            foreach (var element in headerElements)
            {
                headerCollection.Add(string.IsNullOrEmpty(element.Text)
                    ? DefaultHeader(headerCollection.Count)
                    : element.Text.Replace("\r\n", " ").Trim());
            }

            var rowCollection = new Collection<object>();
            if (_maxResults > 0) rowElements = rowElements.Take(_maxResults);
            foreach (var rowElement in rowElements)
            {
                var cellCollection = new Collection<object>();
                var index = 0;
                foreach (var cell in rowElement.FindElements(new SearchParser(_relativeCellLocationInRow).By))
                {
                    // if we didn't have a header row, make one up
                    if (headerCollection.Count <= index)
                    {
                        headerCollection.Add(DefaultHeader(index));
                    }

                    cellCollection.Add(new Collection<object> {headerCollection[index++], cell.Text.Trim()});
                }

                rowCollection.Add(cellCollection);
            }

            _result = rowCollection;
            _columnCount = headerCollection.Count;
            _rowCount = rowCollection.Count;
            _wasCalculated = true;
        }

        /// <summary>
        ///     Executes the query to extract cell values from a table element.
        ///     Assumes that the BrowserDriver is already initialized,
        /// </summary>
        /// <returns>the table in a collection of rows, where each row is a collection of key/value pairs</returns>
        /// <exception cref="NoNullAllowedException">if the browser driver was not initialized</exception>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">if an element could not be found</exception>
        public Collection<object> Query()
        {
            DoExtraction();
            return _result;
        }
    }
}
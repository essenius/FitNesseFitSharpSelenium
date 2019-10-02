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
using System.Linq;
using OpenQA.Selenium;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public class ExtractTableValues
    {
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

        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "FitSharp can't handle optional params")]
        [Documentation("Extract table values from an HTML table, identified by an XPath query")]
        public ExtractTableValues(string tableLocation) : this(tableLocation, 0)
        {
        }

        [Documentation("Extract table values from an HTML table, limit number of results")]
        public ExtractTableValues(string tableLocation, int maxResults) :
            this(tableLocation, "XPath:thead/tr/th", "XPath:tbody/tr", "XPath:td", maxResults)
        {
        }

        [Documentation("Extract table values from an HTML table; import the headers and data for the table from different parent tables, " +
                       "and limit number of results. Uses absolute header locator and absolute data row locator")]
        public ExtractTableValues(string headerLocation, string rowLocation, int maxResults) :
            this(headerLocation, rowLocation, "XPath:td", maxResults)
        {
        }

        [Documentation("Extract table values from an HTML table; import the headers and data for the table from different parent " +
                       "tables and limit # of results. Uses absolute header location, absolute row location and relative cell locator (from rows)")]
        public ExtractTableValues(string headerLocation, string rowLocation, string relativeCellLocationInRow, int maxResults)
        {
            _headerLocation = headerLocation;
            _rowLocation = rowLocation;
            _relativeCellLocationInRow = relativeCellLocationInRow;
            _maxResults = maxResults;
        }

        [Documentation("Extract table values from an HTML table; import the headers and data for the table from different parent " +
                       "tables and limit # of results. Uses absolute table locator, relative (to table) header, relative(to table) data rows, " +
                       "and relative (to row) cell locator")]
        public ExtractTableValues(string tableLocation, string relativeHeaderLocationInTable,
            string relativeRowLocationInTable, string relativeCellLocationInRow, int maxResults)
        {
            _tableLocation = tableLocation;
            _relativeHeaderLocationInTable = relativeHeaderLocationInTable;
            _relativeRowLocationInTable = relativeRowLocationInTable;
            _relativeCellLocationInRow = relativeCellLocationInRow;
            _maxResults = maxResults;
        }

        [Documentation("Number of data rows in the table (excluding header)")]
        public int ColumnCount
        {
            get
            {
                DoExtraction();
                return _columnCount;
            }
        }

        [Documentation("Number of rows in the extracted table")]
        public int RowCount
        {
            get
            {
                DoExtraction();
                return _rowCount;
            }
        }

        private static string DefaultHeader(int columnNumber) => string.Format(CurrentCulture, "Column {0}", columnNumber + 1);

        private void DoExtraction()
        {
            if (_wasCalculated) return;
            if (BrowserDriverContainer.Current == null) throw new NoNullAllowedException("Browser Driver was not initialized");
            ReadOnlyCollection<IWebElement> headerElements;
            IEnumerable<IWebElement> rowElements;
            if (string.IsNullOrEmpty(_tableLocation))
            {
                headerElements = BrowserDriverContainer.Current.FindElements(new SearchParser(_headerLocation).By);
                rowElements = BrowserDriverContainer.Current.FindElements(new SearchParser(_rowLocation).By);
            }
            else
            {
                var tableElements = BrowserDriverContainer.Current.FindElements(new SearchParser(_tableLocation).By);
                headerElements = tableElements.FindElements(new SearchParser(_relativeHeaderLocationInTable).By);
                rowElements = tableElements.FindElements(new SearchParser(_relativeRowLocationInTable).By);
            }

            var headerCollection = HeaderCollection(headerElements);

            if (_maxResults > 0) rowElements = rowElements.Take(_maxResults);

            _result = RowCollection(rowElements, headerCollection);
            _columnCount = headerCollection.Count;
            _rowCount = _result.Count;
            _wasCalculated = true;
        }

        private static Collection<string> HeaderCollection(IEnumerable<IWebElement> headerElements)
        {
            var headerCollection = new Collection<string>();
            foreach (var element in headerElements)
            {
                headerCollection.Add(string.IsNullOrEmpty(element.Text)
                    ? DefaultHeader(headerCollection.Count)
                    : element.Text.Replace("\r\n", " ").Trim());
            }
            return headerCollection;
        }

        private Collection<object> RowCollection(IEnumerable<IWebElement> rowElements, IList<string> headerList)
        {
            var rowCollection = new Collection<object>();
            foreach (var rowElement in rowElements)
            {
                var cellCollection = new Collection<object>();
                var index = 0;
                foreach (var cell in rowElement.FindElements(new SearchParser(_relativeCellLocationInRow).By))
                {
                    // if we didn't have a header row, make one up
                    if (headerList.Count <= index) headerList.Add(DefaultHeader(index));
                    cellCollection.Add(new Collection<object> {headerList[index++], cell.Text.Trim()});
                }
                rowCollection.Add(cellCollection);
            }
            return rowCollection;
        }

        [Documentation("Query table interface returning the content of the table. Assumes that the Browser Driver was already initialized")]
        public Collection<object> Query()
        {
            DoExtraction();
            return _result;
        }
    }
}
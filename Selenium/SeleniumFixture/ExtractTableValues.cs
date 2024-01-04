// Copyright 2015-2024 Rik Essenius
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
using System.Linq;
using OpenQA.Selenium;
using SeleniumFixture.Model;
using SeleniumFixture.Utilities;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture;

/// <summary>Extract values from an HTML table</summary>
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

    /// <summary>Extract table values from an HTML table, identified by an XPath query</summary>
    public ExtractTableValues(string tableLocation) : this(tableLocation, 0)
    {
    }

    /// <summary>Extract table values from an HTML table, limit number of results</summary>
    public ExtractTableValues(string tableLocation, int maxResults) :
        this(tableLocation, "XPath:thead/tr/th", "XPath:tbody/tr", "XPath:td", maxResults)
    {
    }

    /// <summary>
    /// Extract table values from an HTML table; import the headers and data for the table from
    /// different parent tables and limit number of results
    /// </summary>
    /// <param name="headerLocation">absolute header location</param>
    /// <param name="rowLocation">absolute data row location</param>
    /// <param name="maxResults">maximum result count</param>
    public ExtractTableValues(string headerLocation, string rowLocation, int maxResults) :
        this(headerLocation, rowLocation, "XPath:td", maxResults)
    {
    }

    /// <summary>
    /// Extract table values from an HTML table; import the headers
    /// and data for the table from different parent
    /// </summary>
    /// <param name="headerLocation">absolute header location</param>
    /// <param name="rowLocation">absolute row location</param>
    /// <param name="relativeCellLocationInRow">cell locator relative from row</param>
    /// <param name="maxResults">maximum result count</param>
    public ExtractTableValues(
        string headerLocation, 
        string rowLocation, 
        string relativeCellLocationInRow,
        int maxResults)
    {
        _headerLocation = headerLocation;
        _rowLocation = rowLocation;
        _relativeCellLocationInRow = relativeCellLocationInRow;
        _maxResults = maxResults;
    }

    /// <summary>
    /// Extract table values from an HTML table; import the headers and data for the table
    /// from different parent tables and limit # of results
    /// </summary>
    /// <param name="tableLocation">absolute table locator</param>
    /// <param name="relativeHeaderLocationInTable">header locator relative to table</param>
    /// <param name="relativeRowLocationInTable">data row locator relative to table</param>
    /// <param name="relativeCellLocationInRow">cell locator relative to row</param>
    /// <param name="maxResults">maximum result count</param>
    public ExtractTableValues(
        string tableLocation,
        string relativeHeaderLocationInTable,
        string relativeRowLocationInTable,
        string relativeCellLocationInRow,
        int maxResults)
    {
        _tableLocation = tableLocation;
        _relativeHeaderLocationInTable = relativeHeaderLocationInTable;
        _relativeRowLocationInTable = relativeRowLocationInTable;
        _relativeCellLocationInRow = relativeCellLocationInRow;
        _maxResults = maxResults;
    }

    /// <summary>Number of data rows in the table (excluding header)</summary>
    public int ColumnCount
    {
        get
        {
            DoExtraction();
            return _columnCount;
        }
    }

    /// <summary>Number of rows in the extracted table</summary>
    public int RowCount
    {
        get
        {
            DoExtraction();
            return _rowCount;
        }
    }

    private static string DefaultHeader(int columnNumber) =>
        string.Format(CurrentCulture, "Column {0}", columnNumber + 1);

    private void DoExtraction()
    {
        if (_wasCalculated) return;
        if (BrowserDriverContainer.Current == null)
        {
            throw new NoNullAllowedException("Browser Driver was not initialized");
        }
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

    /// <summary>Query table interface returning the content of the table. Assumes that the Browser Driver was already initialized</summary>
    public Collection<object> Query()
    {
        DoExtraction();
        return _result;
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
                cellCollection.Add(new Collection<object> { headerList[index++], cell.Text.Trim() });
            }
            rowCollection.Add(cellCollection);
        }
        return rowCollection;
    }
}
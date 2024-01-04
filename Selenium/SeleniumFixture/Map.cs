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

namespace SeleniumFixture;

/// <summary>
/// Map (Dictionary) that can be used as e.g. capabilities object
/// </summary>
public class Map
{
    private readonly Dictionary<string, string> _map;
    private string _key;
    private string _value;

    /// <summary>create empty map</summary>
    public Map()
    {
        _map = new Dictionary<string, string>();
    }

    /// <summary>create map</summary>
    /// <param name="input">initial map</param>
    public Map(Dictionary<string, string> input) => _map = input;

    /// <summary>Set key for key-value pair to be added</summary>
    public void Key(string input) => _key = input;

    /// <summary>Set value for key-value pair to be added</summary>
    public void Value(string input) => _value = input;

    /// <summary>Execute a line (part of FitNesse interface)</summary>
    public void Execute()
    {
        if (_key == null) return;
        _map.Add(_key, _value);
    }

    /// <summary>Prepare for next test case</summary>
    public void Reset()
    {
        _key = null;
        _value = null;
    }

    /// <returns>the map</returns>
    public Dictionary<string, string> Content() => _map;
}
using System;
using System.Collections.Generic;


namespace SeleniumFixture
{

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

        /// <summary> Set key for key-value pair to be added</summary>
        public void Key(string input) => _key = input;

        /// <summary> Set value for key-value pair to be added</summary>
        public void Value(string input) => _value = input;

        /// <summary> Execute a line (part of FitNesse interface)</summary>
        public void Execute()
        {
            if (_key == null) return;
            _map.Add(_key, _value.ToString());
        }

        public void Reset()
        {
            _key = null;
            _value = null;
        }

        /// <returns>the map</returns>
        public Dictionary<string, string> Content() => _map;


    }
}

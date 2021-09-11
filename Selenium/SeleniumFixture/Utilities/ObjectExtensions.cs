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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture.Utilities
{
    /// <summary>
    ///     Web Driver Extensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        public static bool IsGlob(this string input) => input.Contains("*") || input.Contains("?");

        public static bool IsLike(this string input, string pattern) =>
            Matches(input, "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");

        public static bool IsRegex(this string input) =>
            input.StartsWith("/", StringComparison.CurrentCulture) && input.EndsWith("/", StringComparison.CurrentCulture);

        public static bool Matches(this string input, string pattern) =>
            new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(input);

        public static string RegexPattern(this string input)
        {
            Debug.Assert(input.IsRegex());
            return input[1..^1];
        }

        /// <summary>
        ///     Convert the return value of a JavaScript command to boolean
        /// </summary>
        /// <param name="value">the value (expected to be something parseable to a boolean)</param>
        /// <returns>the boolean representation of the input value</returns>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "False positive")]
        public static bool ToBool(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return bool.Parse(value.ToString());
        }

        /// <summary>
        ///     Convert a graceful name to the corresponding method (or property) name.
        ///     If the name doesn't have spaces, it only capitalizes the first letter. Otherwise it capitalizes the first letter
        ///     of each word and makes the rest lower case.
        /// </summary>
        /// <param name="gracefulName">the graceful name (can be with spaces)</param>
        /// <returns>The method name (PascalCase)</returns>
        public static string ToMethodName(this string gracefulName)
        {
            if (string.IsNullOrEmpty(gracefulName)) return gracefulName;
            if (!gracefulName.Contains(" ")) return char.ToUpper(gracefulName[0], CurrentCulture) + gracefulName[1..];
            var textInfo = CurrentCulture.TextInfo;
            var result = string.Empty;
            var sections = gracefulName.Split(' ');
            return sections.Select(section => textInfo.ToTitleCase(textInfo.ToLower(section)))
                .Aggregate(result, (current, capitalizedSection) => current + capitalizedSection);
        }
    }
}

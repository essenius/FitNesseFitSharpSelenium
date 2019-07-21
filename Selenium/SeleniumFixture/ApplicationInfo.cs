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

using static System.FormattableString;

namespace SeleniumFixture
{
    internal static class ApplicationInfo
    {
        public const string ApplicationName = "SeleniumFixture";
        public const string Author = "Rik Essenius";
        public const string Copyright = "Copyright © Rik Essenius 2015-2019";
        public const string Description = "A FitNesse fixture to drive Selenium WebDriver";

        public const string Version = "2.4.1";
        // don't forget to update the release notes

        public static string ExtendedInfo => Invariant($"{ApplicationName} {Version}. {Description}. {Copyright}");

        public static string VersionInfo(string qualifier)
        {
            switch (qualifier.ToUpperInvariant())
            {
                case "SHORT":
                    return Version;
                case "EXTENDED":
                    return ExtendedInfo;
                default:
                    return Invariant($"{ApplicationName} {Version}");
            }
        }
    }
}
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static System.FormattableString;

namespace SeleniumFixture
{
    /// <summary>Provide fixture metadata</summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    public static class ApplicationInfo
    {
        /// <summary>Name of the fixture</summary>
        public static string ApplicationName { get; } = ThisAssembly.GetName().Name;

        /// <summary>Copyright notice</summary>
        public static string Copyright { get; } =
            ThisAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        /// <summary> Description of the fixture</summary>
        public static string Description { get; } =
            ThisAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        /// <summary> Name, version, description and copyright</summary>
        public static string ExtendedInfo => Invariant($"{ApplicationName} {Version}. {Description}. {Copyright}");

        private static Assembly ThisAssembly => Assembly.GetExecutingAssembly();

        /// <summary>Version of the fixture</summary>
        public static string Version { get; } = ThisAssembly.GetName().Version?.ToString();

        /// <summary> Version info</summary>
        /// <param name="qualifier">Short or Extended</param>
        public static string VersionInfo(string qualifier)
        {
            return qualifier.ToUpperInvariant() switch
            {
                "SHORT" => Version,
                "EXTENDED" => ExtendedInfo,
                _ => Invariant($"{ApplicationName} {Version}")
            };
        }

        /// <summary>Check for minimal version</summary>
        /// <param name="versionString">minimally required version</param>
        /// <returns>true if version is at least that</returns>
        public static bool VersionIsAtLeast(string versionString)
        {
            var versionCompared = new Version(versionString);
            var version = new Version(Version);
            return version.CompareTo(versionCompared) >= 0;
        }
    }
}

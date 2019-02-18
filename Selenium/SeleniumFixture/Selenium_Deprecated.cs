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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using SeleniumFixture.Model;

namespace SeleniumFixture
{
    /// <summary>
    ///     Deprecated and experimental methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Entry point for FitSharp")]
    public sealed partial class Selenium
    {
        private const string ApplicationNameObsoleteMessage =
            "use of Application Name is no longer supported since the W3C spec is enforced. Use New/Set Remote Browser At Address";

        /// <summary>
        ///     Throw an exception if a deprecated function is used.
        /// </summary>
        public static bool ExceptionOnDeprecatedFunctions { get; set; }

        private static void HandleDeprecatedFunction(string fixtureName, string fixtureReplacement)
        {
            if (ExceptionOnDeprecatedFunctions)
            {
                throw new WarningException("Use of deprecated function '" + fixtureName + "'. Replace by '" +
                                           fixtureReplacement + "'");
            }
        }

        /// <summary>
        ///     Set remote browser with a name. No longer supported, so throwing an exception
        /// </summary>
        /// <param name="browserName">The browser to be used</param>
        /// <param name="baseAddress">the address that the browser driver can be found at (including port)</param>
        /// <param name="name">applicationName to be used</param>
        /// <returns>Driver ID</returns>
        [Obsolete(ApplicationNameObsoleteMessage)]
        public static string NewRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string name) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + name + ")");

        /// <summary>
        ///     Use Remote driver for browser on a certain address (including port) and specify an applicationName
        ///     to be able to select a specific node if running on a hub.
        /// </summary>
        /// <param name="browserName">Unused</param>
        /// <param name="baseAddress">Unused</param>
        /// <param name="applicationName">Unused</param>
        /// <returns>whether or not the operation succeeded</returns>
        [Obsolete(ApplicationNameObsoleteMessage)]
        public static bool SetRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string applicationName) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + applicationName + ")");
    }
}
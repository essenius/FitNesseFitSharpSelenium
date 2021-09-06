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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using SeleniumFixture.Model;

// If we disable XML documents in this file, it's because the documentation is in the Obsolete tags.
#pragma warning disable 1591

namespace SeleniumFixture
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Entry point for FitSharp")]
    public sealed partial class Selenium
    {
        private const string ApplicationNameObsoleteMessage =
            "use of Application Name is no longer supported since the W3C spec is enforced. Use New/Set Remote Browser At Address";

        /// <summary>Check for using deprecated functions by throwing an exception if a deprecated function is used.</summary>
        public static bool ExceptionOnDeprecatedFunctions { get; set; }

        [Obsolete("Use WindowSize")]
        public int WindowHeight
        {
            get
            {
                HandleDeprecatedFunction("Window Height", "Window Size");
                return Driver.Manage().Window.Size.Height;
            }
        }

        [Obsolete("Use WindowSize")]
        public int WindowWidth
        {
            get
            {
                HandleDeprecatedFunction("Window Width", "Window Size");
                return Driver.Manage().Window.Size.Width;
            }
        }

        private static void HandleDeprecatedFunction(string fixtureName, string fixtureReplacement)
        {
            if (ExceptionOnDeprecatedFunctions)
            {
                throw new WarningException("Use of deprecated function '" + fixtureName + "'. Replace by '" + fixtureReplacement + "'");
            }
        }

        [Obsolete("Use PageSource instead")]
        public string HtmlSource()
        {
            HandleDeprecatedFunction("HTML Source", "Page Source");
            return PageSource;
        }

        [Obsolete("Use LengthOfPageSource instead")]
        public int LengthOfHtmlSource()
        {
            HandleDeprecatedFunction("Length Of HTML Source", "Length Of Page Source");
            return LengthOfPageSource;
        }

        [Obsolete(ApplicationNameObsoleteMessage)]
        public static string NewRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string name) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + name + ")");

        [Obsolete("Use ProtectedModesAre")]
        public bool ProtectedModeIsOff()
        {
            HandleDeprecatedFunction("Protected Mode Is Off", "Protected Modes Are");
            return ProtectedModesAre("Off");
        }

        [Obsolete("Use ProtectedModesAre")]
        public bool ProtectedModeIsOn()
        {
            HandleDeprecatedFunction("Protected Mode Is On", "Protected Modes Are");
            return ProtectedModesAre("On");
        }

        [Obsolete("Use ProtectedModesAre")]
        public bool ProtectedModesAreEqual()
        {
            HandleDeprecatedFunction("Protected Modes Are Equal", "Protected Modes Are");
            return ProtectedModesAre("Equal");
        }

        [Obsolete(ApplicationNameObsoleteMessage)]
        public static bool SetRemoteBrowserAtAddressWithName(string browserName, string baseAddress, string applicationName) =>
            throw new NotSupportedException(ApplicationNameObsoleteMessage +
                                            " (" + browserName + "," + baseAddress + "," + applicationName + ")");

        [Obsolete("Use WindowSize")]
        public bool SetWindowSizeX(int width, int height)
        {
            HandleDeprecatedFunction("Set Window Size X", "Set Window Size");
            var newSize = new Coordinate(width, height);
            WindowSize = newSize;
            return newSize.CloseTo(WindowSize);
        }

        [Obsolete("Use WaitForPageSourceToChange")]
        public bool WaitForHtmlSourceToChange()
        {
            HandleDeprecatedFunction("Wait For HTML Source To Change", "Wait For Page Source To Change");
            return WaitForPageSourceToChange();
        }

        [Obsolete("use WaitUntilElementDoesNotExist instead")]
        public bool WaitForNoElement(string searchCriterion)
        {
            HandleDeprecatedFunction("Wait For No Element", "Wait Until Element Does Not Exist");
            return WaitFor(drv =>
            {
                try
                {
                    return drv.FindElement(new SearchParser(searchCriterion).By) == null;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
        }

        [Obsolete("Use WaitUntilPageSourceIsLargerThan")]
        public bool WaitUntilHtmlSourceIsLargerThan(int thresholdLength)
        {
            HandleDeprecatedFunction("Wait Until HTML Source Is Larger Than", "Wait Until Page Source Is Larger Than");
            return WaitUntilPageSourceIsLargerThan(thresholdLength);
        }
    }
}

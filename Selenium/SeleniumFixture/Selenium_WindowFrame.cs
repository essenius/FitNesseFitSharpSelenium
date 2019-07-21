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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using OpenQA.Selenium;

namespace SeleniumFixture
{
    /// <summary>
    ///     Window and frame handling methods of the Selenium script table fixture for FitNesse
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "FitSharp can't handle optional parameters")]
    public sealed partial class Selenium
    {
        private string _mainWindowHandle;

        [Documentation("Return the cached window handles (debugging purposes)")]
        internal ReadOnlyCollection<string> CachedWindowHandles { get; private set; }

        [Documentation("Return the internal name (handle) of the current browser window. This can be used later in a Select Window command")]
        public string CurrentWindowName => Driver.CurrentWindowHandle;

        [Documentation("Returns the window handles (debugging purposes)")]
        internal ReadOnlyCollection<string> WindowHandles => Driver.WindowHandles;

        [Documentation("The dimensions of the browser window (width, height)")]
        public Coordinate WindowSize
        {
            get
            {
                var size = Driver.Manage().Window.Size;
                return new Coordinate(size.Width, size.Height);
            }
            set
            {
                var window = Driver.Manage().Window;
                if (value == null) throw new ArgumentNullException(nameof(WindowSize), ErrorMessages.SizeIsNoPair);
                window.Size = new Size(value.X, value.Y);
            }
        }

        [Documentation("Accept an alert, confirm or prompt dialog (press OK)")]
        public bool AcceptAlert()
        {
            if (!WaitForAlert()) return false;
            Driver.SwitchTo().Alert().Accept();
            return WaitForAlertToClose();
        }

        [Documentation("Check whether an alert, confirm or prompt box is active")]
        public bool AlertIsPresent()
        {
            try
            {
                Driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }

        [Documentation("Dismiss an alert, confirm or prompt dialog (press Cancel)")]
        public bool DismissAlert()
        {
            if (!WaitForAlert()) return false;
            Driver.SwitchTo().Alert().Dismiss();
            return WaitForAlertToClose();
        }

        [Documentation("Maximize browser window")]
        public void MaximizeWindow() => Driver.Manage().Window.Maximize();

        [Documentation("Provide a text response to a prompt and confirm (press OK)")]
        public bool RespondToAlert(string text)
        {
            if (!WaitForAlert()) return false;
            Driver.SwitchTo().Alert().SendKeys(text);
            Driver.SwitchTo().Alert().Accept();
            return WaitFor(drv => !AlertIsPresent());
        }

        [Documentation("Selects a window using a window handle (which was returned using Wait For New Window Name or Current Window Name). " +
                       "If no handle is specified, it will select the window that was used for the Open command.")]
        public bool SelectWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) windowName = _mainWindowHandle;
            Driver.SwitchTo().Window(windowName);
            StoreWindowHandles(false);
            return true;
        }

        [Documentation("Stores all the window handles that the browser driver handles, including the main window.")]
        public void StoreWindowHandles() => StoreWindowHandles(true);

        [Documentation("Stores all the window handles that the browser driver handles")]
        public void StoreWindowHandles(bool setMain)
        {
            if (setMain || _mainWindowHandle == null) _mainWindowHandle = Driver.CurrentWindowHandle;
            CachedWindowHandles = Driver.WindowHandles;
        }

        [Documentation("Switch to the root html context (i.e. leave frame context)")]
        public bool SwitchToDefaultContext() => Driver.SwitchTo().DefaultContent() != null;

        [Documentation("Switch context to a frame in the current html page")]
        public bool SwitchToFrame(string searchCriterion) => DoOperationOnElement(searchCriterion, element =>
        {
            Driver.SwitchTo().Frame(element);
            return true;
        });

        internal bool WaitForAlert() => WaitFor(drv => AlertIsPresent());

        internal bool WaitForAlertToClose() => WaitFor(drv => !AlertIsPresent());

        [Documentation("After clicking a link that is known to open a new window, wait for that new window to appear. Returns the window name")]
        public string WaitForNewWindowName()
        {
            var returnValue = WaitFor(drv =>
            {
                var newHandles = drv.WindowHandles.Where(x => !CachedWindowHandles.Contains(x));
                var enumerable = newHandles as IList<string> ?? newHandles.ToList();
                return enumerable.Count > 0 ? enumerable.FirstOrDefault() : null;
            });
            StoreWindowHandles(false);
            return returnValue;
        }

        [Documentation("Check if the window size is close enough to the specified size")]
        public bool WindowSizeIsCloseTo(Coordinate comparison) => WindowSize.CloseTo(comparison);
    }
}
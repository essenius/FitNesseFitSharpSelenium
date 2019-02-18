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
using System.Diagnostics;
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

        /// <summary>
        ///     Returns cached window handles (for debugging purposes).
        /// </summary>
        public ReadOnlyCollection<string> CachedWindowHandles { get; private set; }

        /// <summary>
        ///     Return the current window handle (to be used in Select Window)
        /// </summary>
        public string CurrentWindowName => Driver.CurrentWindowHandle;

        /// <summary>
        ///     Returns the window handles (for debugging purposes).
        /// </summary>
        /// <returns>String Collection of window handles </returns>
        public ReadOnlyCollection<string> WindowHandles => Driver.WindowHandles;

        /// <summary>
        ///     Return the height of the browser window
        /// </summary>
        public int WindowHeight => Driver.Manage().Window.Size.Height;

        /// <summary>
        ///     Return the width of the browser window
        /// </summary>
        public int WindowWidth => Driver.Manage().Window.Size.Width;

        /// <summary>
        ///     Accept (OK) an alert
        /// </summary>
        public void AcceptAlert()
        {
            Driver.SwitchTo().Alert().Accept();
            WaitFor(drv => !AlertIsPresent());
        }

        /// <summary>
        ///     Check if an alert is shown.
        /// </summary>
        public bool AlertIsPresent()
        {
            try
            {
                Driver.SwitchTo().Alert();
                Debug.Print("Found alert");
                return true;
            }
            catch (NoAlertPresentException)
            {
                Debug.Print("No alert");
                return false;
            }
        }

        /// <summary>
        ///     Dismiss (Cancel) an alert
        /// </summary>
        public void DismissAlert()
        {
            Driver.SwitchTo().Alert().Dismiss();
            WaitFor(drv => !AlertIsPresent());
        }

        /// <summary>
        ///     Maximize the browser window
        /// </summary>
        public void MaximizeWindow()
        {
            Driver.Manage().Window.Maximize();
        }

        /// <summary>
        ///     Respond to a prompt
        /// </summary>
        /// <param name="text">text to be provided to the prompt</param>
        public void RespondToAlert(string text)
        {
            Driver.SwitchTo().Alert().SendKeys(text);
            AcceptAlert();
            WaitFor(drv => !AlertIsPresent());
        }

        /// <summary>
        ///     Selects a window using a window handle (which was returned using <see cref="WaitForNewWindowName" />).
        /// </summary>
        /// <param name="windowName">Name of the window.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchWindowException" />
        /// <returns>true</returns>
        public bool SelectWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName))
            {
                windowName = _mainWindowHandle;
            }

            Driver.SwitchTo().Window(windowName);
            StoreWindowHandles(false);
            return true;
        }

        /// <summary>
        ///     Set the size of a browser window (width x height)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>whether the size was set exactly</returns>
        public bool SetWindowSizeX(int width, int height)
        {
            var window = Driver.Manage().Window;
            window.Size = new Size(width, height);
            // ChromeDriver is off a bit sometimes. Working around that.
            return Math.Abs(window.Size.Width - width) <=2 && 
                   Math.Abs(window.Size.Height - height) <= 2;
        }

        /// <summary>
        ///     Stores all the window handles that the browser driver handles, including the main window.
        /// </summary>
        public void StoreWindowHandles()
        {
            StoreWindowHandles(true);
        }

        /// <summary>
        ///     Stores all the window handles that the browser driver handles.
        /// </summary>
        public void StoreWindowHandles(bool setMain)
        {
            if (setMain || _mainWindowHandle == null) _mainWindowHandle = Driver.CurrentWindowHandle;
            CachedWindowHandles = Driver.WindowHandles;
        }

        /// <summary>
        ///     Switch back to the default page (after switching to another iframe)
        /// </summary>
        /// <returns>whether it succeeded</returns>
        public bool SwitchToDefaultContext() => Driver.SwitchTo().DefaultContent() != null;

        /// <summary>
        ///     Switch context to an iFrame
        /// </summary>
        /// <param name="searchCriterion">Criterion (method:locator)</param>
        /// <returns>whether it succeeded</returns>
        public bool SwitchToFrame(string searchCriterion)
        {
            return DoOperationOnElement(searchCriterion, element =>
            {
                var frame = Driver.SwitchTo().Frame(element);
                Debug.Print("Frame title:" + frame.Title);
                return true;
            });
        }

        /// <summary>
        ///     After clicking a link that is known to open a new window, wait for that new window to appear
        /// </summary>
        /// <returns>the name of the new window</returns>
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
    }
}
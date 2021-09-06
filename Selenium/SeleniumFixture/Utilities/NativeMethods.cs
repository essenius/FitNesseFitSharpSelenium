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
using System.Drawing;
using System.Runtime.InteropServices;

namespace SeleniumFixture.Utilities
{
    internal class NativeMethods
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        // Internet Explorer doesn't like it if we use anything else than 100% screen scaling (as set in the Windows Settings)
        internal virtual bool ScreenScalingIs1()
        {
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var desktop = graphics.GetHdc();
            var logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VerticalHeightInPixels);
            var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DesktopVerticalHeightInPixels);
            return logicalScreenHeight == physicalScreenHeight;
        }

        // see http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        private enum DeviceCap
        {
            VerticalHeightInPixels = 10,
            DesktopVerticalHeightInPixels = 117
        }
    }
}

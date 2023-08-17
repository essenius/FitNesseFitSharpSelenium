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

using OpenQA.Selenium;

namespace SeleniumFixture.Model
{
    /// <summary>
    ///     Simulate HTML5 Drag and Drop actions by injecting and executing JavaScript functions
    ///     This is needed because the default Selenium drag/drop method does not work on HTML5 drag and drop
    ///     see e.g. http://elementalselenium.com/tips/39-drag-and-drop
    ///     It is a simplified C# port of the Java based simulator referred to in
    ///     https://code.google.com/p/selenium/issues/detail?id=6315#c5
    ///     That one only worked in Firefox; this revised one has been tested on Internet Explorer 9,
    ///     Chrome 39, Firefox 32, and PhantomJS 1.9.8 (all Windows). For IE9, compatibility mode should be off.
    ///     The JavaScript functions have no dependency on jQuery so should work in any decent browser.
    /// </summary>
    internal static class DragDrop
    {
        private const string JavaScriptEventSimulator =
            "function createDragEvent(eventName, options) {\r\n" +
            "   var event = createMouseEvent(eventName, options);\r\n" +
            "	var dataTransfer = {\r\n" +
            "		data: options.dragData == null ? {} : options.dragData,\r\n" +
            "		setData: function(eventName, val){\r\n" +
            "			if (typeof val === 'string') {\r\n" +
            "				this.data[eventName] = val;\r\n" +
            "			}\r\n" +
            "		},\r\n" +
            "		getData: function(eventName){\r\n" +
            "			return this.data[eventName];\r\n" +
            "		},\r\n" +
            "		clearData: function(){\r\n" +
            "			return this.data = {};\r\n" +
            "		},\r\n" +
            "		setDragImage: function(dragElement, x, y) {}\r\n" +
            "	};\r\n" +
            "	event.dataTransfer = dataTransfer;\r\n" +
            "	return event;\r\n" +
            "}\r\n" +
            "function createMouseEvent(eventName, options) {\r\n" +
            "   var event = document.createEvent(\"CustomEvent\");\r\n" +
            "	event.initCustomEvent(eventName, true, true, null);\r\n" +
            "	event.view = window;\r\n" +
            "	event.detail = 0;\r\n" +
            "	event.screenX = window.screenX + options.clientX;\r\n" +
            "	event.screenY = window.screenY + options.clientY;\r\n" +
            "	event.clientX = options.clientX;\r\n" +
            "	event.clientY = options.clientY;\r\n" +
            "	event.ctrlKey = false;\r\n" +
            "	event.altKey = false;\r\n" +
            "	event.shiftKey = false;\r\n" +
            "	event.metaKey = false;\r\n" +
            "	event.button = 0;\r\n" +
            "	event.relatedTarget = null;\r\n" +
            "	return event;\r\n" +
            "}\r\n" +
            "function dispatchEvent(webElement, eventName, event) {\r\n" +
            "	if (webElement.dispatchEvent) {\r\n" +
            "		webElement.dispatchEvent(event);\r\n" +
            "	} else if (webElement.fireEvent) {\r\n" +
            "		webElement.fireEvent(\"on\"+eventName, event);\r\n" +
            "	}\r\n" +
            "}\r\n" +
            "function simulateEventCall(element, eventName, dragStartEvent, options) {\r\n" +
            "	var event = null;\r\n" +
            "	if (eventName.indexOf(\"mouse\") > -1) {\r\n" +
            "		event = createMouseEvent(eventName, options);\r\n" +
            "	} else {\r\n" +
            "		event = createDragEvent(eventName, options);\r\n" +
            "	}\r\n" +
            "	if (dragStartEvent != null) {\r\n" +
            "		event.dataTransfer = dragStartEvent.dataTransfer;\r\n" +
            "	}\r\n" +
            "	dispatchEvent(element, eventName, event);\r\n" +
            "	return event;\r\n" +
            "}\r\n";

        private const string SimulateEvent = JavaScriptEventSimulator +
                                             "function simulateEvent(element, eventName, clientX, clientY, dragData) {\r\n" +
                                             "	return simulateEventCall(element, eventName, null, {clientX: clientX, clientY: clientY, dragData: dragData});\r\n" +
                                             "}\r\n" +
                                             "var event = simulateEvent(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);\r\n" +
                                             "if (event.dataTransfer != null) {\r\n" +
                                             "	return event.dataTransfer.data;\r\n" +
                                             "}\r\n";

        private const string SimulateHtml5DragAndDrop = JavaScriptEventSimulator +
                                                        "function simulateHTML5DragAndDrop(dragFrom, dragTo, dragFromX, dragFromY, dragToX, dragToY) {\r\n" +
                                                        "	var mouseDownEvent = simulateEventCall(dragFrom, \"mousedown\", null, {clientX: dragFromX, clientY: dragFromY});\r\n" +
                                                        "	var dragStartEvent = simulateEventCall(dragFrom, \"dragstart\", null, {clientX: dragFromX, clientY: dragFromY});\r\n" +
                                                        "	var dragEnterEvent = simulateEventCall(dragTo,   \"dragenter\", dragStartEvent, {clientX: dragToX, clientY: dragToY});\r\n" +
                                                        "	var dragOverEvent  = simulateEventCall(dragTo,   \"dragover\",  dragStartEvent, {clientX: dragToX, clientY: dragToY});\r\n" +
                                                        "	var dropEvent      = simulateEventCall(dragTo,   \"drop\",      dragStartEvent, {clientX: dragToX, clientY: dragToY});\r\n" +
                                                        "	var dragEndEvent   = simulateEventCall(dragFrom, \"dragend\",   dragStartEvent, {clientX: dragToX, clientY: dragToY});\r\n" +
                                                        "}\r\n" +
                                                        "simulateHTML5DragAndDrop(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);\r\n";

        /// <summary>
        ///     Cross-Window Drag And Drop Example
        /// </summary>
        /// <param name="dragFromDriver">The WebDriver where the drag starts</param>
        /// <param name="dragFromElement">The WebElement to drag from</param>
        /// <param name="dropToDriver">The WebDriver where the drag ends</param>
        /// <param name="dropToElement">The WebElement to drag to</param>
        public static void DragToWindow(
            IWebDriver dragFromDriver, IWebElement dragFromElement, IWebDriver dropToDriver, IWebElement dropToElement)
        {
            // Drag start
            Html5SimulateEvent(dragFromDriver, dragFromElement, @"mousedown", Position.Center, null);
            var dragData = Html5SimulateEvent(dragFromDriver, dragFromElement, @"dragstart", Position.Center, null);
            dragData = Html5SimulateEvent(dragFromDriver, dragFromElement, @"dragenter", Position.Center, dragData);
            dragData = Html5SimulateEvent(dragFromDriver, dragFromElement, @"dragleave", Position.Left, dragData);
            dragData = Html5SimulateEvent(dragFromDriver, dragFromDriver.FindElement(By.TagName("body")), @"dragleave",
                Position.Left, dragData);

            // Drag to other window
            Html5SimulateEvent(
                dropToDriver, dropToDriver.FindElement(By.TagName("body")), @"dragenter", Position.Right, null);
            Html5SimulateEvent(dropToDriver, dropToElement, @"dragenter", Position.Right, null);
            Html5SimulateEvent(dropToDriver, dropToElement, @"dragover", Position.Center, null);
            dragData = Html5SimulateEvent(dropToDriver, dropToElement, @"drop", Position.Center, dragData);
            Html5SimulateEvent(dragFromDriver, dragFromElement, @"dragend", Position.Center, dragData);
        }

        /// <summary>
        ///     Drags and drops a web element from source to target
        /// </summary>
        /// <param name="driver">The WebDriver to execute on</param>
        /// <param name="dragFrom">The WebElement to drag from</param>
        /// <param name="dragTo">The WebElement to drag to</param>
        /// <param name="dragFromX">The X position to click relative to the top-left-corner of the client</param>
        /// <param name="dragFromY">The Y position to click relative to the top-left-corner of the client</param>
        /// <param name="dragToX">The X position to release relative to the top-left-corner of the client</param>
        /// <param name="dragToY">The Y position to release relative to the top-left-corner of the client</param>
        private static void Html5DragAndDrop(
            IWebDriver driver, IWebElement dragFrom, IWebElement dragTo,
            int dragFromX, int dragFromY, int dragToX, int dragToY)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                SimulateHtml5DragAndDrop, dragFrom, dragTo, dragFromX, dragFromY, dragToX, dragToY);
        }

        /// <summary>
        ///     Drags and drops a web element from source to target
        /// </summary>
        /// <param name="driver">The WebDriver to execute on</param>
        /// <param name="dragFrom">The WebElement to drag from</param>
        /// <param name="dragTo">The WebElement to drag to</param>
        /// <param name="dragFromPosition">The place to click on the dragFrom</param>
        /// <param name="dragToPosition">The place to release on the dragTo</param>
        public static void Html5DragAndDrop(
            IWebDriver driver,
            IWebElement dragFrom,
            IWebElement dragTo,
            Position dragFromPosition,
            Position dragToPosition)
        {
            var fromLocation = dragFrom.Location;
            var toLocation = dragTo.Location;
            var fromSize = dragFrom.Size;
            var toSize = dragTo.Size;

            // Get Client X and Client Y locations
            var dragFromX = fromLocation.X + dragFromPosition.GetX(fromSize.Width);
            var dragFromY = fromLocation.Y + dragFromPosition.GetY(fromSize.Height);
            var dragToX = toLocation.X + dragToPosition.GetX(toSize.Width);
            var dragToY = toLocation.Y + dragToPosition.GetY(toSize.Height);

            Html5DragAndDrop(driver, dragFrom, dragTo, dragFromX, dragFromY, dragToX, dragToY);
        }

        /// <summary>
        ///     Calls a drag event
        /// </summary>
        /// <param name="driver">The WebDriver to execute on</param>
        /// <param name="dragFrom">The WebElement to simulate on</param>
        /// <param name="eventName">The event name to call</param>
        /// <param name="clientX">The mouse click X position on the screen</param>
        /// <param name="clientY">The mouse click Y position on the screen</param>
        /// <param name="data">The data transfer data</param>
        /// <returns>The updated data transfer data</returns>
        private static object Html5SimulateEvent(
            IWebDriver driver,
            IWebElement dragFrom,
            string eventName,
            int clientX,
            int clientY,
            object data) =>
            ((IJavaScriptExecutor)driver).ExecuteScript(SimulateEvent, dragFrom, eventName, clientX, clientY, data);

        /// <summary>
        ///     Calls a drag event
        /// </summary>
        /// <param name="driver">The WebDriver to execute on</param>
        /// <param name="dragFrom">The WebElement to simulate on</param>
        /// <param name="eventName">The event name to call</param>
        /// <param name="mousePosition">The mouse click area in the element</param>
        /// <param name="data">The data transfer data</param>
        /// <returns>The updated data transfer data</returns>
        private static object Html5SimulateEvent(
            IWebDriver driver,
            IWebElement dragFrom,
            string eventName,
            Position mousePosition,
            object data)
        {
            var fromLocation = dragFrom.Location;
            var fromSize = dragFrom.Size;
            var clientX = fromLocation.X + mousePosition.GetX(fromSize.Width);
            var clientY = fromLocation.Y + mousePosition.GetY(fromSize.Height);
            return Html5SimulateEvent(driver, dragFrom, eventName, clientX, clientY, data);
        }

        /// <summary>
        ///     Allows referring to positions in a rectangle
        /// </summary>
        public class Position
        {
            public static readonly Position Bottom = new();
            public static readonly Position BottomLeft = new();
            public static readonly Position BottomRight = new();
            public static readonly Position Center = new();
            public static readonly Position Left = new();
            public static readonly Position Right = new();
            public static readonly Position Top = new();
            public static readonly Position TopLeft = new();
            public static readonly Position TopRight = new();

            /// <summary>
            ///     Calculate relative X coordinate corresponding to a position
            /// </summary>
            /// <param name="width">the width of the rectangle</param>
            /// <returns>the X coordinate relative to the width</returns>
            public int GetX(int width)
            {
                if (TopLeft.Equals(this) || Left.Equals(this) || BottomLeft.Equals(this)) return 1;
                if (Top.Equals(this) || Center.Equals(this) || Bottom.Equals(this)) return width / 2;
                if (TopRight.Equals(this) || Right.Equals(this) || BottomRight.Equals(this)) return width - 1;
                return 0;
            }

            /// <summary>
            ///     Calculate relative Y coordinate corresponding to a position
            /// </summary>
            /// <param name="height">the height of the rectangle</param>
            /// <returns>the Y coordinate relative to the height</returns>
            public int GetY(int height)
            {
                if (TopLeft.Equals(this) || Top.Equals(this) || TopRight.Equals(this)) return 1;
                if (Left.Equals(this) || Center.Equals(this) || Right.Equals(this)) return height / 2;
                if (BottomLeft.Equals(this) || Bottom.Equals(this) || BottomRight.Equals(this)) return height - 1;
                return 0;
            }
        }
    }
}

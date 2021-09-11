// Copyright 2021 Rik Essenius
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
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace SeleniumFixture.Model
{
    internal class CustomBy: By
    {
        protected readonly string ElementIdentifier;
        protected readonly List<By> ByList = new();
            
        protected string DisplayName;

        public static By IdOrName(string selector) => new ByIdOrName(selector);

        public static By Label(string selector) => new ByLabel(selector);
        public static By Content(string selector) => new ByContent(selector);
        public static By PartialContent(string selector) => new ByPartialContent(selector);
        public static By Trial(string selector) => new ByTrial(selector);

        /// <summary>Make this a virtual class by making its constructor protected</summary>
        protected CustomBy(string elementIdentifier)
        {
            ElementIdentifier = !string.IsNullOrEmpty(elementIdentifier) 
                ? elementIdentifier 
                : throw new ArgumentException(@"element identifier cannot be null or the empty string", nameof(elementIdentifier));
        }

        /// <summary>Find a single element.</summary>
        /// <param name="context">Context used to find the element.</param>
        /// <returns>The element that matches</returns>
        public override IWebElement FindElement(ISearchContext context)
        {
            NoSuchElementException lastException = null;
            foreach (var by in ByList)
            {
                try
                {
                    return by.FindElement(context);
                }
                catch (NoSuchElementException ex)
                {
                    lastException = ex;
                }
            }
            Debug.Assert(lastException != null, nameof(lastException) + " != null");
            throw new NoSuchElementException($"Could not find element {DisplayName}", lastException);
        }

        /// <summary>Finds many elements</summary>
        /// <param name="context">Context used to find the element.</param>
        /// <returns>A readonly collection of elements that match.</returns>
        public override ReadOnlyCollection<IWebElement> FindElements(ISearchContext context)
        {
            var webElementList = new List<IWebElement>();
            foreach (var by in ByList)
            {
                try
                {
                    webElementList.AddRange(by.FindElements(context));
                }
                catch (NoSuchElementException)
                {
                    // ignore
                }
            }
            return webElementList.AsReadOnly();
        }

        /// <summary>Writes out a description of this By object.</summary>
        /// <returns>Converts the value of this instance to a <see cref="T:System.String" /></returns>
        public override string ToString() => $"{DisplayName}({ElementIdentifier})";
    }
}


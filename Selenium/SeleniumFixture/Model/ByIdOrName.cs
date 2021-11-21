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

using OpenQA.Selenium.Appium;

namespace SeleniumFixture.Model
{
    /// <summary>
    ///     Finds element on text, id or the name attribute has the specified value.
    /// </summary>
    internal class ByIdOrName : CustomBy
    {
        public ByIdOrName(string elementIdentifier) : base(elementIdentifier)
        {
            DisplayName = nameof(ByIdOrName);
            ByList.Add(Id(ElementIdentifier));
            ByList.Add(Name(ElementIdentifier));
        }
    }
}


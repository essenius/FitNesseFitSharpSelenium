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

namespace SeleniumFixture.Model
{
    /// <summary> Finds element on associated label content.</summary>
    internal class ByLabel : CustomBy
    {
        public ByLabel(string elementIdentifier) : base(elementIdentifier)
        {
            DisplayName = "ByLabel";
            // check for the label text with and without colon
            var criterion = elementIdentifier.Trim();
            if (criterion.EndsWith(":")) criterion = criterion[..^1];
            AddToByListFor(criterion);
            AddToByListFor(criterion + ":");
        }

        private void AddToByListFor(string criterion)
        {
            foreach (var qualifier in new [] { "preceding", "following" })
            {
                // elements with an id and a parent label having a matching for attribute
                ByList.Add(XPath($"//*[@id=parent::label/@for and {qualifier}-sibling::*[text[normalize-space()=\"{criterion}\"]]]"));
                // elements with an id and a sibling label with a matching for attribute
                ByList.Add(XPath($"//*[@id={qualifier}-sibling::label[text()[normalize-space()=\"{criterion}\"]]/@for]"));
            }
            // input elements with a parent label having a matching text
            foreach (var element in new [] { "input", "meter", "progress", "select", "textArea"})
            {
                ByList.Add(XPath($"//label[text()[normalize-space()=\"{criterion}\"]]//{element}"));
            }
        }
    }
}

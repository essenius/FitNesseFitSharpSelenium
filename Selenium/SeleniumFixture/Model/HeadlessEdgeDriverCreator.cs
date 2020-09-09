// Copyright 2015-2020 Rik Essenius
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
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;

namespace SeleniumFixture.Model
{
    internal class HeadlessEdgeDriverCreator : EdgeDriverCreator
    {
        public HeadlessEdgeDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
        {
        }

        public override string Name { get; } = "EDGEHEADLESS";

        protected override EdgeOptions EdgeOptions()
        {
            var options = base.EdgeOptions();
            options.AddArguments("headless"); 
            return options;
        }
    }
}

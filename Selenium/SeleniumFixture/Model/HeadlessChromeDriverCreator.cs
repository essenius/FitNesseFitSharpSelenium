﻿// Copyright 2015-2024 Rik Essenius
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumFixture.Model;

internal class HeadlessChromeDriverCreator(Proxy proxy, TimeSpan timeout) : ChromeDriverCreator(proxy, timeout)
{
    public override string Name => @"CHROMEHEADLESS";

    protected override ChromeOptions ChromeOptions()
    {
        var options = base.ChromeOptions();
        // see https://bugs.chromium.org/p/chromium/issues/detail?id=737678 for why disable-gpu
        options.AddArguments("headless=new", "disable-gpu");
        return options;
    }
}
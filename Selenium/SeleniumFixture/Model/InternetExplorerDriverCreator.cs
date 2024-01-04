// Copyright 2015-2024 Rik Essenius
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
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;

namespace SeleniumFixture.Model;

internal class InternetExplorerDriverCreator : BrowserDriverCreator
{
    public InternetExplorerDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
    {
    }

    public override string Name => "IE";

    private static string EdgePath()
    {
        var edgePath = AppConfig.Get("InternetExplorer.EdgePath");
        if (!string.IsNullOrEmpty(edgePath)) return edgePath;
        const string defaultSubPath = @"\Microsoft\Edge\Application\msedge.exe";
        var programFilesX86 = AppConfig.Get("ProgramFiles(x86)");
        var defaultEdgePath = $"{programFilesX86}{defaultSubPath}";
        if (File.Exists(defaultEdgePath)) return defaultEdgePath;
        var programFiles = AppConfig.Get("ProgramFiles");
        defaultEdgePath = $"{programFiles}{defaultSubPath}";
        return File.Exists(defaultEdgePath) ? defaultEdgePath : null;
    }

    private static bool IgnoreProtectedModeSetting()
    {
        var ignoreProtectedModeSettingsString = AppConfig.Get("InternetExplorer.IgnoreProtectedModeSettings");
        return !string.IsNullOrEmpty(ignoreProtectedModeSettingsString) &&
               bool.Parse(ignoreProtectedModeSettingsString);
    }

    private InternetExplorerOptions InternetExplorerOptions()
    {
        var options = new InternetExplorerOptions
        {
            Proxy = Proxy,
            IntroduceInstabilityByIgnoringProtectedModeSettings = IgnoreProtectedModeSetting()
        };
        var edgePath = EdgePath();

        if (string.IsNullOrEmpty(edgePath)) return options;
        options.AttachToEdgeChrome = true;
        options.EdgeExecutablePath = edgePath;
        return options;
    }

    public override IWebDriver LocalDriver(object options)
    {
        var driverFolder = ConfiguredFolder("IEWebDriver");
        InternetExplorerDriverService driverService = null;
        IWebDriver driver;
        try
        {
            driverService = GetDefaultService<InternetExplorerDriverService>(driverFolder);
            var ieOptions = options == null ? InternetExplorerOptions() : (InternetExplorerOptions)options;
            driver = new InternetExplorerDriver(driverService, ieOptions, Timeout);
        }
        catch
        {
            driverService?.Dispose();
            throw;
        }
        return driver;
    }

    public override DriverOptions Options() => InternetExplorerOptions();
}
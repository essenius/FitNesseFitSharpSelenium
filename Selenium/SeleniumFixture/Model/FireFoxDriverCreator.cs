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
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace SeleniumFixture.Model;

internal class FireFoxDriverCreator : BrowserDriverCreator
{
    public FireFoxDriverCreator(Proxy proxy, TimeSpan timeout) : base(proxy, timeout)
    {
    }

    public static string IntegratedAuthenticationDomain { get; set; } =
        AppConfig.Get("Firefox.IntegratedAuthenticationDomain");

    public override string Name => "FIREFOX";

    protected virtual FirefoxOptions FirefoxOptions()
    {
        // TODO: check out https://stackoverflow.com/questions/43960301/using-http-proxy-with-selenium-geckodriver/43961118#43961118

        var options = new FirefoxOptions { Proxy = Proxy };

        // As of FF49 Firefox can trust Root authorities in the windows certificate store. 
        // For more details see https://bugzilla.mozilla.org/show_bug.cgi?id=1265113
        // Unfortunately, the feature is off by default. This setting enables it.
        // For more details see https://bugzilla.mozilla.org/show_bug.cgi?id=1314010
        options.SetPreference("security.enterprise_roots.enabled", true);

        if (!string.IsNullOrEmpty(IntegratedAuthenticationDomain))
        {
            options.SetPreference(@"network.automatic-ntlm-auth.trusted-uris", IntegratedAuthenticationDomain);
            options.SetPreference(@"network.negotiate-auth.delegation-uris", IntegratedAuthenticationDomain);
            options.SetPreference(@"network.negotiate-auth.trusted-uris", IntegratedAuthenticationDomain);
        }

        // workaround for bug, see: https://github.com/mozilla/geckodriver/issues/1068
        options.SetPreference(@"security.sandbox.content.level", 5);

        // enable Silverlight
        options.SetPreference(@"plugin.state.npctrl", 2);
        options.AcceptInsecureCertificates = true;
        return options;
    }

    public override IWebDriver LocalDriver(object options)
    {
        var driverFolder = ConfiguredFolder("GeckoWebDriver");
        FirefoxDriverService driverService = null;
        IWebDriver driver;
        try
        {
            driverService = GetDefaultService<FirefoxDriverService>(driverFolder);
            // Workaround for the issue making .NET Core networking slow with GeckoDriver.
            // see https://github.com/SeleniumHQ/selenium/issues/7840
            //driverService.Host = "::1";
            var firefoxOptions = options == null ? FirefoxOptions() : (FirefoxOptions)options;
            driver = new FirefoxDriver(driverService, firefoxOptions, Timeout);
        }
        catch
        {
            driverService?.Dispose();
            throw;
        }
        return driver;
    }

    public override DriverOptions Options() => FirefoxOptions();
}
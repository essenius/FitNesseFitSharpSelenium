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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SeleniumFixture.Model;

internal abstract class BrowserDriverCreator(Proxy proxy, TimeSpan timeout)
{
    protected readonly Proxy Proxy = proxy;
    protected readonly TimeSpan Timeout = timeout;

    public abstract string Name { get; }

    protected static Uri BaseUri(string baseAddress) => new(baseAddress);

    protected static string ConfiguredFolder(string specificFolder)
    {
        var driverFolder = AppConfig.Get(specificFolder);
        if (string.IsNullOrEmpty(driverFolder))
        {
            driverFolder = AppConfig.Get("DriverFolder");
        }
        return driverFolder;
    }

    // This is a workaround for the absence of an IDriverService interface, using reflection
    // T is the driver service to be instantiated (e.g. ChromeDriverService)
    internal static T GetDefaultService<T>(string driverFolder = null)
    {
        var serviceType = typeof(T);
        var typeList = new List<Type>();
        var parameterList = new List<object>();
        if (!string.IsNullOrEmpty(driverFolder))
        {
            typeList.Add(typeof(string));
            parameterList.Add(driverFolder);
        }

        var methodInfo = serviceType.GetMethod("CreateDefaultService", typeList.ToArray());
        Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
        try
        {
            return (T) methodInfo.Invoke(serviceType, parameterList.ToArray());
        }
        catch (TargetInvocationException)
        {
            // We might not need it, so ignore for now
            return default;
        }
    }

    // I tried to make these methods smarter (eliminate redundancy) e.g. via generics, but that is not easy with all the hard dependencies
    // that browser drivers have on their services and options. So I decided to live with the redundancy for now.
    // So, all the driver creators currently have their own implementation.
    public abstract IWebDriver LocalDriver(object options);

    public abstract DriverOptions Options();

    public virtual IWebDriver RemoteDriver(string baseAddress, DriverOptions options)
    {
        var uri = BaseUri(baseAddress);
        options ??= Options();
        // We're being hit by this issue: https://github.com/SeleniumHQ/selenium/issues/12475
        // Appium doesn't use it, so disabling the proxy for now. TODO: monitor resolution of the issue and revert when fixed.
        options.Proxy = null;
        var result = new RemoteWebDriver(uri, options.ToCapabilities(), Timeout);
            
        return result;
    }
}
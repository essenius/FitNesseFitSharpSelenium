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
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SeleniumFixtureTest;

[AttributeUsage(AttributeTargets.Method)]
public class EndToEndTestCasesAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo methodInfo) => EndToEndTest.TestCases;

    public string GetDisplayName(MethodInfo methodInfo, object[] data)
    {
        if (data == null) return null;
        var endToEndMethod = data[0] as MethodInfo;
        return endToEndMethod == null
            ? null
            : string.Format(CultureInfo.CurrentCulture, "{0} - {1}", methodInfo.ReflectedType?.Name, endToEndMethod.Name);
    }
}
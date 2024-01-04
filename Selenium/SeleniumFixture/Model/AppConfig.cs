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

using Microsoft.Extensions.Configuration;

namespace SeleniumFixture.Model;

internal static class AppConfig
{
    private static IConfigurationBuilder _builder;
    private static IConfigurationRoot _root;

    private static IConfigurationRoot Root
    {
        get
        {
            if (_root != null) return _root;
            // order is important. Later in the list means more priority
            _builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
            _root = _builder.Build();
            return _root;
        }
    }

    public static string Get(string name) => Root[name];
}
using System.Resources;
// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SeleniumFixture;

[assembly: AssemblyTitle(ApplicationInfo.ApplicationName)]
[assembly: AssemblyDescription(ApplicationInfo.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(ApplicationInfo.Author)]
[assembly: AssemblyProduct(ApplicationInfo.ApplicationName)]
[assembly: AssemblyCopyright(ApplicationInfo.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("889c1884-e77a-4ff3-a55d-30bce399fae2")]
[assembly: AssemblyVersion(ApplicationInfo.Version)]
[assembly: InternalsVisibleTo("SeleniumFixtureTest")]
[assembly: NeutralResourcesLanguage("en-US")]


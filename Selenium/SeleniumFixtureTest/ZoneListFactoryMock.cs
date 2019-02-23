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

using System.Collections.Generic;
using SeleniumFixture.Model;

namespace SeleniumFixtureTest
{
    internal class ZoneListFactoryMock : IZoneListFactory
    {
        private readonly bool[] _isProtected;

        public ZoneListFactoryMock(bool[] isProtected) => _isProtected = isProtected;

        public List<IZone> CreateZoneList()
        {
            var zoneList = new List<IZone>();
            for (var zone = Zone.MinValue; zone <= Zone.MaxValue; zone++) zoneList.Add(Create(zone));
            return zoneList;
        }

        private IZone Create(int id) => new ZoneMock(id, _isProtected[id - 1]);
    }
}
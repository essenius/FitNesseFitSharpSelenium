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
using System.Collections.ObjectModel;
using System.Linq;

namespace SeleniumFixture.Model
{
    internal class ProtectedMode
    {
        private readonly List<IZone> _zones;

        public ProtectedMode(IZoneListFactory zoneListFactory) => _zones = zoneListFactory.CreateZoneList();

        public Collection<Collection<object>> State
        {
            get
            {
                var response = new Collection<Collection<object>>();
                foreach (var zone in _zones)
                {
                    var row = new Collection<object> { zone.Id, zone.IsProtected, zone.FoundIn };
                    response.Add(row);
                }

                return response;
            }
        }

        public bool AllAre(bool state) => _zones.All(zone => zone.IsProtected == state);

        public bool AllAreSame()
        {
            var orResult = false;
            var andResult = true;
            foreach (var zone in _zones)
            {
                orResult = orResult || zone.IsProtected;
                andResult = andResult && zone.IsProtected;
            }

            return orResult == andResult;
        }
    }
}

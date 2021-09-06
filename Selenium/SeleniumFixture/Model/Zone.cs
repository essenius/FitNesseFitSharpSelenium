// Copyright 2015-2021 Rik Essenius
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
using System.Runtime.Versioning;
using Microsoft.Win32;
using static System.Globalization.CultureInfo;

namespace SeleniumFixture.Model
{
    internal class Zone : IZone
    {
        private const int Enabled = 0; // Disabled = 3; not necessary right now
        public const int MaxValue = 4;
        public const int MinValue = 1;
        private const string ProtectedModeKeyName = "2500";

        private const string ZoneSubKey = "Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Zones\\{0}";

        private readonly Dictionary<string, string> _baseKeys = new()
        {
            { "Machine Policies", "HKLM\\SOFTWARE\\Policies\\{0}" },
            { "User Policies", "HKCU\\SOFTWARE\\Policies\\{0}" },
            { "User", "HKCU\\SOFTWARE\\{0}" },
            { "Machine", "HKLM\\SOFTWARE\\{0}" }
        };

        private string _foundIn;
        private bool? _isProtected;

        public Zone(int zoneId, RegistryKey hklm, RegistryKey hkcu)
        {
            Id = zoneId;
            Hklm = hklm;
            Hkcu = hkcu;
        }

        private RegistryKey Hkcu { get; }
        private RegistryKey Hklm { get; }

        public string FoundIn
        {
            get
            {
                if (_foundIn == null) RetrieveProtectedValue();
                return _foundIn;
            }
        }

        public int Id { get; }

        public bool IsProtected
        {
            get
            {
                if (_isProtected == null) RetrieveProtectedValue();
                Debug.Assert(_isProtected != null, nameof(_isProtected) + " != null");
                return (bool)_isProtected;
            }
        }

        [SupportedOSPlatform("windows")]
        private object GetZoneValueFrom(string keyString)
        {
            var rootKey = RootKeyOf(keyString);
            // the Substring works because both HKLM and HKCU are 4 characters
            var registryKey = rootKey.OpenSubKey(keyString[5..], false);
            return registryKey?.GetValue(ProtectedModeKeyName);
        }

        public bool? IsProtectedIn(string registryLocation)
        {
            if (!OperatingSystem.IsWindows()) return false;
            if (!_baseKeys.ContainsKey(registryLocation))
            {
                throw new ArgumentException(ErrorMessages.RegistryLocationIssue);
            }
            var subKey = string.Format(InvariantCulture, ZoneSubKey, Id);
            var keyString = string.Format(InvariantCulture, _baseKeys[registryLocation], subKey);
            var zoneValue = GetZoneValueFrom(keyString);
            if (zoneValue == null) return null;
            return Convert.ToInt32(zoneValue, InvariantCulture) == Enabled;
        }

        private void RetrieveProtectedValue()
        {
            if (!OperatingSystem.IsWindows())
            {
                _isProtected = false;
                return;
            }
            _foundIn = string.Empty;
            foreach (var key in _baseKeys.Keys)
            {
                var isProtected = IsProtectedIn(key);
                if (isProtected == null) continue;
                _foundIn = key;
                _isProtected = (bool)isProtected;
                return;
            }
            // no registry setting found; default is 'protected'
            _isProtected = true;
        }

        private RegistryKey RootKeyOf(string keyString) => keyString.StartsWith(@"HKLM", StringComparison.Ordinal) ? Hklm : Hkcu;
    }
}

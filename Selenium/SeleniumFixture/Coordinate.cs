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
using static System.Globalization.CultureInfo;
using static System.FormattableString;

namespace SeleniumFixture
{
    /// <summary>Location coordinates or size of rectangle</summary>
    public class Coordinate
    {
        /// <summary>Class for location or rectangle size</summary>
        /// <param name="x">x coordinate (horizontal)</param>
        /// <param name="y">y coordinate (vertical)</param>
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Initialize class via a string x,y</summary>
        /// <param name="input">x,y pair with both x and y int</param>
        public Coordinate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                X = 0;
                Y = 0;
                return;
            }
            var list = input.Split(',', 'x');
            if (list.Length != 2) throw new ArgumentException(ErrorMessages.CoordinateIsNoPair);
            X = Convert.ToInt32(list[0], InvariantCulture);
            Y = Convert.ToInt32(list[1], InvariantCulture);
        }

        /// <summary>the X (horizontal) value</summary>
        public int X { get; }

        /// <summary>the Y (vertical) value</summary>
        public int Y { get; }

        /// <summary>Used for size calculation. E.g. Chrome is not always precise with window sizing.</summary>
        public bool CloseTo(Coordinate comparison) =>
            comparison != null && Math.Abs(X - comparison.X) <= 2 && Math.Abs(Y - comparison.Y) <= 2;

        /// <returns>whether the object values are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var p = (Coordinate)obj;
            return X == p.X && Y == p.Y;
        }

        /// <returns>the hash code for the coordinate</returns>
        public override int GetHashCode() => Tuple.Create(X, Y).GetHashCode();

        /// <summary>Parse a string into a coordinate. Expected format: x,y (with both x and y int)</summary>
        public static Coordinate Parse(string input) => new(input);

        /// <returns>a string representation of the coordinate: x,y</returns>
        public override string ToString() => Invariant($"{X}, {Y}");
    }
}

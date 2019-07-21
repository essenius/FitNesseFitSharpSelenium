using System;
using static System.Globalization.CultureInfo;
using static System.FormattableString;

namespace SeleniumFixture
{
    public class Coordinate
    {
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                X = 0;
                Y = 0;
                return;
            }
            var list = input.Split(',');
            if (list.Length != 2) throw new ArgumentException(ErrorMessages.CoordinateIsNoPair);
            X = Convert.ToInt32(list[0], InvariantCulture);
            Y = Convert.ToInt32(list[1], InvariantCulture);
        }

        public int X { get; }
        public int Y { get; }

        // used for size calculation. Chrome is not always precise.
        public bool CloseTo(Coordinate comparison) => 
            comparison != null && Math.Abs(X - comparison.X) <= 2 && Math.Abs(Y - comparison.Y) <= 2;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var p = (Coordinate)obj;
            return X == p.X && Y == p.Y;
        }

        public override int GetHashCode() => Tuple.Create(X, Y).GetHashCode();

        public static Coordinate Parse(string input) => new Coordinate(input);
        public override string ToString() => Invariant($"{X}, {Y}");
    }
}
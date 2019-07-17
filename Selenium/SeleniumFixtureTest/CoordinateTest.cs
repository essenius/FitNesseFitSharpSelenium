using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture;

namespace SeleniumFixtureTest
{
    [TestClass]
    public class CoordinateTest
    {
        [TestMethod, TestCategory("Unit")]
        public void CoordinateParseTest()
        {
            var coordinate = Coordinate.Parse(string.Empty);
            Assert.AreEqual(0, coordinate.X, "X coordinate is 0");
            Assert.AreEqual(0, coordinate.Y, "Y coordinate is 0");
            var hash = coordinate.GetHashCode();
            Assert.AreEqual("0, 0", coordinate.ToString(), "ToString is 0, 0");
            Assert.IsFalse(coordinate.Equals(null), "coordinate is not null");
            coordinate = Coordinate.Parse(" 23 , 45 ");
            Assert.AreNotEqual(hash, coordinate.GetHashCode(), "Hashes of different coordinates are not equal");
            Assert.IsTrue(new Coordinate(23, 45).Equals(coordinate), "Coordinate parsed right");
            Assert.IsFalse(new Coordinate(23, 44).Equals(coordinate), "wrong Y coordinate");
            Assert.IsFalse(new Coordinate(24, 45).Equals(coordinate), "wrong X coordinate");
            Assert.IsFalse(new Coordinate(24, 44).Equals(coordinate), "wrong X and Y coordinate");
            Assert.AreEqual(new Coordinate(23, 45).GetHashCode(), coordinate.GetHashCode(), "Hash codes match");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void CoordinateInvalidParseTest()
        {
            var _ = new Coordinate("bogus");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(FormatException))]
        public void CoordinateNonNUmmericCoordinateTest()
        {
            var _ = new Coordinate("bogus,0");
        }

        [TestMethod, TestCategory("Unit")]
        public void CoordinateCloseToTest()
        {
            var reference= new Coordinate(25,35);
            Assert.IsTrue(reference.CloseTo(new Coordinate(23, 37)), "Close on both X and Y");
            Assert.IsFalse(reference.CloseTo(new Coordinate(22, 33)), "Close on Y only");
            Assert.IsFalse(reference.CloseTo(new Coordinate(27, 38)), "Close on X only");
            Assert.IsFalse(reference.CloseTo(new Coordinate(28, 32)), "Not Close");
        }

    }
}
